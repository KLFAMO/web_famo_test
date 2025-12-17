# forms.py
import json
from django import forms
from django.forms import inlineformset_factory
from django.utils.text import slugify
from .models import Element, NetworkInterface, Tag, ElementTag, ElementType

class ElementForm(forms.ModelForm):
    """Main form for Element edit/create."""

    tag_names = forms.CharField(
        required=False,
        label="Tags",
        help_text="Comma-separated list of tags",
    )

    class Meta:
        model = Element
        fields = ["name", "element_type", "serial", "location", 
                  "description", "properties", "is_module", "parent"
                  ]

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        # if editing an existing Element, prepopulate tags field
        if self.instance.pk:
            current_tags = self.instance.tags.all().order_by("name")
            self.fields["tag_names"].initial = ", ".join(t.name for t in current_tags)

    def save(self, commit=True):
        """
        Standard save method overridden to handle tags saving separately.
        Actual tag saving is done in save_tags() method called from the view.
        """
        return super().save(commit=commit)
    
    def _parse_tag_names(self):
        """
        Convert raw tag names string into a list of unique, trimmed names.
        """
        raw = self.cleaned_data.get("tag_names") or ""
        parts = [p.strip() for p in raw.replace(";", ",").split(",")]
        # remove duplicates while preserving order
        seen = set()
        names = []
        for p in parts:
            if not p:
                continue
            if p.lower() in seen:
                continue
            seen.add(p.lower())
            names.append(p)
        return names

    def save_tags(self, element: Element):
        """
        Save tags for the given Element instance based on the tag_names field.
        """
        if not self.is_valid():
            return

        names = self._parse_tag_names()

        # znajdź istniejące tagi po nazwie
        existing_tags = {t.name: t for t in Tag.objects.filter(name__in=names)}
        tags_to_use = []

        for name in names:
            tag = existing_tags.get(name)
            if not tag:
                # utworzyć nowy tag
                base_slug = slugify(name) or "tag"
                slug = base_slug
                i = 2
                # zapewnij unikalność sluga
                while Tag.objects.filter(slug=slug).exists():
                    slug = f"{base_slug}-{i}"
                    i += 1
                tag = Tag.objects.create(name=name, slug=slug)
            tags_to_use.append(tag)

        # aktualizacja tabeli ElementTag: zostaw tylko tags_to_use
        ElementTag.objects.filter(element=element).exclude(tag__in=tags_to_use).delete()

        existing_ids = set(
            ElementTag.objects.filter(element=element, tag__in=tags_to_use).values_list("tag_id", flat=True)
        )

        to_create = [
            ElementTag(element=element, tag=t)
            for t in tags_to_use
            if t.id not in existing_ids
        ]
        ElementTag.objects.bulk_create(to_create)

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
    extra=1,
    can_delete=True,
    exclude=['id'],  # Wykluczamy pole id, Django sam się nim zajmie
)

class ElementTypeForm(forms.ModelForm):
    # Pole edycyjne jako tekst JSON
    properties_template_json = forms.CharField(
        required=False,
        widget=forms.Textarea(attrs={
            "rows": 28,
            "class": "form-control font-monospace",
            "spellcheck": "false",
            "style": "white-space: pre; tab-size: 2;",
        }),
        help_text="Wprowadź poprawny JSON (obiekt).",
        label="Properties template (JSON)"
    )

    class Meta:
        model = ElementType
        fields = ["name", "vendor", "model", "notes"]  # JSON obsługujemy osobno przez properties_template_json

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)

        # Inicjalizacja textarea aktualnym JSON-em z bazy
        if self.instance and self.instance.pk:
            self.fields["properties_template_json"].initial = json.dumps(
                self.instance.properties_template or {},
                indent=2,
                ensure_ascii=False,
            )
        else:
            self.fields["properties_template_json"].initial = "{}"

    def clean_properties_template_json(self):
        raw = (self.cleaned_data.get("properties_template_json") or "").strip()
        if not raw:
            return {}

        try:
            parsed = json.loads(raw)
        except json.JSONDecodeError as e:
            raise forms.ValidationError(f"Niepoprawny JSON: {e}")

        if not isinstance(parsed, dict):
            raise forms.ValidationError("JSON musi być obiektem (dict), nie listą ani wartością prostą.")

        return parsed

    def save(self, commit=True):
        obj = super().save(commit=False)

        # Przepisujemy sparsowany dict do JSONField
        obj.properties_template = self.cleaned_data.get("properties_template_json") or {}

        if commit:
            obj.save()
        return obj
