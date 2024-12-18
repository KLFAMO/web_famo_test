from django.shortcuts import render
from django.http import HttpResponse
from django.views.generic import TemplateView
from rest_framework.views import APIView
from rest_framework.response import Response
import json
import sys
from django.conf import settings
sys.path.append(settings.MYTOOLS_PATH)
import sqldata as sqd
import time_tools as tim


class IndexView(TemplateView):
    template_name = "anda.html"

    def get_context_data(self, **kwargs):
        from_mjd = self.request.GET.get('from_mjd', 1000000)
        to_mjd = self.request.GET.get('to_mjd', 1000000)
        table_name = self.request.GET.get('table_name', '')

        print(from_mjd, to_mjd, table_name)

        try:
            from_mjd = float(from_mjd)
            to_mjd = float(to_mjd)

            data_from_db = sqd.getdata(table_name, from_mjd, to_mjd)
            mjd_tab_json = json.dumps(data_from_db.mjd_tab().tolist())
            val_tab_json = json.dumps(data_from_db.val_tab().tolist())
        except Exception as e:
            print('problem:', e)
            mjd_tab_json = []
            val_tab_json = []

        # Kontekst dla szablonu
        context = super().get_context_data(**kwargs)
        context.update({
            'tables_names': sqd.gettables(),
            'mjd_tab_json': mjd_tab_json,
            'val_tab_json': val_tab_json,
            'mjd_now': tim.getMJD(),
            'last_from_mjd': from_mjd,
            'last_to_mjd': to_mjd,
            'last_table_name': table_name,
        })
        return context



class DataTablesNames(APIView):
    def get(self, request, *args, **kwargs):
        data = sqd.gettables()
        return Response(data)


class TableData(APIView):
    def get(self, request, *args, **kwargs):
        from_mjd = request.query_params.get('from_mjd', 1000000)
        to_mjd = request.query_params.get('to_mjd', 1000000)
        table_name = request.query_params.get('table_name', '')
        print(from_mjd, to_mjd, table_name)
        try:
            data_from_db = sqd.getdata(table_name, from_mjd, to_mjd)
            mjd_tab = data_from_db.mjd_tab().tolist()
            val_tab = data_from_db.val_tab().tolist()
        except:
            print('problem')
            mjd_tab = []
            val_tab = [] 

        data = {
            "table_name" : table_name,
            "from_mjd" : from_mjd,
            "to_mjd" : to_mjd,
            "data" : {
                "mjd" : mjd_tab,
                "val" : val_tab
            } 
        }
        return Response(data)
