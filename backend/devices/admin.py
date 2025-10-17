from django.contrib import admin
from .models import (
    Connection,
    ConnectorType,
    Device,
    ElementType,
    Element,
    ExposedPort,
    IpAssignment,
    Location,
    NetworkInterface,
    Port,
    PortType,
)

@admin.register(Device)
class DeviceAdmin(admin.ModelAdmin):
    list_display = ('name', 'ip_famo', 'mac_famo', 'ip_if', 'mac_if', 'device_type', 'location', 'username', 'description')
    search_fields = ('name', 'ip_famo', 'mac_famo', 'ip_if', 'mac_if', 'device_type', 'location', 'username')
    list_filter = ('device_type', 'location')
    ordering = ('numeric_ip_famo', 'ip_if')


@admin.register(IpAssignment)
class IpAssignmentAdmin(admin.ModelAdmin):
    list_display = ('mac_addr', 'ip_addr', 'hostname', 'network_type', 'kind', 'active')
    search_fields = ('mac_addr', 'ip_addr', 'hostname', 'network_type', 'kind')
    list_filter = ('network_type', 'kind', 'active')
    ordering = ('mac_addr',)

admin.site.register(ElementType)
admin.site.register(Element)
admin.site.register(PortType)
admin.site.register(Port)
admin.site.register(ConnectorType)
admin.site.register(Connection)
admin.site.register(Location)
admin.site.register(NetworkInterface)
admin.site.register(ExposedPort)
