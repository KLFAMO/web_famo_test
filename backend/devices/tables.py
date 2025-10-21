# tables.py
import django_tables2 as tables
from django.utils.safestring import mark_safe
from django.urls import reverse
from .models import Element, NetworkInterface, IpAssignment

class ElementTable(tables.Table):
    """Sortable table for Elements with a custom column rendering related interfaces/IPs."""
    name = tables.Column(order_by=("name",), verbose_name="Name")
    element_type = tables.Column(verbose_name="Type")
    location = tables.Column(verbose_name="Location")
    interfaces = tables.Column(empty_values=(), orderable=False, verbose_name="Interfaces / IPs")
    actions = tables.Column(empty_values=(), orderable=False)

    class Meta:
        model = Element
        template_name = "django_tables2/bootstrap.html"  # works also without Bootstrapa
        fields = ("name", "element_type", "location")    # + nasze custom kolumny
        attrs = {"class": "table table-striped table-hover table-sm"}  # simple styling

    def render_interfaces(self, record: Element):
        """
        Render a list of interfaces with active IP assignments (prefetched in the view).
        """
        # We rely on prefetch: record.ifaces -> list[NetworkInterface]
        ifaces = getattr(record, "ifaces", None) or []
        if not ifaces:
            return mark_safe('<span class="text-muted">no network interfaces</span>')

        chunks = []
        for iface in ifaces:
            ips = getattr(iface, "active_ip_assignments", None) or []
            if ips:
                ip_lines = "".join(
                    f'<li>{a.ip_addr}'
                    f'{f" <small>({a.hostname})</small>" if a.hostname else ""}'
                    f' <small class="text-muted">[{a.kind}]</small></li>'
                    for a in ips
                )
            else:
                ip_lines = '<small class="text-muted">no active IP assignments</small>'

            chunks.append(
                f'<div style="margin-bottom:.25rem;">'
                f'<strong>{iface.network_type}</strong> | MAC: {iface.mac_addr}'
                f'<ul style="margin:.25rem 0 .25rem 1rem;">{ip_lines}</ul>'
                f'</div>'
            )
        return mark_safe("".join(chunks))

    def render_actions(self, record: Element):
        """Render action links (Edit)."""
        url = reverse("element_edit", kwargs={"pk": record.pk})
        return mark_safe(f'<a href="{url}">Edit</a>')