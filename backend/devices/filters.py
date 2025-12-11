# filters.py
import django_filters as df
from django.db.models import Q
from .models import Element, ElementType, Location, Tag


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
    tags = df.CharFilter(
        method="filter_tags",
        label="Tags (comma-separated)",
    )

    class Meta:
        model = Element
        fields = ["element_type", "location", "network_type", "tags"]

    def filter_q(self, queryset, name, value):
        """Search by name OR serial."""
        if not value:
            return queryset
        return queryset.filter(Q(name__icontains=value) | Q(serial__icontains=value))

    def filter_tags(self, queryset, name, value):
        """
        Filter by tags (comma-separated). Only elements having ALL specified tags are returned.
        """
        if not value:
            return queryset

        tags_raw = [t.strip() for t in value.split(",") if t.strip()]
        if not tags_raw:
            return queryset

        # Get Tag objects for the given names
        tag_qs = Tag.objects.filter(name__in=tags_raw)

        # Each tag must be present
        for tag in tag_qs:
            queryset = queryset.filter(tags=tag)

        return queryset.distinct()
