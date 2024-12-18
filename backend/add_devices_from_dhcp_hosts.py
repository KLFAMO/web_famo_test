import os
import subprocess
import ipaddress

import django

# Konfiguracja Django
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'web_famo.settings')  # Zamie? 'your_project' na nazw? swojego projektu
django.setup()

from devices.models import Device

# Konfiguracja zdalnego urz?dzenia
REMOTE_HOST = "pi@192.168.3.100"
REMOTE_FILES = {
    "/etc/hosts": "hosts",
    "/etc/dhcp/dhcpd.conf": "dhcpd.conf",
}
LOCAL_DIR = "./config_files"

def fetch_files():
    if not os.path.exists(LOCAL_DIR):
        os.makedirs(LOCAL_DIR)
    for remote_path, local_name in REMOTE_FILES.items():
        local_path = os.path.join(LOCAL_DIR, local_name)
        try:
            print(f"Pobieranie {remote_path} z {REMOTE_HOST}...")
            subprocess.run(["scp", f"{REMOTE_HOST}:{remote_path}", local_path], check=True)
            print(f"Zapisano jako {local_path}")
        except subprocess.CalledProcessError as e:
            print(f"Error during fetching {remote_path}: {e}")

# Funkcje do parsowania plik?w
def parse_hosts(file_path):
    devices = []
    with open(file_path, 'r') as file:
        for line in file:
            line = line.strip()
            if not line or line.startswith("#"):
                continue
            parts = line.split()
            ip = parts[0]
            names = parts[1:]  # Hosty mog? mie? wiele alias?w
            for name in names:
                devices.append({'ip': ip, 'name': name})
    return devices

def parse_dhcpd(file_path):
    """
    Parsuje plik /etc/dhcp/dhcpd.conf i zwraca list? powi?za? nazw host?w z adresami MAC.

    Args:
        file_path (str): ?cie?ka do pliku dhcpd.conf.

    Returns:
        list: Lista s?ownik?w z polami `name` i `mac`.
    """
    devices = []
    with open(file_path, 'r') as file:
        lines = file.readlines()

    current_host = None
    current_mac = None

    for line in lines:
        line = line.strip()
        if line.startswith("host"):
            # Zaczynamy nowy blok dla hosta
            current_host = line.split()[1]  # Nazwa hosta
            current_mac = None
        elif line.startswith("hardware ethernet"):
            # Pobieramy adres MAC
            current_mac = line.split()[-1].strip(";")
        elif line.startswith("fixed-address") and current_host:
            # Ko?czymy blok hosta
            devices.append({'name': current_host, 'mac': current_mac})
            current_host = None
            current_mac = None

    return devices


def merge_device_lists(ip_list, mac_list):
    """
    Merge lists: (name -> ip) and (name -> mac) into one list (name -> ip -> mac).

    Args:
        ip_list (list): List of dictionaries with fields `name` and `ip`.
        mac_list (list): Lista s?ownik?w z polami `name` i `mac`.

    Returns:
        list: Po??czona lista s?ownik?w z polami `name`, `ip`, i `mac`.
    """
    # Zamiana mac_list na s?ownik {name: mac} dla szybszego dost?pu
    mac_dict = {item['name']: item['mac'] for item in mac_list}

    # ??czenie na podstawie pola `name`
    merged_list = []
    for ip_entry in ip_list:
        name = ip_entry['name']
        ip = ip_entry['ip']
        mac = mac_dict.get(name)  # Pobierz MAC, je?li istnieje
        merged_list.append({'name': name, 'ip': ip, 'mac': mac})

    return merged_list


def add_devices_to_database(device_list):
    """
    Dodaje urz?dzenia do bazy danych na podstawie po??czonej listy, ignoruj?c nieprawid?owe adresy IP.

    Args:
        device_list (list): Lista s?ownik?w z polami `name`, `ip`, i `mac`.
    """
    for device in device_list:
        ip = device['ip']
        try:
            # Sprawdzanie poprawno?ci adresu IP
            parsed_ip = ipaddress.ip_address(ip)
            if parsed_ip.version != 4:  # Ignorujemy adresy IPv6
                print(f"Ignored IPv6 or unsupported IP: {ip} for device {device['name']}")
                continue
        except ValueError:
            print(f"Ignored invalid IP: {ip} for device {device['name']}")
            continue

        # Dodanie urz?dzenia do bazy danych
        device_obj, created = Device.objects.update_or_create(
            name=device['name'],  # Wyszukiwanie na podstawie `name`
            defaults={
                'ip_famo': ip,
                'mac_famo': device['mac']
            }
        )
        if created:
            print(f"Added: {device_obj.name} (IP: {device_obj.ip_famo}, MAC: {device_obj.mac_famo})")
        else:
            print(f"Updated: {device_obj.name} (IP: {device_obj.ip_famo}, MAC: {device_obj.mac_famo})")


# G??wna logika
if __name__ == "__main__":
    # Pobranie plik?w ze zdalnego urz?dzenia
    # fetch_files()

    # Parsowanie plik?w
    hosts_devices = parse_hosts(os.path.join(LOCAL_DIR, "hosts"))
    # print("\nParsed /etc/hosts:")
    # for device in hosts_devices:
    #     print(device)

    dhcp_devices = parse_dhcpd(os.path.join(LOCAL_DIR, "dhcpd.conf"))
    # print("\nParsed /etc/dhcp/dhcpd.conf:")
    # for device in dhcp_devices:
    #     print(device)
    
    merged_list = merge_device_lists(hosts_devices, dhcp_devices)
    for device in merged_list:
        print(device)

    add_devices_to_database(merged_list)

    # ??czenie danych i eliminacja duplikat?w
    # all_devices = {f"{d['ip']}|{d['name']}": d for d in (hosts_devices + dhcp_devices)}
    # unique_devices = list(all_devices.values())

    # Dodanie urz?dze? do bazy
    # print("\nAdding devices to database...")
    # add_devices_to_db(unique_devices)
