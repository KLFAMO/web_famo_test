from django.core.management.base import BaseCommand, CommandError
from django.conf import settings
from django.db import transaction
from devices.models import IpAssignment
import getpass, paramiko, os, re, json

RE_HOST_BLOCK = re.compile(r'host\s+(?P<name>[^\s{]+)\s*\{(?P<body>[^}]*)\}', re.I | re.S)
RE_HW_ETH     = re.compile(r'hardware\s+ethernet\s+(?P<mac>[0-9A-Fa-f:\-]{12,17})\s*;', re.I)
RE_FIXED_ADDR = re.compile(r'fixed-address\s+(?P<ip>(?:\d{1,3}\.){3}\d{1,3})\s*;', re.I)
RE_HOSTNAME   = re.compile(r'(?:option\s+host-name|ddns-hostname)\s+"(?P<hn>[^"]+)"\s*;', re.I)


class Command(BaseCommand):
    help = 'Fetch DHCP configuration file from remote server'

    def add_arguments(self, parser):
        parser.add_argument("--strict-host-key", action="store_true",
                            help="If set, require known_hosts verification instead of auto-accept.")
    
    def handle(self, *args, **opts):
        host = getattr(settings, "DHCP_HOST", None)
        user = getattr(settings, "DHCP_USER", None)
        port = int(getattr(settings, "DHCP_PORT", 22))
        remote_path = getattr(settings, "DHCP_PATH", "/etc/dhcp/dhcpd.conf")
        key_path = getattr(settings, "DHCP_KEY", None)
        strict = bool(opts.get("strict_host_key"))

        if not host or not user:
            raise CommandError("Set DHCP_HOST and DHCP_USER in .env / settings.py.")

        ssh = paramiko.SSHClient()
        if strict:
            ssh.load_system_host_keys()
            ssh.set_missing_host_key_policy(paramiko.RejectPolicy())
        else:
            ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())

        try:
            if key_path and os.path.exists(key_path):
                # login using SSH key
                ssh.connect(hostname=host, port=port, username=user, key_filename=key_path, timeout=10)
            else:
                # ask for password (no echo)
                password = getpass.getpass(f"Password for {user}@{host}: ")
                ssh.connect(hostname=host, port=port, username=user, password=password, timeout=10)

            sftp = ssh.open_sftp()
            try:
                with sftp.file(remote_path, mode="r") as f:
                    data = f.read().decode("utf-8", errors="ignore")
            finally:
                try:
                    sftp.close()
                except Exception:
                    pass
        except Exception as e:
            raise CommandError(f"Download not successful {remote_path} from {host}: {e}") from e
        finally:
            try:
                ssh.close()
            except Exception:
                pass

        devices = self.parse_dhcp_config(data)
        result = self.update_ipassignments(devices)
        self.stdout.write(self.style.SUCCESS(f"DHCP sync completed: {json.dumps(result)}"))
        

    def parse_dhcp_config(self, data: str):
        """Parse DHCP configuration data and extract device info.
        Returns list of dicts with keys: name, mac, ip, hostname (if available).
        """
        devices = []
        for match in RE_HOST_BLOCK.finditer(data):
            name = match.group("name").strip()
            body = match.group("body")

            mac_match = RE_HW_ETH.search(body)
            ip_match = RE_FIXED_ADDR.search(body)
            hn_match = RE_HOSTNAME.search(body)

            mac = mac_match.group("mac").strip() if mac_match else None
            ip = ip_match.group("ip").strip() if ip_match else None
            hn = hn_match.group("hn").strip() if hn_match else None

            devices.append({
                "name": name,
                "mac": mac,
                "ip": ip,
                "hostname": hn,
            })
        
        return devices

    def update_ipassignments(self, devices):
        """
        Sync IpAssignment table to exactly match the DHCP config
        for network_type='FAMO' and kind='DHCP'.

        Rules:
        - Create if MAC from DHCP doesn't exist in DB.
        - Update if the MAC exists but IP or hostname changed.
        - Delete DB rows that are not present in DHCP anymore.
        - Ignore everything that is not (FAMO, DHCP).
        """
        NETWORK = "FAMO"
        KIND = "DHCP"

        def normalize_mac(mac: str) -> str:
            mac = (mac or "").strip().lower().replace("-", ":")
            if ":" not in mac and len(mac) == 12:
                mac = ":".join(mac[i:i+2] for i in range(0, 12, 2))
            return mac

        # Build authoritative map from DHCP: mac -> {ip, hostname}
        dhcp_map = {}
        skipped = 0
        for d in devices:
            mac = normalize_mac(d.get("mac") or "")
            ip = (d.get("ip") or "").strip()
            if not mac or not ip:
                skipped += 1
                continue
            hostname = (d.get("hostname") or d.get("name") or "").strip()
            dhcp_map[mac] = {"ip": ip, "hostname": hostname}

        created = updated = deleted = hostname_updates = 0

        with transaction.atomic():
            # Lock current FAMO/DHCP rows
            existing_qs = IpAssignment.objects.select_for_update().filter(
                network_type=NETWORK,
                kind=KIND,
            )
            existing_by_mac = {ia.mac_addr.lower(): ia for ia in existing_qs}

            # Upsert based on DHCP
            for mac, payload in dhcp_map.items():
                ia = existing_by_mac.get(mac)
                if ia is None:
                    IpAssignment.objects.create(
                        kind=KIND,
                        network_type=NETWORK,
                        mac_addr=mac,
                        ip_addr=payload["ip"],
                        hostname=payload["hostname"],
                        active=True,
                    )
                    created += 1
                else:
                    fields = []
                    if ia.ip_addr != payload["ip"]:
                        ia.ip_addr = payload["ip"]
                        fields.append("ip_addr")
                        updated += 1
                    if payload["hostname"] and ia.hostname != payload["hostname"]:
                        ia.hostname = payload["hostname"]
                        fields.append("hostname")
                        hostname_updates += 1
                    if fields:
                        ia.save(update_fields=fields)

            # Delete rows no longer present in DHCP
            dhcp_macs = set(dhcp_map.keys())
            to_delete_ids = [ia.id for mac, ia in existing_by_mac.items() if mac not in dhcp_macs]
            if to_delete_ids:
                IpAssignment.objects.filter(id__in=to_delete_ids).delete()
                deleted = len(to_delete_ids)

        return {
            "created": created,
            "updated": updated,
            "hostname_updates": hostname_updates,
            "deleted": deleted,
            "skipped_incomplete": skipped,
            "total_after": IpAssignment.objects.filter(network_type=NETWORK, kind=KIND).count(),
        }