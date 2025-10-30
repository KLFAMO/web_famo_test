# forms.py
from django import forms
from django.forms import inlineformset_factory
from .models import Element, NetworkInterface

class ElementForm(forms.ModelForm):
    """Main form for Element edit/create."""
    class Meta:
        model = Element
        fields = ["name", "element_type", "serial", "location", 
                  "description", "properties", "is_module", "parent"
                  ]

class NetworkInterfaceForm(forms.ModelForm):
    """Inline form for related NetworkInterface objects."""
    class Meta:
        model = NetworkInterface
        fields = ["network_type", "mac_addr", "description", "active"]

NetworkInterfaceFormSet = inlineformset_factory(
    parent_model=Element,
    model=NetworkInterface,
    # form=NetworkInterfaceForm,
    fields=("network_type", "mac_addr", "description", "active"),
    extra=0,
    can_delete=True,
    exclude=['id'],  # Wykluczamy pole id, Django sam się nim zajmie
)
