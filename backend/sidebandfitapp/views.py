from django.shortcuts import render
from django.http import HttpResponse
import sys
from django.conf import settings

sys.path.append(settings.MYTOOLS_PATH)
import sidebands as sb
import os

def index(request):
    if len(request.GET) == 0:
        return render(request,"sideband.html")
    context = dict()
    if request.GET['sub'] == 'fit' and ('folder' in request.GET) and ('filename' in request.GET) :
        vz, Tr, Tz = sb.fit(request.GET['folder'] + '/' +request.GET['filename']
                            , 'sidebandfitapp/static/sidebandfitapp/fit.png')
        context = {'vz': vz, 'Tr':Tr, 'Tz':Tz}
    if request.GET['sub'] == 'svn_update':
        stream = os.popen('svn update /home/stront/svnSr/data')
        output = stream.read()
        print(output)
    return render(request, "sideband.html", context)
