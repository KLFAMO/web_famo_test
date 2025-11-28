from django.conf import settings
from django.core.management import call_command, CommandError
from django.contrib import messages
from django.db import transaction
from django.db.models import Prefetch
from django.http import HttpResponse
from django.shortcuts import redirect, render
from django.utils.safestring import mark_safe
from django.urls import reverse
from django.views.decorators.http import require_POST
from django.views.generic import ListView, TemplateView
from django.views.generic import CreateView, UpdateView, DetailView
from django_filters.views import FilterView
from django_tables2.views import SingleTableMixin
from .models import Device
from rest_framework.views import APIView
from rest_framework.response import Response
import sys, json
from io import StringIO
from rest_framework import serializers, status
import socket, ipaddress
from .models import Element, NetworkInterface, IpAssignment
from .tables import ElementTable
from .filters import ElementFilter
from .forms import ElementForm, NetworkInterfaceFormSet

sys.path.append(settings.MYTOOLS_PATH)
import telnet

dds_list = [
    {'name':'Sr1_dds_blueMOT', 'ip':'192.168.3.122', 'port':23},
    {'name':'Sr1_fox8_dds', 'ip':'192.168.3.8', 'port':5556},
    {'name':'Sr2_fox7_dds', 'ip':'192.168.3.7', 'port':5556},
    {'name':'Hyd_dds_rack_4ch', 'ip':'192.168.3.103', 'port':23},
    {'name':'Hyd_dds_rack_1ch', 'ip':'192.168.3.104', 'port':10},
    {'name':'Hyd_dds_dedryft_4ch', 'ip':'192.168.3.11', 'port':23},
]

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

def setdds(request):
    context=dict()
    context['dds_list'] = dds_list
    if len(request.GET)==0:
        return render(request,"setdds.html", context)

    dds = request.GET['dds']
    ind = -1
    for i, e in enumerate(dds_list):
        if e['name']==dds:
            ind = i
    mes = request.GET['mes']
    print(ind, dds_list[ind]['ip'], dds_list[ind]['port'])
    ans = telnet.send(mes=mes, ip=dds_list[ind]['ip'], port=int(dds_list[ind]['port']))

    context['ans'] = ans
    return render(request, "setdds.html", context)


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
            .prefetch_related(Prefetch("network_interfaces", queryset=iface_qs, to_attr="ifaces"))
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

        return render(self.request, self.template_name, {
            "form": form,
            "formset": formset,
            "element": getattr(self, "object", None),
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

        if form_valid and formset_valid:
            try:
                with transaction.atomic():
                    # Zapis/utworzenie elementu
                    if save_element:
                        self.object = form.save()
                    elif self.object is None:
                        # Create bez 'save_element' – i tak musimy mieć obiekt dla FK
                        self.object = form.save()

                    # Zapis formsetu
                    if save_ifaces:
                        instances = formset.save(commit=False)
                        for obj in instances:
                            if getattr(obj, "element_id", None) is None:
                                obj.element = self.object
                            obj.save()
                        for obj in formset.deleted_objects:
                            obj.delete()
                        formset.save_m2m()

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

        type_schema = el.element_type.properties_template or {}

        params = (
            type_schema.get("eth_communication", {})
                       .get("parameters", {})
        )

        ctx.update({
            "type_schema_json": json.dumps(type_schema, indent=2, ensure_ascii=False),
            "param_tree": params,
            "param_root_path": "eth_communication:parameters",
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
    def get(self, request):
        element_types = request.GET.getlist('element_type')

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
                )
            )
        )

        if element_types:
            queryset = queryset.filter(element_type__name__in=element_types)

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
                "name": el.name,
                "ip_famo": ip_famo,
                "element_type": el.element_type.name if el.element_type else None,
                "description": el.description,
                "location": el.location.name if el.location else None,
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
