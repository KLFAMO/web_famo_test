# your_app/management/commands/migrate_devices_to_elements.py
from django.core.management.base import BaseCommand
from django.db import transaction
from django.db.models import Q
from devices.models import Element, ElementType, Location, NetworkInterface, Device
import re

NETWORK_FAMO = "FAMO"
NETWORK_IF = "IF"

def normalize_mac(mac: str | None) -> str | None:
    """Return MAC as 'AA:BB:CC:DD:EE:FF' or None."""
    if not mac:
        return None
    s = mac.strip().upper().replace("-", ":")
    if ":" not in s:
        if len(s) == 12 and re.fullmatch(r"[0-9A-F]{12}", s):
            s = ":".join(s[i:i+2] for i in range(0, 12, 2))
    s = ":".join(part.zfill(2) for part in s.split(":") if part != "")
    return s

class Command(BaseCommand):
    help = "Migrate devices.Device rows into elements.Element (+ NetworkInterface for FAMO/IF)."

    def add_arguments(self, parser):
        parser.add_argument("--commit", action="store_true",
                            help="Actually write changes to the database (default: dry-run with rollback).")
        parser.add_argument("--default-type-name", default="Legacy Device",
                            help="ElementType.name used when device_type is empty.")

    def handle(self, *args, **opts):
        commit = opts["commit"]
        default_type_name = opts["default_type_name"]

        self.stdout.write(self.style.NOTICE(
            f"Starting migration Device → Element (commit={'YES' if commit else 'NO, dry-run'})"
        ))

        default_et, _ = ElementType.objects.get_or_create(
            name=default_type_name,
            defaults={"vendor": "", "model": "", "properties_template": {}, "notes": "Auto-created during migration"},
        )

        # Zbierz istniejące interfejsy (TYLKO z elements.NetworkInterface), z przypiętym elementem.
        # Mapujemy: MAC -> (element_id, element_name)
        existing_mac_to_el = {
            row["mac_addr"]: (row["element_id"], row["element__name"])
            for row in NetworkInterface.objects.select_related("element")
            .values("mac_addr", "element_id", "element__name")
        }

        total = Device.objects.count()
        created_elements = 0
        linked_interfaces = 0
        skipped_existing_el = 0      # element o tej nazwie już był
        skipped_by_mac_on_element = 0
        warnings = 0

        with transaction.atomic():
            for d in Device.objects.all().order_by("id"):
                mac_famo = normalize_mac(getattr(d, "mac_famo", None))
                mac_if = normalize_mac(getattr(d, "mac_if", None))
                macs_to_check = {m for m in (mac_famo, mac_if) if m}

                # >>> GŁÓWNY WARUNEK POMINIĘCIA <<<
                # Pomiń TYLKO jeśli MAC już należy do jakiegoś Elementu poprzez NetworkInterface.
                # (czyli istnieje NI z tym MAC i ma przypięty element)
                already_on_element = [(m, existing_mac_to_el[m]) for m in macs_to_check if m in existing_mac_to_el]
                if already_on_element:
                    skipped_by_mac_on_element += 1
                    details = ", ".join(
                        f"{m}→Element(id={eid}, name='{ename}')"
                        for m, (eid, ename) in already_on_element
                    )
                    self.stdout.write(self.style.WARNING(
                        f"[SKIP] Device(id={d.id}, name='{d.name}') skipped; MAC already on Element: {details}"
                    ))
                    continue

                # Wyznacz ElementType
                et_name = (d.device_type or "").strip()
                if et_name:
                    et, _ = ElementType.objects.get_or_create(
                        name=et_name,
                        defaults={"vendor": "", "model": "", "properties_template": {}, "notes": "From legacy device_type"},
                    )
                else:
                    et = default_et

                # Lokalizacja
                loc = None
                if d.location:
                    loc, _ = Location.objects.get_or_create(
                        name=d.location.strip(),
                        defaults={"description": "Auto-created from legacy Device.location"},
                    )

                # Opis i properties (IP tylko jako ślad w properties)
                desc_parts = []
                if d.description: desc_parts.append(d.description.strip())
                if d.comment: desc_parts.append(f"(legacy comment) {d.comment.strip()}")
                if d.username: desc_parts.append(f"(legacy username) {d.username.strip()}")
                description = "\n".join(desc_parts)

                properties = {
                    "legacy_device_id": d.id,
                    "legacy_source": "devices.Device",
                }
                if d.ip_famo: properties["legacy_ip_famo"] = d.ip_famo
                if getattr(d, "ip_if", None): properties["legacy_ip_if"] = d.ip_if

                # Utwórz/pobierz Element po unikalnej nazwie
                el, el_created = Element.objects.get_or_create(
                    name=d.name.strip(),
                    defaults={
                        "element_type": et,
                        "serial": "",
                        "location": loc,
                        "description": description,
                        "properties": properties,
                        "parent": None,
                        "is_module": False,
                    },
                )

                if not el_created:
                    need_save = False
                    if el.element_type_id != et.id:
                        el.element_type = et; need_save = True
                    if el.location_id != (loc.id if loc else None):
                        el.location = loc; need_save = True
                    if description and description not in (el.description or ""):
                        el.description = (el.description + "\n" if el.description else "") + description
                        need_save = True
                    # merge properties bez nadpisywania istniejących kluczy
                    merged = dict(el.properties or {})
                    for k, v in properties.items():
                        merged.setdefault(k, v)
                    if merged != el.properties:
                        el.properties = merged; need_save = True
                    if need_save:
                        el.save(update_fields=["element_type","location","description","properties"])
                    skipped_existing_el += 1
                else:
                    created_elements += 1

                # Twórz interfejsy (skoro nie było ich wcześniej na żadnym elemencie)
                if mac_famo:
                    ni, ni_created = NetworkInterface.objects.get_or_create(
                        mac_addr=mac_famo,
                        defaults={
                            "element": el,
                            "network_type": NETWORK_FAMO,
                            "description": "Imported from legacy devices",
                            "active": True,
                        },
                    )
                    if not ni_created and ni.element_id != el.id:
                        warnings += 1
                        self.stdout.write(self.style.WARNING(
                            f"[WARN] MAC {mac_famo} already assigned to Element(id={ni.element_id}); skip reassignment."
                        ))
                    else:
                        linked_interfaces += 1
                        existing_mac_to_el[mac_famo] = (el.id, el.name)

                if mac_if:
                    ni, ni_created = NetworkInterface.objects.get_or_create(
                        mac_addr=mac_if,
                        defaults={
                            "element": el,
                            "network_type": NETWORK_IF,
                            "description": "Imported from legacy devices",
                            "active": True,
                        },
                    )
                    if not ni_created and ni.element_id != el.id:
                        warnings += 1
                        self.stdout.write(self.style.WARNING(
                            f"[WARN] MAC {mac_if} already assigned to Element(id={ni.element_id}); skip reassignment."
                        ))
                    else:
                        linked_interfaces += 1
                        existing_mac_to_el[mac_if] = (el.id, el.name)

            if not commit:
                self.stdout.write(self.style.NOTICE("Dry-run mode: rolling back all DB changes."))
                transaction.set_rollback(True)

        self.stdout.write(self.style.SUCCESS(
            "Done. Devices scanned: {} | Elements created: {} | Elements preexisting: {} | "
            "Interfaces linked/created: {} | Skipped by MAC already on Element: {} | Warnings: {}".format(
                total, created_elements, skipped_existing_el, linked_interfaces, skipped_by_mac_on_element, warnings
            )
        ))
