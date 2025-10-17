from django.db import models
from django.core.validators import RegexValidator
from django.core.exceptions import ValidationError
from django.db.models import Q, F
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


class ElementType(models.Model):
    """
    Element type, for instance: "Thorlabs Lens AC254-100-A", "Newport Mirror 10D20ER.2", "Coherent Verdi V10"
    Defines common properties for a type of elements.
    """
    name = models.CharField(max_length=160)
    vendor = models.CharField(max_length=120, blank=True)
    model = models.CharField(max_length=120, blank=True)
    properties_template = models.JSONField(default=dict, blank=True)
    notes = models.TextField(blank=True)

    class Meta:
        unique_together = ("vendor", "model", "name")

    def __str__(self):
        base = f"{self.name} {self.vendor} {self.model}".strip()
        return base
    

class Location(models.Model):
    """
    Physical location of an element (device, component, part).
    """
    name = models.CharField(max_length=50, unique=True)
    parent = models.ForeignKey(
        "self", null=True, blank=True, on_delete=models.PROTECT, related_name="children"
    )
    description = models.CharField(max_length=50, blank=True)

    def __str__(self):
        return self.name


class Element(models.Model):
    """
    An instance of an element (device, component, part).
    Each element has a type (ElementType) defining its common properties.
    """
    name = models.CharField(max_length=120, unique=True)
    element_type = models.ForeignKey(ElementType, on_delete=models.PROTECT, related_name="elements")
    serial = models.CharField(max_length=120, blank=True)
    location = models.ForeignKey(Location, null=True, blank=True, on_delete=models.PROTECT, related_name="elements")
    description = models.TextField(blank=True)
    properties = models.JSONField(default=dict, blank=True)      # np. {"f_mm": 100, "AR_nm": 689, "ip":"10.0.0.12"}

    parent = models.ForeignKey("self", null=True, blank=True, on_delete=models.CASCADE, related_name="children")
    is_module = models.BooleanField(default=False)

    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    class Meta:
        indexes = [models.Index(fields=["is_module"])]

    def __str__(self):
        return self.name


MAC_VALIDATOR = RegexValidator(
    regex=r"^[0-9A-Fa-f]{2}(:[0-9A-Fa-f]{2}){5}$",
    message="MAC w formacie XX:XX:XX:XX:XX:XX"
)


class NetworkInterface(models.Model):
    """
    Network interface associated with a device.
    Each device can have multiple network interfaces (e.g., FAMO and IF networks).
    """
    element = models.ForeignKey(Element, on_delete=models.CASCADE, related_name="network_interfaces", verbose_name="Element")
    mac_addr = models.CharField(max_length=17, validators=[MAC_VALIDATOR], unique=True)
    network_type = models.CharField(max_length=4, choices=NETWORK_CHOICES, verbose_name="Network Type")
    description = models.CharField(max_length=120, blank=True, verbose_name="Description")
    active = models.BooleanField(default=True)

    def __str__(self):
        return f"{self.element} | {self.mac_addr} ({self.network_type})"


class IpAssignment(models.Model):
    """
    Assignment IP:
    - STATIC: static IP
    - DHCP: reservation in DHCP (based on config in dhcp)
    """
    KIND_CHOICES = [
        ("STATIC", "Static"),
        ("DHCP", "DHCP"),
    ]

    kind = models.CharField(max_length=10, choices=KIND_CHOICES)
    network_type = models.CharField(max_length=4, choices=NETWORK_CHOICES)

    mac_addr = models.CharField(max_length=17, validators=[MAC_VALIDATOR], db_index=True)
    ip_addr = models.GenericIPAddressField(protocol="IPv4", db_index=True)
    hostname = models.CharField(max_length=63, blank=True)

    # soft connection by MAC
    interface = models.ForeignKey(
        NetworkInterface,
        to_field="mac_addr",
        db_constraint=False,
        on_delete=models.SET_NULL,
        null=True, blank=True,
        related_name="ip_assignments",
    )

    active = models.BooleanField(default=True)
    # last_seen = models.DateTimeField(auto_now=True)
    # notes = models.TextField(blank=True)

    class Meta:
        constraints = [
            models.UniqueConstraint(
                fields=["network_type","mac_addr","kind"],
                condition=Q(active=True),
                name="uq_active_by_mac_kind_network",
            ),
            models.UniqueConstraint(
                fields=["network_type","ip_addr"],
                condition=Q(active=True),
                name="uq_active_ip_per_network",
            ),
        ]
        indexes = [
            models.Index(fields=["active","network_type","mac_addr"]),
            models.Index(fields=["active","network_type","ip_addr"]),
        ]
    
    def clean(self):
        # validate that interface.network_type matches self.network_type
        if self.interface_id is not None:
            iface = self.interface  # may be already cached
            if iface and iface.network_type != self.network_type:
                raise ValidationError("interface.network_type must match IpAssignment.network_type.")

    def relink(self):
        """Spróbuj podlinkować po MAC do NetworkInterface."""
        try:
            self.interface = NetworkInterface.objects.get(mac_addr=self.mac_addr)
        except NetworkInterface.DoesNotExist:
            self.interface = None
        self.save(update_fields=["interface"])

    def __str__(self):
        tag = self.kind.lower()
        host = f" {self.hostname}" if self.hostname else ""
        return f"[{self.network_type}/{tag}] {self.ip_addr}{host} <- {self.mac_addr}"


GENDER_CHOICES = [
    ("MALE", "Male"),
    ("FEMALE", "Female"),
    ("NA", "N/A"),
]

class ConnectorType(models.Model):
    code = models.CharField(max_length=40, unique=True)      # ex. "SMA", "BNC", "FCPC"
    label = models.CharField(max_length=80)                  # descriptive name
    gender = models.CharField(
        max_length=6,
        choices=GENDER_CHOICES,
        default="NA",
    )
    notes = models.TextField(blank=True)
    properties = models.JSONField(default=dict, blank=True)

    def __str__(self):
        g = f" {self.gender}" if self.gender!="NA" else ""
        return f"{self.code}{g}"


class PortType(models.Model):
    SIGNAL_KIND = [
        ("RF","RF"),
        ("ANALOG","Analog Voltage/Current"),
        ("ETH","Ethernet"),
        ("LASER","Laser Beam"),
        ("TTL","TTL"),
        ("OTHER","Other"),
    ]
    code = models.CharField(max_length=40, unique=True)   # np. "RF_50OHM", "LASER_780NM"
    label = models.CharField(max_length=100)
    kind = models.CharField(max_length=10, choices=SIGNAL_KIND)
    properties = models.JSONField(default=dict, blank=True)

    default_connector = models.ForeignKey(
        ConnectorType, null=True, blank=True, on_delete=models.SET_NULL, related_name="default_for_types"
    )

    def __str__(self):
        return f"{self.code} ({self.kind})"


class Port(models.Model):
    DIRECTION = [("IN","In"), ("OUT","Out"), ("INOUT","In/Out")]

    element = models.ForeignKey(Element, on_delete=models.CASCADE, related_name="ports")
    name = models.CharField(max_length=60)                   # ex. "RF IN", "CH1", "Laser Out"
    number = models.PositiveIntegerField(null=True, blank=True) # port number within element (optional)
    direction = models.CharField(max_length=6, choices=DIRECTION, default="INOUT")
    port_type = models.ForeignKey(PortType, on_delete=models.PROTECT, related_name="ports")
    connector = models.ForeignKey(ConnectorType, on_delete=models.PROTECT, related_name="ports", null=True, blank=True)
    properties = models.JSONField(default=dict, blank=True)

    class Meta:
        constraints = [
            models.UniqueConstraint(fields=["element","name"], name="uq_port_element_name"),
            models.UniqueConstraint(fields=["element","number"], name="uq_port_element_number", condition=Q(number__isnull=False)),
        ]
        indexes = [models.Index(fields=["element"])]

    def __str__(self):
        num = f" #{self.number}" if self.number is not None else ""
        return f"{self.element.name}:{self.name}{num}"

    # compatibility check
    @staticmethod
    def _dirs_compatible(a, b) -> bool:
        pairs = {("OUT","IN"), ("IN","OUT"), ("INOUT","IN"), ("INOUT","OUT"), ("INOUT","INOUT")}
        return (a, b) in pairs

    def compatible_with(self, other: "Port") -> bool:
        if self.port_type.kind != other.port_type.kind:
            return False
        if not self._dirs_compatible(self.direction, other.direction):
            return False
        if self.connector_id and other.connector_id and self.connector_id != other.connector_id:
            return False
        return True


class Connection(models.Model):
    """
    Connection between two ports.
    The order of ports (a,b) is not important, i.e., connection between (a,b) is the same as (b,a).
    """
    MEDIUM = [
        ("CABLE","Cable/Connector"),
        ("FREE","Free-space"),
        ("VIRTUAL","Virtual/Logical"),
    ]
    a = models.ForeignKey(Port, on_delete=models.CASCADE, related_name="connections_a")
    b = models.ForeignKey(Port, on_delete=models.CASCADE, related_name="connections_b")
    medium = models.CharField(max_length=8, choices=MEDIUM, default="CABLE")
    active = models.BooleanField(default=True)
    cable_element = models.ForeignKey(Element, null=True, blank=True, on_delete=models.SET_NULL, related_name="as_cable_for")
    properties = models.JSONField(default=dict, blank=True)

    created_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        constraints = [
            # a != b
            models.CheckConstraint(check=~Q(a=F("b")), name="ck_conn_distinct_ports"),
            models.UniqueConstraint(fields=["a","b"], condition=Q(active=True), name="uq_active_conn_ab"),
        ]
        indexes = [models.Index(fields=["active"])]

    def clean(self):
        # Clean up order of ports
        if self.a_id and self.b_id and self.a_id > self.b_id:
            self.a_id, self.b_id = self.b_id, self.a_id
        # Validate compatibility
        if self.a_id and self.b_id and not self.a.compatible_with(self.b):
            raise ValidationError("Ports are not compatible (type/direction/connector).")

    def save(self, *args, **kwargs):
        self.clean()
        return super().save(*args, **kwargs)

    def __str__(self):
        return f"{self.a} ? {self.b} ({self.medium})"


class ExposedPort(models.Model):
    """
    Exposed port on a module element.
    Maps an alias (external name) to an internal port on a child element.
    1 exposed port per alias per module.
    1:1 mapping between exposed port and internal port.
    """
    module = models.ForeignKey(Element, on_delete=models.CASCADE, related_name="exposed_ports")
    inner_port = models.ForeignKey(Port, on_delete=models.CASCADE, related_name="exposed_as")
    alias = models.CharField(max_length=60)  # external name for the port
    order = models.PositiveIntegerField(null=True, blank=True)  # for UI

    class Meta:
        constraints = [
            models.UniqueConstraint(fields=["module","alias"], name="uq_exposed_alias_per_module"),
            models.UniqueConstraint(fields=["module","inner_port"], name="uq_exposed_unique_mapping"),
        ]
        ordering = ["module_id","order","alias"]
    
    def clean(self):
        from django.core.exceptions import ValidationError
        # 1) module must be a module
        if self.module and not self.module.is_module:
            raise ValidationError("module must have is_module=True.")
        # 2) child-only: inner_port.element.parent == module
        if self.inner_port and self.module:
            el = self.inner_port.element
            if el.parent_id != self.module_id:
                raise ValidationError("inner_port must belong to a direct child of the module.")
    
    def __str__(self):
        return f"{self.module} :: {self.alias} -> {self.inner_port}"

