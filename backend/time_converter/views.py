from django.shortcuts import render
from django.http import JsonResponse
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


def api_mjd(request):
    """
    API endpoint returning current server MJD as JSON.

    Response example:
    { "mjd": "60739" }
    """
    # getMJD returns a float; return the integer part as string to match existing UI expectations
    try:
        mjd_val = tim.getMJD()
        mjd_str = str(float(mjd_val))
    except Exception:
        # On error, return 500 with helpful message
        return JsonResponse({'error': 'failed to compute MJD'}, status=500)

    return JsonResponse({'mjd': mjd_str})