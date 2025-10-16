from django.db import models
import struct
import socket


NETWORK_CHOICES = [
        ('FAMO', 'FAMO Network'),
        ('IF', 'Institute Network'),
    ]


class Device(models.Model):

    name = models.CharField(max_length=100, verbose_name="Device Name")
    device_type = models.CharField(max_length=50, verbose_name="Device Type", blank=True, null=True)
    location = models.CharField(max_length=100, verbose_name="Location", blank=True, null=True)
    description = models.TextField(verbose_name="Description", blank=True, null=True)
    username = models.CharField(max_length=100, verbose_name="Username", blank=True, null=True)
    comment = models.TextField(verbose_name="Additional Comment", blank=True, null=True)

    # to be removed in future
    ip_famo = models.GenericIPAddressField(verbose_name="FAMO Network IP", blank=True, null=True)
    numeric_ip_famo = models.BigIntegerField(blank=True, null=True, editable=False)
    ip_if = models.GenericIPAddressField(verbose_name="IF Network IP", blank=True, null=True)
    numeric_ip_famo = models.BigIntegerField(blank=True, null=True, editable=False)
    mac_famo = models.CharField(max_length=17, verbose_name="FAMO Network MAC Address", blank=True, null=True)
    mac_if = models.CharField(max_length=17, verbose_name="IF Network MAC Address", blank=True, null=True)
    
    
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


# class ElementType(models.Model):
#     """
#     Element type, for instance: "Thorlabs Lens AC254-100-A", "Newport Mirror 10D20ER.2", "Coherent Verdi V10"
#     Defines common properties for a type of elements.
#     """
#     name = models.CharField(max_length=160)
#     vendor = models.CharField(max_length=120, blank=True)
#     model = models.CharField(max_length=120, blank=True)
#     spec_template = models.JSONField(default=dict, blank=True)
#     notes = models.TextField(blank=True)

#     class Meta:
#         unique_together = ("vendor", "model", "name")

#     def __str__(self):
#         base = f"{self.name} {self.vendor} {self.model}".strip()
#         return base
    

# class Element(models.Model):
#     """
#     An instance of an element (device, component, part).
#     Each element has a type (ElementType) defining its common properties.
#     """
#     name = models.CharField(max_length=120, unique=True)
#     element_type = models.ForeignKey(ElementType, on_delete=models.PROTECT, related_name="elements")
#     status = models.CharField(max_length=16, default="active")   # active/spare/broken/retired
#     serial = models.CharField(max_length=120, blank=True)
#     location_label = models.CharField(max_length=120, blank=True)  #  "Table A / Rack 1", simple description of location
#     description = models.TextField(blank=True)
#     # specyfic parameters, e.g. for a lens: {"f_mm": 100, "AR_nm": 689}
#     properties = models.JSONField(default=dict, blank=True)      # np. {"f_mm": 100, "AR_nm": 689, "ip":"10.0.0.12"}
#     tags = models.JSONField(default=list, blank=True)             #  ["689MOT","clock-path"]

#     # optional composition: an element can contain sub-elements
#     parent = models.ForeignKey("self", null=True, blank=True, on_delete=models.CASCADE, related_name="children")

#     created_at = models.DateTimeField(auto_now_add=True)
#     updated_at = models.DateTimeField(auto_now=True)

#     class Meta:
#         indexes = [models.Index(fields=["status"])]

#     def __str__(self):
#         return self.nam


# class NetworkInterface(models.Model):
#     """
#     Network interface associated with a device.
#     Each device can have multiple network interfaces (e.g., FAMO and IF networks).
#     """
#     device = models.ForeignKey(Device, on_delete=models.CASCADE, related_name="network_interfaces", verbose_name="Device")
#     name = models.CharField(max_length=100, verbose_name="Interface Name")
#     mac_addr = models.CharField(max_length=17, verbose_name="MAC Address")
#     enabled = models.BooleanField(default=True, verbose_name="Enabled")
#     network_type = models.CharField(max_length=4, choices=NETWORK_CHOICES, verbose_name="Network Type")

#     def __str__(self):
#         return f"{self.name} ({'Enabled' if self.enabled else 'Disabled'})"


# class FamoDHCP(models.Model):

#     mac_addr = models.OneToOneField(
#         NetworkInterface,
#         on_delete=models.CASCADE,
#         primary_key=True,
#         to_field="mac_addr",
#         verbose_name="MAC Address"
#     )
#     ip = models.GenericIPAddressField(verbose_name="IP Address")
#     hostname = models.CharField(max_length=255, verbose_name="Hostname")
#     active = models.BooleanField(default=True, verbose_name="Active")

#     def __str__(self):
#         return f"{self.hostname} ({self.ip}) - {'Active' if self.active else 'Inactive'}"


# class StaticIP(models.Model):

#     network_interface = models.OneToOneField(
#         NetworkInterface,
#         on_delete=models.CASCADE,
#         related_name="static_ip",
#         verbose_name="Network Interface"
#     )
#     ip = models.GenericIPAddressField(verbose_name="Static IP Address")
#     network_type = models.CharField(max_length=4, choices=NETWORK_CHOICES, verbose_name="Network Type")
#     description = models.TextField(blank=True, null=True, verbose_name="Description")

#     def __str__(self):
#         return f"{self.network_interface.name} - {self.ip} ({self.network_type})"
    