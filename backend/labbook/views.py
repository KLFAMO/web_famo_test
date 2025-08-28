from django.shortcuts import redirect
from django.urls import reverse
from django.views.generic import TemplateView
import sys
from django.conf import settings
sys.path.append(settings.MYTOOLS_PATH)
from sqldata import get_logs, sendMessage
from time_tools import getMJD

class LogsMainView(TemplateView):
    template_name = "logs_main.html"

    def post(self, request, *args, **kwargs):
        from_mjd = request.POST.get("from_mjd")
        to_mjd = request.POST.get("to_mjd")
        message = request.POST.get("message")
        tags = request.POST.get("tags")

        if from_mjd and message:
            if not to_mjd:
                to_mjd = from_mjd
            sendMessage(from_mjd, to_mjd, message, tags)

        start_mjd = request.GET.get("start_mjd", "60750")
        end_mjd = request.GET.get("end_mjd", "70000")

        return redirect(f"{reverse('labbook')}?start_mjd={start_mjd}&end_mjd={end_mjd}")


    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context["tytul"] = "Labbook"

        from_mjd = float(self.request.GET.get("from_mjd", 60751))
        to_mjd = float(self.request.GET.get("to_mjd", 100000))

        context["from_mjd"] = from_mjd
        context["to_mjd"] = to_mjd

        log_data = get_logs(from_mjd, to_mjd)
        columns = ["from_mjd", "to_mjd", "message", "category", "id", "parent_id", "extra"]
        logs = [dict(zip(columns, entry)) for entry in log_data]
        context["logs"] = sorted(logs, key=lambda x: x['from_mjd'], reverse=True)
        context["mjd_now"] = getMJD()

        return context
