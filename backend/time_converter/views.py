from django.shortcuts import render
from django import forms
import sys
sys.path.append("~/svnSr/progs/mytools")
import time_tools as tim


# Formularz z dwoma polami: jedno dla daty MJD, drugie dla daty lokalnej
class DateConversionForm(forms.Form):
    mjd_date = forms.CharField(label='MJD', max_length=100, required=False)
    local_date = forms.CharField(label='Local (YYYY-MM-DD:HH-mm-ss)', max_length=100, required=False)

def convert_date(request):
    form = DateConversionForm()
    mjd_result = ""
    local_result = ""

    if request.method == 'POST':
        form = DateConversionForm(request.POST)
        if form.is_valid():
            if 'convert_mjd' in request.POST:
                mjd_date = form.cleaned_data['mjd_date']
                try:
                    local_result = tim.MJD2local(mjd_date)
                except Exception as e:
                    local_result = f"Error: {str(e)}"
            elif 'convert_local' in request.POST:
                local_date = form.cleaned_data['local_date']
                try:
                    mjd_result = tim.local2MJD(local_date) 
                except Exception as e:
                    mjd_result = f"Error: {str(e)}"

    return render(request, 'convert.html', {
        'form': form,
        'mjd_result': mjd_result,
        'local_result': local_result,
    })