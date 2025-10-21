# management/commands/relink_ipassignments.py
from django.core.management.base import BaseCommand
from devices.models import IpAssignment

class Command(BaseCommand):
    help = "Relink IpAssignments.interface by MAC"

    def handle(self, *args, **opts):
        n = 0
        for a in IpAssignment.objects.all().iterator():
            old = a.interface_id
            a.relink()
            if a.interface_id != old:
                n += 1
        self.stdout.write(self.style.SUCCESS(f"Relinked: {n}"))
