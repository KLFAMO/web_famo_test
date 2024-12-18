from django.db import models
import struct
import socket


class Device(models.Model):

    name = models.CharField(max_length=100, verbose_name="Device Name")
    ip_famo = models.GenericIPAddressField(verbose_name="FAMO Network IP", blank=True, null=True)
    numeric_ip_famo = models.BigIntegerField(blank=True, null=True, editable=False)
    ip_if = models.GenericIPAddressField(verbose_name="IF Network IP", blank=True, null=True)
    numeric_ip_famo = models.BigIntegerField(blank=True, null=True, editable=False)
    mac_famo = models.CharField(max_length=17, verbose_name="FAMO Network MAC Address", blank=True, null=True)
    mac_if = models.CharField(max_length=17, verbose_name="IF Network MAC Address", blank=True, null=True)
    device_type = models.CharField(max_length=50, verbose_name="Device Type", blank=True, null=True)
    location = models.CharField(max_length=100, verbose_name="Location", blank=True, null=True)
    description = models.TextField(verbose_name="Description", blank=True, null=True)
    username = models.CharField(max_length=100, verbose_name="Username", blank=True, null=True)
    comment = models.TextField(verbose_name="Additional Comment", blank=True, null=True)

    created_at = models.DateTimeField(auto_now_add=True, verbose_name="Created At")
    updated_at = models.DateTimeField(auto_now=True, verbose_name="Updated At")

    def __str__(self):
        return f"{self.name} ({self.ip_famo})"

    def save(self, *args, **kwargs):
        if self.ip_famo:
            self.numeric_ip_famo = struct.unpack("!I", socket.inet_aton(self.ip_famo))[0]
        else:
            self.numeric_ip_famo = None
        
        if self.ip_if:
            self.numeric_ip_if = struct.unpack("!I", socket.inet_aton(self.ip_if))[0]
        else:
            self.numeric_ip_if = None
        super().save(*args, **kwargs)
