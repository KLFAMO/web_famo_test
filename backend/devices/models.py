from django.db import models


class Device(models.Model):

    name = models.CharField(max_length=100, verbose_name="Device Name")
    ip_famo = models.GenericIPAddressField(verbose_name="FAMO Network IP", blank=True, null=True)
    ip_if = models.GenericIPAddressField(verbose_name="IF Network IP", blank=True, null=True)
    mac = models.CharField(max_length=17, verbose_name="MAC Address", blank=True, null=True)
    device_type = models.CharField(max_length=50, verbose_name="Device Type", blank=True, null=True)
    location = models.CharField(max_length=100, verbose_name="Location", blank=True, null=True)
    description = models.TextField(verbose_name="Description", blank=True, null=True)
    username = models.CharField(max_length=100, verbose_name="Username", blank=True, null=True)
    comment = models.TextField(verbose_name="Additional Comment", blank=True, null=True)

    created_at = models.DateTimeField(auto_now_add=True, verbose_name="Created At")
    updated_at = models.DateTimeField(auto_now=True, verbose_name="Updated At")

    def __str__(self):
        return f"{self.name} ({self.ip_famo})"

