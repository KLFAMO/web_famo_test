from django.contrib import admin
from .models import Device

@admin.register(Device)
class DeviceAdmin(admin.ModelAdmin):
    list_display = ('name', 'ip_famo', 'mac_famo', 'ip_if', 'mac_if', 'device_type', 'location', 'username', 'description')
    search_fields = ('name', 'ip_famo', 'mac_famo', 'ip_if', 'mac_if', 'device_type', 'location', 'username')
    list_filter = ('device_type', 'location')
    ordering = ('numeric_ip_famo', 'ip_if')
