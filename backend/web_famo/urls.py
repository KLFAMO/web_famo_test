"""web_famo URL Configuration

The `urlpatterns` list routes URLs to views. For more information please see:
    https://docs.djangoproject.com/en/2.2/topics/http/urls/
Examples:
Function views
    1. Add an import:  from my_app import views
    2. Add a URL to urlpatterns:  path('', views.home, name='home')
Class-based views
    1. Add an import:  from other_app.views import Home
    2. Add a URL to urlpatterns:  path('', Home.as_view(), name='home')
Including another URLconf
    1. Import the include() function: from django.urls import include, path
    2. Add a URL to urlpatterns:  path('blog/', include('blog.urls'))
"""
from django.contrib import admin
from django.urls import path
import devices.views
import sidebandfitapp.views
import mainapp.views
import lock689.views
import anda.views
import freq_monitor.views
import time_converter.views
import labbook.views

urlpatterns = [
    path('admin/', admin.site.urls),
    path('', mainapp.views.index),
    path('telnet/', devices.views.telnet_send, name='telnet'),
    path('telnet_cmd/', devices.views.telnet_cmd, name='telnet_cmd'),
    path('devices/', devices.views.device_list, name='device_list'),
    path('elements/', devices.views.ElementListView.as_view(), name='element_list'),
    path('elements/add/', devices.views.ElementCreateView.as_view(), name='element_add'),
    path('elements/<int:pk>/edit/', devices.views.ElementUpdateView.as_view(), name='element_edit'),
    path('elements/<int:pk>/connect/', devices.views.ElementConnectView.as_view(), name='element_connect'),
    path('elements/fetch-dhcp/', devices.views.run_fetch_dhcp, name='run_fetch_dhcp'),
    path('elements/relink-ips/', devices.views.run_relink_ips, name='run_relink_ips'),
    path('elements/free-ips/', devices.views.FreeIpListView.as_view(), name='free_ip_list'),
    path('sidebands/', sidebandfitapp.views.index, name='sidebands'),
    path('lock689/', lock689.views.index, name='lock689'),
    path('lock689/get_data', lock689.views.get_data),
    path('lock689/get_param', lock689.views.get_param),
    path('lock689/set_param', lock689.views.set_param),
    path('anda/', anda.views.IndexView.as_view(), name='anda'),
    path('anda2/', anda.views.IndexView2.as_view(), name='anda2'),
    path('anda/upload_script/', anda.views.UploadScript.as_view(), name='anda_upload_script'),
    path('api/anda/tables_names/', anda.views.DataTablesNames.as_view(), name='anda_tables'),
    path('api/anda/table_data', anda.views.TableData.as_view(), name='anda_table_data'),
    path('api/anda/script', anda.views.ScriptData.as_view(), name='anda_script'),
    path('api/devices', devices.views.DeviceNamesAPIView.as_view(), name='device_names_api'),
    path('api/device/<int:element_id>', devices.views.DeviceNameByIdAPIView.as_view(), name='device_name_api'),
    path('api/elements', devices.views.ElementNamesAPIView.as_view(), name='element_names_api'),
    path('api/element/<int:element_id>/properties', devices.views.ElementPropertiesAPIView.as_view(), name='element_properties_api'),
    path('api/element/<int:element_id>/properties/update', devices.views.ElementPropertiesUpdateAPIView.as_view(), name='update_element_properties_api'),
    path("elementtype/", devices.views.ElementTypeListView.as_view(), name="elementtype_list"),
    path("elementtype/add/", devices.views.ElementTypeCreateView.as_view(), name="elementtype_add"),
    path("elementtype/<int:pk>/edit/", devices.views.ElementTypeUpdateView.as_view(), name="elementtype_edit"),
    path('api/telnet', devices.views.TelnetAPIView.as_view(), name='telnet_api'),
    path('api/mjd', time_converter.views.api_mjd, name='mjd_api'),
    path('freq_monitor/', freq_monitor.views.index, name='freq_monitor'),
    path('freq_monitor/get_data', freq_monitor.views.get_data),
    path('time_converter/', time_converter.views.convert_date, name='time_converter'),
    path('labbook/', labbook.views.LogsMainView.as_view(), name='labbook'),
    path("api/labbook", labbook.views.LabbookApiView.as_view(), name="labbook_api"),
]
