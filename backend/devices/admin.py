from django.contrib import admin
from .models import (
    Connection,
    ConnectorType,
    Device,
    ElementTag,
    ElementType,
    Element,
    ExposedPort,
    IpAssignment,
    Location,
    NetworkInterface,
    Port,
    PortType,
    Tag,
)

class TagAdmin(admin.ModelAdmin):
    list_display = ["name", "slug", "color"]
    search_fields = ["name", "slug"]   # << to jest wymagane dla autocomplete_fields
    ordering = ["name"]

class ElementTagInline(admin.TabularInline):
    model = ElementTag
    extra = 1  # number of extra forms to display
    autocomplete_fields = ["tag"]

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

@admin.register(Element)
class ElementAdmin(admin.ModelAdmin):
    list_display = ["name", "element_type", "location"]
    inlines = [ElementTagInline]
    search_fields = ["name"]

admin.site.register(ElementType)
admin.site.register(Tag, TagAdmin)
admin.site.register(ElementTag)
admin.site.register(PortType)
admin.site.register(Port)
admin.site.register(ConnectorType)
admin.site.register(Connection)
admin.site.register(Location)
admin.site.register(NetworkInterface)
admin.site.register(ExposedPort)
