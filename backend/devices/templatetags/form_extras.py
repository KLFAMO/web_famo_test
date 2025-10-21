from django import template
register = template.Library()

@register.filter
def add_class(field, css):
    """Add CSS class to a form field widget dynamically."""
    return field.as_widget(attrs={**field.field.widget.attrs, "class": css})
