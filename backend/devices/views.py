from django.shortcuts import redirect, render
from django.db.models import Prefetch
from django.urls import reverse
from django.views.generic import ListView
from django.http import HttpResponse
from django.views.generic import CreateView, UpdateView
from django_tables2.views import SingleTableMixin
from django_filters.views import FilterView
from .models import Device
from rest_framework.views import APIView
from rest_framework.response import Response
import sys
from django.conf import settings
from rest_framework import serializers, status
import socket
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
    paginate_by = 50

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

class ElementCreateView(CreateView):
    """Create Element with inline NetworkInterface formset."""
    model = Element
    form_class = ElementForm
    template_name = "devices/element_form.html"

    def get(self, request, *args, **kwargs):
        form = self.form_class()
        formset = NetworkInterfaceFormSet()
        return render(request, self.template_name, {"form": form, "formset": formset})

    def post(self, request, *args, **kwargs):
        form = self.form_class(request.POST)
        formset = NetworkInterfaceFormSet(request.POST)
        if form.is_valid() and formset.is_valid():
            element = form.save()
            formset.instance = element
            formset.save()
            return redirect(reverse("element_list"))
        return render(request, self.template_name, {"form": form, "formset": formset})

class ElementUpdateView(UpdateView):
    """Update Element with inline NetworkInterface formset."""
    model = Element
    form_class = ElementForm
    template_name = "element_form.html"
    context_object_name = "element"

    def get(self, request, *args, **kwargs):
        element = self.get_object()
        form = self.form_class(instance=element)
        formset = NetworkInterfaceFormSet(instance=element)
        return render(request, self.template_name, {"form": form, "formset": formset, "element": element})

    def post(self, request, *args, **kwargs):
        element = self.get_object()
        form = self.form_class(request.POST, instance=element)
        formset = NetworkInterfaceFormSet(request.POST, instance=element)
        if form.is_valid() and formset.is_valid():
            form.save()
            formset.save()
            return redirect(reverse("element_list"))
        return render(request, self.template_name, {"form": form, "formset": formset, "element": element})



class DeviceNamesAPIView(APIView):
    def get(self, request):
        device_types = request.GET.getlist('device_type')
        queryset = Device.objects.all()
        if device_types:
            queryset = queryset.filter(device_type__in=device_types)
        devices = queryset.values('name', 'ip_famo', 'device_type', 'description', 'location')
        return Response(list(devices)) 


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