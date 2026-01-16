# devices/views.py

from django.conf import settings
from django.core.management import call_command, CommandError
from django.contrib import messages
from django.db import transaction
from django.db.models import Prefetch, Q
from django.http import HttpResponse
from django.shortcuts import redirect, render
from django.utils.safestring import mark_safe
from django.urls import reverse
from django.views.decorators.http import require_POST
from django.views.generic import CreateView, UpdateView, DetailView, ListView, TemplateView
from django_filters.views import FilterView
from django_tables2.views import SingleTableMixin
from .models import Device
from rest_framework.views import APIView
from rest_framework.response import Response
import sys, json
from io import StringIO
from rest_framework import serializers, status
import socket, ipaddress
from .models import Element, NetworkInterface, IpAssignment, Tag, ElementTag, ElementType
from .tables import ElementTable
from .filters import ElementFilter
from .forms import ElementForm, NetworkInterfaceFormSet, ElementTypeForm
from .utils import normalize_mac

sys.path.append(settings.MYTOOLS_PATH)
import telnet

def telnet_send(request):
    if len(request.GET)==0:
        return render(request,"telnet.html")

    ip = request.GET['ip']
    port = int(request.GET['port'])
    mes = request.GET['mes']
    ans = telnet.send(mes=mes, ip=ip, port=port)
    context=dict()
    context['ans'] = ans
    return render(request, "telnet.html", context)

def telnet_cmd(request):
    ip = request.GET['ip']
    port = int(request.GET['port'])
    mes = request.GET['mes']
    ans = telnet.send(mes=mes, ip=ip, port=port)
    return HttpResponse(ans)

def device_list(request):
    devices = Device.objects.all()
    return render(request, 'device_list.html', {'devices': devices})


class ElementListView(SingleTableMixin, FilterView):
    """
    Pro list view:
    - Sortable table (django-tables2)
    - Filters (django-filter)
    - Prefetch interfaces and active IPs to avoid N+1
    """
    table_class = ElementTable
    model = Element
    template_name = "devices/elements_list.html"   # root-level templates/
    filterset_class = ElementFilter
    paginate_by = 100

    def get_queryset(self):
        # Prefetch only active IP assignments for each interface
        active_ip_qs = IpAssignment.objects.filter(active=True).order_by("network_type", "kind", "ip_addr")

        iface_qs = NetworkInterface.objects.prefetch_related(
            Prefetch("ip_assignments", queryset=active_ip_qs, to_attr="active_ip_assignments")
        )

        # Base queryset with relations and prefetch applied
        return (
            Element.objects
            .select_related("element_type", "location")
            .prefetch_related(
                Prefetch("network_interfaces", queryset=iface_qs, to_attr="ifaces"),
                "tags",
            )
            .order_by("name")
        )

# views.py
# from django.contrib import messages
# from django.db import transaction
# from django.shortcuts import redirect, render
# from django.urls import reverse
# from django.views.generic.edit import CreateView, UpdateView

# from .forms import ElementForm, NetworkInterfaceFormSet
# from .models import Element

class ElementFormsetMixin:
    form_class = ElementForm
    formset_class = NetworkInterfaceFormSet
    template_name = "devices/element_form.html"

    def get_success_url(self):
        return reverse("element_list")

    def get_context_data(self, **kwargs):
        ctx = super().get_context_data(**kwargs)
        instance = getattr(self, "object", None)
        if "formset" not in ctx:
            ctx["formset"] = self.formset_class(instance=instance)
        if "form" not in ctx:
            ctx["form"] = self.form_class(instance=instance)
        ctx["element"] = instance
        ctx["all_tags"] = Tag.objects.all().order_by("name")

        if "properties_json" not in ctx:
            if instance is not None and instance.properties:
                ctx["properties_json"] = json.dumps(
                    instance.properties,
                    indent=2,
                    ensure_ascii=False,
                )
            else:
                ctx["properties_json"] = "{}"

        return ctx

    def render_invalid(self, form, formset):
        if form.errors:
            for field, errs in form.errors.items():
                for err in errs:
                    messages.error(self.request, f"Błąd w polu elementu '{field}': {err}")
        for i, f in enumerate(formset.forms):
            if f.errors:
                for field, errs in f.errors.items():
                    for err in errs:
                        messages.error(self.request, f"Interfejs #{i+1} – pole '{field}': {err}")
        for err in formset.non_form_errors():
            messages.error(self.request, f"Błąd formsetu: {err}")

        properties_json = self.request.POST.get("properties_json", "")

        return render(self.request, self.template_name, {
            "form": form,
            "formset": formset,
            "element": getattr(self, "object", None),
            "properties_json": properties_json if properties_json else "{}",
            "all_tags": Tag.objects.all().order_by("name"),
        })


    def post(self, request, *args, **kwargs):
        # Ustal, czy to update po obecności pk/slug w URL
        pk_kw = getattr(self, "pk_url_kwarg", "pk")
        slug_kw = getattr(self, "slug_url_kwarg", "slug")
        is_update = (pk_kw in kwargs) or (slug_kw in kwargs)

        self.object = self.get_object() if is_update else None

        form = self.form_class(request.POST, request.FILES, instance=self.object)
        formset = self.formset_class(request.POST, request.FILES, instance=self.object)

        action = request.POST.get("_action", "save_all")
        save_element = action in ("save_all", "save_element")
        save_ifaces  = action in ("save_all", "save_ifaces")

        form_valid = (not save_element) or form.is_valid()
        formset_valid = (not save_ifaces) or formset.is_valid()

        # --- nowe: walidacja JSON-a z textarea ---
        properties_raw = request.POST.get("properties_json", "").strip()
        properties_parsed = None
        json_valid = True
        if properties_raw:
            try:
                properties_parsed = json.loads(properties_raw)
            except json.JSONDecodeError as e:
                json_valid = False
                form.add_error(None, f"Niepoprawny JSON w polu właściwości elementu: {e}")
        else:
            properties_parsed = {}

        if not json_valid:
            return self.render_invalid(form, formset)

        if form_valid and formset_valid:
            try:
                with transaction.atomic():
                    # Save/create Element
                    if save_element:
                        self.object = form.save()
                    elif self.object is None:
                        # Create new Element without saving if not saving element
                        self.object = form.save()

                    # ustawiamy properties na sparsowany JSON
                    if save_element:
                        self.object.properties = properties_parsed
                        self.object.save(update_fields=["properties"])

                    # Save formset
                    if save_ifaces:
                        instances = formset.save(commit=False)
                        for obj in instances:
                            if getattr(obj, "element_id", None) is None:
                                obj.element = self.object
                            obj.save()
                        for obj in formset.deleted_objects:
                            obj.delete()
                        formset.save_m2m()
                
                if save_element and hasattr(form, "save_tags"):
                    form.save_tags(self.object)

                messages.success(request, "Zapis zakończony pomyślnie.")
                return redirect(self.get_success_url())

            except Exception as e:
                messages.error(request, f"Wystąpił błąd podczas zapisu: {e}")

        return self.render_invalid(form, formset)


class ElementCreateView(ElementFormsetMixin, CreateView):
    model = Element
    # form_class, template i post bierzemy z mixina


class ElementUpdateView(ElementFormsetMixin, UpdateView):
    model = Element
    # form_class, template i post bierzemy z mixina


class FreeIpListView(TemplateView):
    """
    Shows free IP addresses in 192.168.3.x (FAMO network),
    excluding dynamic pool 192.168.3.50–80.
    """
    template_name = "devices/free_ips.html"

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)

        # Full /24 network
        network = ipaddress.ip_network("192.168.3.0/24")

        # Active IPs in FAMO network within 192.168.3.x
        used_ips = set(
            IpAssignment.objects.filter(
                active=True,
                network_type="FAMO",
                ip_addr__startswith="192.168.3.",
            ).values_list("ip_addr", flat=True)
        )

        free_ips = []
        for ip in network.hosts():  # 192.168.3.1–254
            ip_str = str(ip)
            last_octet = int(ip_str.split(".")[-1])

            # Skip dynamic pool 192.168.3.50–80
            if 50 <= last_octet <= 80:
                continue

            # Skip used addresses
            if ip_str in used_ips:
                continue

            free_ips.append(ip_str)

        context["free_ips"] = free_ips
        context["total_free"] = len(free_ips)
        context["subnet"] = "192.168.3.0/24"
        context["excluded_range"] = "192.168.3.50–80"
        return context

    

class ElementConnectView(DetailView):
    """
    Connect UI:
    - renders a collapsible tree for eth_communication.parameters
    - leaves (val/min/max) are editable inputs
    """
    model = Element
    template_name = "devices/element_connect.html"
    context_object_name = "element"

    def get_context_data(self, **kwargs):
        ctx = super().get_context_data(**kwargs)
        el = self.object

        # 1) templaet typu elementu
        # 2) effective properties (merged)
        type_schema = el.element_type.properties_template or {}
        effective_schema = el.get_effective_properties()

        eth_comm = effective_schema.get("eth_communication", {})
        params = eth_comm.get("parameters", {})
        port = eth_comm.get("port", None)

        # --- get device IP FAMO ---
        device_ip = None
        ip_row = (
            IpAssignment.objects
            .filter(
                active=True,
                network_type="FAMO",
                interface__element=el,
            )
            .order_by("kind")
            .first()
        )

        if ip_row:
            device_ip = ip_row.ip_addr

        ctx.update({
            # for debugging
            "type_schema_json": json.dumps(type_schema, indent=2, ensure_ascii=False),
            "effective_schema_json": json.dumps(effective_schema, indent=2, ensure_ascii=False),

            # this is used to render the param tree
            "param_tree": params,
            "param_root_path": "eth_communication:parameters",

            "device_ip": device_ip,
            "eth_port": port,
        })
        return ctx


class DeviceNamesAPIView(APIView):
    def get(self, request):
        device_types = request.GET.getlist('device_type')
        queryset = Device.objects.all()
        if device_types:
            queryset = queryset.filter(device_type__in=device_types)
        devices = queryset.values('name', 'ip_famo', 'device_type', 'description', 'location')
        return Response(list(devices)) 
    

class ElementNamesAPIView(APIView):
    """ 
    GET /api/elements
    Optional query parameters:
    - element_type: filter by one or more element types (by name)
    - tag: filter by one or more tag names
    ex. api/elements?element_type=type1&type2&tag=tag1&tag2

    """
    def get(self, request):
        element_types = request.GET.getlist('element_type')
        tag_names = [t.strip() for t in request.GET.getlist('tag') if t.strip()]

        queryset = (
            Element.objects
            .select_related("location", "element_type")
            .prefetch_related(
                Prefetch(
                    "network_interfaces",
                    queryset=NetworkInterface.objects.filter(active=True).prefetch_related(
                        Prefetch(
                            "ip_assignments",
                            queryset=IpAssignment.objects.filter(active=True),
                            to_attr="active_ip"
                        )
                    ),
                    to_attr="ifaces"
                ),
                "tags",
            )
        )

        if element_types:
            queryset = queryset.filter(element_type__name__in=element_types)
        
        if tag_names:
            tag_qs = Tag.objects.filter(name__in=tag_names)
            for tag in tag_qs:
                queryset = queryset.filter(tags=tag)
            queryset = queryset.distinct()

        results = []

        for el in queryset:
            # --- wybór IP FAMO ---
            ip_famo = None
            for iface in el.ifaces:
                if iface.network_type == "FAMO":
                    if iface.active_ip:
                        # zakładam, że 1 aktywne IP na interfejs
                        ip_famo = iface.active_ip[0].ip_addr
                        break

            results.append({
                "id": el.id,
                "name": el.name,
                "ip_famo": ip_famo,
                "element_type": el.element_type.name if el.element_type else None,
                "description": el.description,
                "location": el.location.name if el.location else None,
                "tags": ", ".join([t.name for t in el.tags.all()]),
            })

        return Response(results)


class TelnetRequestSerializer(serializers.Serializer):
    command = serializers.CharField(max_length=100, allow_blank=False, trim_whitespace=False)
    ip = serializers.IPAddressField(protocol='both')
    port = serializers.IntegerField(min_value=1, max_value=65535)


class TelnetAPIView(APIView):
    """
    POST /api/telnet
    Body (application/json):
    {
      "command": "example command 1",
      "ip": "192.168.3.6",
      "port": 23
    }
    200 OK – sent correctly
    400 Bad Request – invalid input
    504 Gateway Timeout / 502 Bad Gateway – network error / timeout
    """
    # permission_classes = [AllowAny] 

    def post(self, request, *args, **kwargs):
        serializer = TelnetRequestSerializer(data=request.data)
        if not serializer.is_valid():
            return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

        command = serializer.validated_data["command"]
        ip = serializer.validated_data["ip"]
        port = serializer.validated_data["port"]

        try:
            ans = telnet.send(mes=command, ip=ip, port=port, todb=True)
            if isinstance(ans, bytes):
                ans = ans.decode(errors="replace")

            return Response(
                {
                    "status": "ok",
                    "ip": ip,
                    "port": port,
                    "command": command,
                    "response": ans,
                },
                status=status.HTTP_200_OK,
            )

        except (socket.timeout, TimeoutError) as e:
            return Response(
                {"detail": f"Timeout while connecting to {ip}:{port}", "error": str(e)},
                status=status.HTTP_504_GATEWAY_TIMEOUT,
            )
        except (ConnectionError, OSError) as e:
            return Response(
                {"detail": f"Network/Telnet error to {ip}:{port}", "error": str(e)},
                status=status.HTTP_502_BAD_GATEWAY,
            )
        except Exception as e:
            return Response(
                {"detail": "Unexpected server error", "error": str(e)},
                status=status.HTTP_500_INTERNAL_SERVER_ERROR,
            )


def _dict(v):
    return v if isinstance(v, dict) else {}


def deep_merge(base: dict, override: dict) -> dict:
    """
    Rekurencyjny merge: override nadpisuje base.
    """
    base = _dict(base)
    override = _dict(override)
    out = dict(base)

    for k, v in override.items():
        if isinstance(v, dict) and isinstance(out.get(k), dict):
            out[k] = deep_merge(out[k], v)
        else:
            out[k] = v
    return out


def deep_prune_equal(type_dict: dict, effective_dict: dict) -> dict:
    """
    Z effective_dict wycina wszystko, co jest identyczne jak w type_dict.
    Wynik to minimalny dict nadpisań, który trzeba trzymać w el.properties.
    """
    type_dict = _dict(type_dict)
    effective_dict = _dict(effective_dict)

    out = {}
    for k, v in effective_dict.items():
        if k in type_dict:
            tv = type_dict[k]
            if isinstance(v, dict) and isinstance(tv, dict):
                pruned = deep_prune_equal(tv, v)
                if pruned:
                    out[k] = pruned
            else:
                if v != tv:
                    out[k] = v
        else:
            # typ tego klucza nie ma -> to jest "własność elementu", więc trzymamy
            out[k] = v
    return out


class ElementPropertiesUpdateAPIView(APIView):
    """
    PATCH /api/elements/<id>/properties/update/
    Body may contain:
      - partial json with properties to patch
      - or full json with all desired properties
    Backend:
      1) get current effective properties
      2) put desired (patched) over current effective
      3) reduce by removing all identical to type template
      4) save minimal overrides to el.properties
      5) return effective properties as response
    """

    @transaction.atomic
    def patch(self, request, pk=None, element_id=None):
        element_id = element_id if element_id is not None else pk

        el = (
            Element.objects
            .select_related("element_type")
            .only("id", "properties", "element_type__properties_template")
            .get(pk=element_id)
        )

        payload = request.data or {}
        incoming = payload.get("properties", payload)

        if not isinstance(incoming, dict):
            return Response(
                {"detail": "Expected JSON object (either body itself or under 'properties')."},
                status=status.HTTP_400_BAD_REQUEST,
            )

        type_template = el.element_type.properties_template or {}

        # 1) merge current effective
        current_effective = el.get_effective_properties()

        # 2) put patched/incoming over current effective
        desired_effective = deep_merge(current_effective, incoming)

        # 3) remove all identical to type template
        new_overrides = deep_prune_equal(type_template, desired_effective)

        # 4) save minimal overrides
        el.properties = new_overrides
        el.save(update_fields=["properties"])

        # 5) return effective properties
        return Response(el.get_effective_properties(), status=status.HTTP_200_OK)



class ElementPropertiesAPIView(APIView):
    def get(self, request, element_id):
        element_id = element_id if element_id is not None else pk

        el = (
            Element.objects
            .select_related("element_type")
            .only("id", "properties", "element_type__properties_template")
            .get(pk=element_id)
        )

        # merge identycznie jak w ElementConnectView:
        merged = el.get_effective_properties()

        return Response(merged)



@require_POST
def run_fetch_dhcp(request):
    """
    Run the 'fetch_dhcp' management command, accepting a password from POST.
    NOTE: open to all users per your request; keep POST+CSRF. Do not log the password.
    """
    out = StringIO()
    password = request.POST.get("password") or None
    strict = bool(request.POST.get("strict_host_key"))  # optional checkbox

    try:
        with transaction.atomic():
            # Pass password only if present; avoid printing it anywhere
            if password is not None:
                call_command("fetch_dhcp", stdout=out, password=password, strict_host_key=strict)
            else:
                call_command("fetch_dhcp", stdout=out, strict_host_key=strict)
        # Do not display the password in messages
        messages.success(request, f"fetch_dhcp finished.\n{out.getvalue()[:2000]}")
    except CommandError as e:
        messages.error(request, f"fetch_dhcp failed: {e}\n{out.getvalue()[:1000]}")
    except Exception as e:
        messages.error(request, f"Unexpected error in fetch_dhcp: {e}")
    return redirect(reverse("element_list"))


@require_POST
def run_relink_ips(request):
    """
    Run the 'relink_ips' management command.
    NOTE: intentionally open to all users (as requested).
    """
    out = StringIO()
    try:
        with transaction.atomic():
            call_command("relink_ips", stdout=out)
        messages.success(request, f"relink_ips finished.\n{out.getvalue()[:2000]}")
    except CommandError as e:
        messages.error(request, f"relink_ips failed: {e}\n{out.getvalue()[:1000]}")
    except Exception as e:
        messages.error(request, f"Unexpected error in relink_ips: {e}")
    return redirect(reverse("element_list"))


class ElementTypeListView(ListView):
    model = ElementType
    template_name = "devices/elementtype_list.html"
    context_object_name = "types"
    paginate_by = 50

    def get_queryset(self):
        qs = ElementType.objects.all().order_by("name", "vendor", "model")
        q = (self.request.GET.get("q") or "").strip()
        if q:
            qs = qs.filter(
                Q(name__icontains=q) |
                Q(vendor__icontains=q) |
                Q(model__icontains=q) |
                Q(notes__icontains=q)
            )
        return qs


class ElementTypeFormMixin:
    model = ElementType
    form_class = ElementTypeForm
    template_name = "devices/elementtype_form.html"

    def get_success_url(self):
        # jeśli masz listę typów, podmień na swoją nazwę
        return reverse("elementtype_edit", kwargs={"pk": self.object.pk})

    def form_valid(self, form):
        resp = super().form_valid(form)
        messages.success(self.request, "Zapis typu elementu zakończony pomyślnie.")
        return resp

    def form_invalid(self, form):
        messages.error(self.request, "Formularz zawiera błędy. Popraw je i spróbuj ponownie.")
        return super().form_invalid(form)


class ElementTypeCreateView(ElementTypeFormMixin, CreateView):
    pass


class ElementTypeUpdateView(ElementTypeFormMixin, UpdateView):
    pass

