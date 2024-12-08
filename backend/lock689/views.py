from django.shortcuts import render
from django.http import HttpResponse
import sys
import pathlib as pa
import json

sys.path.append("~/svnSr/progs/mytools")
import FAMO_tools as fts
import telnet as tln

def index(request):
    context = dict()
    return render(request, "lock689.html", context)

def get_data(request):
    data = tln.send(ip='localhost', port=2003, mes='data')
    return HttpResponse( data )

def get_param(request):
    param = tln.send(ip='localhost', port=2003, mes='param')
    return HttpResponse( param )

def set_param(request):
    param = list(request.GET.dict().keys())[0]
    val = request.GET.dict()[param]
    print(param, val)
    tln.send(ip='localhost', port=2003, mes='set ' + param + ' ' + val)
    return HttpResponse( 'ok' )
