# devices/management/commands/fill_mac_norm.py
from django.core.management.base import BaseCommand
from devices.models import NetworkInterface, IpAssignment
from devices.utils import normalize_mac

class Command(BaseCommand):
    help = "Fill mac_norm fields for existing records"

    def handle(self, *args, **kwargs):
        for model in (NetworkInterface, IpAssignment):
            n = 0
            for obj in model.objects.all().iterator():
                norm = normalize_mac(obj.mac_addr)
                if norm and obj.mac_norm != norm:
                    obj.mac_norm = norm
                    obj.save(update_fields=["mac_norm"])
                    n += 1
            self.stdout.write(self.style.SUCCESS(f"{model.__name__}: updated {n} entries"))
