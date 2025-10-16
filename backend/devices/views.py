from django.shortcuts import render
from django.http import HttpResponse
from .models import Device
from rest_framework.views import APIView
from rest_framework.response import Response
import sys
from django.conf import settings
from rest_framework import serializers, status
import socket

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