from django.core.management.base import BaseCommand, CommandError
from django.conf import settings
import os
import getpass
import paramiko


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

        self.stdout.write(data)