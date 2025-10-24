from django.http import JsonResponse, HttpResponseBadRequest
from django.views import View
from django.views.generic import TemplateView
from django.utils.decorators import method_decorator
from django.views.decorators.csrf import csrf_exempt
from django.conf import settings
import json, sys

# Add your custom library path
sys.path.append(settings.MYTOOLS_PATH)

# Import your existing database tools
from sqldata import get_logs, sendMessage, modifyMessage, dbquery
from time_tools import getMJD


def _row_to_api_dict(row):
    """
    Convert a row from the 'logs' SQL table into the REST API dictionary format.

    Expected SQL row structure:
    (mjd, mjd2, mes, tag, id, prev_id, new_id)
    but we handle missing columns gracefully.

    Returns:
        dict with keys matching the REST API specification:
        id, start_mjd, end_mjd, message, tag, parentId,
    """
    start_mjd = row[0]
    end_mjd = row[1]
    message = row[2]
    tag = row[3]
    _id = row[4]
    parent_id = 0
    if len(row) >= 6 and row[5] is not None:
        parent_id = row[5]

    return {
        "id": int(_id),
        "start_mjd": str(start_mjd),
        "end_mjd": str(end_mjd),
        "message": message,
        "tag": tag,
        "parentId": int(parent_id) if parent_id else 0,
    }


class LabbookApiView(View):
    """
    REST API endpoint for Labbook logs.

    URL: /api/labbook

    Supported methods:
      - GET:    list or retrieve a single log (?id=)
      - POST:   insert new log
      - PUT:    modify an existing log (creates a new version using modifyMessage)
    """

    @method_decorator(csrf_exempt)
    def dispatch(self, *args, **kwargs):
        # Disable CSRF for simplicity when using fetch() from frontend
        return super().dispatch(*args, **kwargs)

    def get(self, request):
        """
        GET method:
          - /api/labbook?from_mjd=...&to_mjd=...  ? list logs in range
          - /api/labbook?id=5                     ? get single log
        """
        _id = request.GET.get("id")
        if _id is not None:
            # --- Single record request ---
            try:
                _id = int(_id)
            except ValueError:
                return HttpResponseBadRequest("Bad id")

            rows = dbquery(f"SELECT * FROM logs WHERE id={_id};")
            if not rows:
                return JsonResponse({"detail": "Not found"}, status=404)
            return JsonResponse(_row_to_api_dict(rows[0]), safe=False)

        # --- List request ---
        try:
            start_mjd = float(request.GET.get("from_mjd", 0))
            end_mjd = float(request.GET.get("to_mjd", 10**9))
        except ValueError:
            return HttpResponseBadRequest("Bad from_mjd/to_mjd")

        rows = get_logs(start_mjd, end_mjd)
        payload = [_row_to_api_dict(r) for r in rows]
        # Sort descending by start_mjd for newest first
        payload.sort(key=lambda x: float(x["start_mjd"]), reverse=True)
        return JsonResponse(payload, safe=False)

    def post(self, request):
        """
        POST method:
        Create a new log entry.

        Body (application/json):
        {
          "start_mjd": "...",
          "end_mjd": "...",
          "message": "...",
          "tag": "...",
        }
        """
        if request.content_type != "application/json":
            return HttpResponseBadRequest("Content-Type must be application/json")
        try:
            data = json.loads(request.body.decode("utf-8"))
            start_mjd = str(data["start_mjd"])
            end_mjd = str(data.get("end_mjd", start_mjd))
            message = data["message"]
            tag = data.get("tag", "")
        except (KeyError, json.JSONDecodeError):
            return HttpResponseBadRequest("Invalid JSON body")

        # Use your existing function to insert a new log
        sendMessage(start_mjd, end_mjd, message, tag)
        return JsonResponse({"status": "created"}, status=201)

    def put(self, request):
        """
        PUT method:
        Update (version) an existing log entry.
        Creates a new record linked to the previous one using 'modifyMessage()'.

        Body (application/json):
        {
          "id": 5,
          "start_mjd": "...",
          "end_mjd": "...",
          "message": "...",
          "tag": "...",
        }
        """
        if request.content_type != "application/json":
            return HttpResponseBadRequest("Content-Type must be application/json")
        try:
            data = json.loads(request.body.decode("utf-8"))
            _id = int(data["id"])
            start_mjd = str(data["start_mjd"])
            end_mjd = str(data.get("end_mjd", start_mjd))
            message = data["message"]
            tag = data.get("tag", "")
        except (KeyError, ValueError, json.JSONDecodeError):
            return HttpResponseBadRequest("Invalid JSON body")

        # Ensure the record exists
        rows = dbquery(f"SELECT id FROM logs WHERE id={_id};")
        if not rows:
            return JsonResponse({"detail": "id not found"}, status=404)

        # Use your function that handles versioning of messages
        modifyMessage(start_mjd, end_mjd, message, tag, str(_id))
        return JsonResponse({"status": "updated"}, status=200)


class LogsMainView(TemplateView):
    """
    Standard Django TemplateView for the Labbook frontend.
    This page loads the HTML/JS UI that communicates with /api/labbook.
    """
    template_name = "logs_main.html"

    def get_context_data(self, **kwargs):
        ctx = super().get_context_data(**kwargs)
        ctx["tytul"] = "Labbook"
        ctx["mjd_now"] = getMJD()
        return ctx
