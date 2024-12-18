from django.shortcuts import render
from django.http import HttpResponse
from rest_framework.views import APIView
from rest_framework.response import Response
import json
import sys
from django.conf import settings
sys.path.append(settings.MYTOOLS_PATH)
import sqldata as sqd
import time_tools as tim

def index(request):

    from_mjd = request.GET.get('from_mjd', 1000000)
    to_mjd = request.GET.get('to_mjd', 1000000)
    table_name = request.GET.get('table_name', '')
    print(from_mjd, to_mjd, table_name)
    try:
        data_from_db = sqd.getdata(table_name, from_mjd, to_mjd)
        mjd_tab_json = json.dumps(data_from_db.mjd_tab().tolist())
        val_tab_json = json.dumps(data_from_db.val_tab().tolist())
    except:
        print('problem')
        mjd_tab_json = []
        val_tab_json = []

    context = {
        'tables_names': sqd.gettables(),
        'mjd_tab_json': mjd_tab_json,
        'val_tab_json': val_tab_json,
        'mjd_now': tim.getMJD(),
        'last_from_mjd': from_mjd,
        'last_to_mjd': to_mjd,
        'last_table_name': table_name,
    } 
    return render(request, "anda.html", context)


class DataTablesNames(APIView):
    def get(self, request, *args, **kwargs):
        tables = sqd.gettables()
        return Response(tables)
