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
    tags = tables.Column(
        empty_values=(),
        orderable=False,
        verbose_name="Tags",
    )
    actions = tables.Column(empty_values=(), orderable=False)

    class Meta:
        model = Element
        template_name = "django_tables2/bootstrap.html"  # works also without Bootstrapa
        fields = ("name", "element_type", "location")    # + our custom columns
        sequence = ("name", "element_type", "location", "tags", "interfaces", "actions")
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

    def render_tags(self, record: Element):
        """
        Render tags associated with the Element as badges.
        """
        tags = getattr(record, "tags", None)
        if not tags:
            return mark_safe('<span class="text-muted">—</span>')

        tag_list = list(tags.all())
        if not tag_list:
            return mark_safe('<span class="text-muted">—</span>')

        html = "".join(
            f'<span class="badge bg-secondary me-1 mb-1">{t.name}</span>'
            for t in tag_list
        )
        return mark_safe(html)

    def render_actions(self, record: Element):
        edit_url = reverse("element_edit", kwargs={"pk": record.pk})
        connect_url = reverse("element_connect", kwargs={"pk": record.pk})
        return mark_safe(
            f'''
            <div class="d-flex flex-column gap-1">
                <a href="{edit_url}" class="btn btn-sm btn-primary">Edit</a>
                <a href="{connect_url}" class="btn btn-sm btn-outline-secondary">Connect</a>
            </div>
            '''
        )