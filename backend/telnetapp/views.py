from django.shortcuts import render
from django.http import HttpResponse
import pathlib
import sys

sys.path.append( str(pathlib.Path(__file__).absolute().parents[1] / 'tools') )
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
    print(ans)
    context=dict()
    context['ans'] = ans
    return render(request, "telnet.html", context)

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