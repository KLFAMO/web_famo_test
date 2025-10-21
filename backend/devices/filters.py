# filters.py
import django_filters as df
from django.db.models import Q
from .models import Element, ElementType, Location

class ElementFilter(df.FilterSet):
    """FilterSet for Element list."""
    q = df.CharFilter(method="filter_q", label="Search (name/serial)")
    element_type = df.ModelChoiceFilter(queryset=ElementType.objects.all(), label="Type")
    location = df.ModelChoiceFilter(queryset=Location.objects.all(), label="Location")
    network_type = df.ChoiceFilter(
        field_name="network_interfaces__network_type",
        choices=[("FAMO", "FAMO"), ("IF", "IF")],
        label="Network",
    )

    class Meta:
        model = Element
        fields = ["element_type", "location", "network_type"]

    def filter_q(self, queryset, name, value):
        """Search by name OR serial."""
        if not value:
            return queryset
        return queryset.filter(Q(name__icontains=value) | Q(serial__icontains=value))
