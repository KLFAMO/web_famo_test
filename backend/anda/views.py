from collections.abc import Mapping, Iterable
from django.conf import settings
from django.shortcuts import render
from django.http import HttpResponse
from django.views.generic import TemplateView
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework import parsers, status
import json
import sys
from django.conf import settings
sys.path.append(settings.MYTOOLS_PATH)
import sqldata as sqd
import time_tools as tim
import tempfile
import subprocess
import os

TIMEOUT_S = 60 # maks time for script execution

EXTRA_PATHS = [
    str(getattr(settings, "BASE_DIR", "")),       # katalog projektu (z manage.py)
    str(getattr(settings, "MYTOOLS_PATH", "")),   # gdzie le?y sqldata / time_tools
]
EXTRA_PATHS = [p for p in EXTRA_PATHS if p]       # odfiltruj puste

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


class UploadScript(TemplateView):
    template_name = "anda_script.html"


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


def to_jsonable(x, *, depth=0, max_depth=20, max_items=None):
    """Konwertuje 'x' na struktury JSON-owalne bez zale?no?ci od numpy/pandas."""
    if depth > max_depth:
        return "/*depth limit*/"

    # Prymitywy
    if x is None or isinstance(x, (bool, int, float, str)):
        return x

    # Obiekty z API podobnym do numpy/pandas ? bez importï¿½w:
    # 1) 'tolist' (np. ndarray, wiele kontenerï¿½w)
    tolist = getattr(x, "tolist", None)
    if callable(tolist):
        try:
            return to_jsonable(tolist(), depth=depth+1, max_depth=max_depth, max_items=max_items)
        except Exception:
            pass

    # 2) 'item' (np. skalar numpy)
    item = getattr(x, "item", None)
    if callable(item):
        try:
            return to_jsonable(item(), depth=depth+1, max_depth=max_depth, max_items=max_items)
        except Exception:
            pass

    # 3) 'to_dict' (np. DataFrame, niestandardowe obiekty)
    to_dict = getattr(x, "to_dict", None)
    if callable(to_dict):
        try:
            # sprï¿½buj orient="records" je?li wspierane
            try:
                d = to_dict(orient="records")
            except TypeError:
                d = to_dict()
            return to_jsonable(d, depth=depth+1, max_depth=max_depth, max_items=max_items)
        except Exception:
            pass

    # Mapping (s?owniki)
    if isinstance(x, Mapping):
        out = {}
        for k, v in x.items():
            out[str(k)] = to_jsonable(v, depth=depth+1, max_depth=max_depth, max_items=max_items)
        return out

    # Sekwencje / iterowalne (ale nie string/bytes)
    if isinstance(x, (list, tuple, set)):
        it = x
    elif isinstance(x, (bytes, bytearray, str)):
        return str(x)
    elif isinstance(x, Iterable):
        it = x
    else:
        # Ostatnia prï¿½ba: je?li da si? zrzutowa? do JSON, ok; inaczej string
        try:
            json.dumps(x)
            return x
        except Exception:
            return str(x)

    # Z?ï¿½? list? z iterowalnego (z opcjonalnym limitem elementï¿½w)
    out_list = []
    count = 0
    for elem in it:
        out_list.append(to_jsonable(elem, depth=depth+1, max_depth=max_depth, max_items=max_items))
        count += 1
        if max_items is not None and count >= max_items:
            out_list.append("/*truncated*/")
            break
    return out_list


class ScriptData(APIView):
    parser_classes = [parsers.MultiPartParser]

    def post(self, request, *args, **kwargs):
        if 'script' not in request.FILES:
            return Response({"status": "error", "message": "No script file uploaded"}, status=400)

        uploaded = request.FILES['script']
        code_bytes = uploaded.read()
        filename = getattr(uploaded, 'name', 'script.py')

        # --- parse field 'vars' ---
        raw_vars = request.data.get('vars', '')
        try:
            if raw_vars and raw_vars.strip().startswith('['):
                var_names = json.loads(raw_vars)
            else:
                var_names = [v.strip() for v in raw_vars.split(',') if v.strip()]
        except Exception:
            return Response({"status": "error", "message": "Invalid 'vars' format"}, status=400)

        # ==== if var_names build JSON wtih them ====
        if var_names:
            with tempfile.TemporaryDirectory() as workdir:
                user_path = os.path.join(workdir, "user_script.py")
                with open(user_path, "wb") as f:
                    f.write(code_bytes)

                runner_path = os.path.join(workdir, "runner.py")
                runner_code = (
                    "import sys, os, json\n"
                    f"sys.path[:0] = {json.dumps(EXTRA_PATHS)}\n"
                    "from collections.abc import Mapping, Iterable\n"
                    "\n"
                    "def to_jsonable(x, *, depth=0, max_depth=20, max_items=None):\n"
                    "    if depth > max_depth:\n"
                    "        return '/*depth limit*/'\n"
                    "    if x is None or isinstance(x, (bool, int, float, str)):\n"
                    "        return x\n"
                    "    tolist = getattr(x, 'tolist', None)\n"
                    "    if callable(tolist):\n"
                    "        try:\n"
                    "            return to_jsonable(tolist(), depth=depth+1, max_depth=max_depth, max_items=max_items)\n"
                    "        except Exception:\n"
                    "            pass\n"
                    "    item = getattr(x, 'item', None)\n"
                    "    if callable(item):\n"
                    "        try:\n"
                    "            return to_jsonable(item(), depth=depth+1, max_depth=max_depth, max_items=max_items)\n"
                    "        except Exception:\n"
                    "            pass\n"
                    "    to_dict = getattr(x, 'to_dict', None)\n"
                    "    if callable(to_dict):\n"
                    "        try:\n"
                    "            try:\n"
                    "                d = to_dict(orient='records')\n"
                    "            except TypeError:\n"
                    "                d = to_dict()\n"
                    "            return to_jsonable(d, depth=depth+1, max_depth=max_depth, max_items=max_items)\n"
                    "        except Exception:\n"
                    "            pass\n"
                    "    if isinstance(x, Mapping):\n"
                    "        return {str(k): to_jsonable(v, depth=depth+1, max_depth=max_depth, max_items=max_items) for k, v in x.items()}\n"
                    "    if isinstance(x, (list, tuple, set)):\n"
                    "        it = x\n"
                    "    elif isinstance(x, (bytes, bytearray, str)):\n"
                    "        return str(x)\n"
                    "    elif isinstance(x, Iterable):\n"
                    "        it = x\n"
                    "    else:\n"
                    "        try:\n"
                    "            json.dumps(x)\n"
                    "            return x\n"
                    "        except Exception:\n"
                    "            return str(x)\n"
                    "    out_list = []\n"
                    "    count = 0\n"
                    "    for elem in it:\n"
                    "        out_list.append(to_jsonable(elem, depth=depth+1, max_depth=max_depth, max_items=max_items))\n"
                    "        count += 1\n"
                    "        if max_items is not None and count >= max_items:\n"
                    "            out_list.append('/*truncated*/')\n"
                    "            break\n"
                    "    return out_list\n"
                    "\n"
                    "ns = {}\n"
                    "code = open('user_script.py','r',encoding='utf-8').read()\n"
                    "exec(compile(code, 'user_script.py', 'exec'), ns, ns)\n"
                    f"names = {json.dumps(var_names)}\n"
                    "out = {n: to_jsonable(ns.get(n, None)) for n in names} if names else {}\n"
                    "print(json.dumps(out, ensure_ascii=False))\n"
                )
                with open(runner_path, "w", encoding="utf-8") as r:
                    r.write(
                        runner_code
                    )

                env = os.environ.copy()
                env["PYTHONPATH"] = os.pathsep.join([*EXTRA_PATHS, env.get("PYTHONPATH", "")])
                try:
                    proc = subprocess.run(
                        [sys.executable, "runner.py"],
                        cwd=workdir,
                        capture_output=True,
                        text=True,
                        env=env,
                        timeout=TIMEOUT_S,
                    )
                except subprocess.TimeoutExpired:
                    return Response(
                        {"error": "Script timed out", "timeout_s": TIMEOUT_S},
                        status=408
                    )

            if proc.returncode != 0:
                return Response(
                    {"error": "Script failed", "stderr": proc.stderr, "returncode": proc.returncode},
                    status=status.HTTP_400_BAD_REQUEST
                )

            stdout = (proc.stdout or "").strip()
            try:
                data = json.loads(stdout)
            except json.JSONDecodeError:
                return Response(
                    {"status": "error", "message": "Runner output not JSON", "stdout": stdout},
                    status=400
                )
            return Response(data, status=200)

        # ==== if no var_names -> no JSON response ====
        with tempfile.NamedTemporaryFile(delete=False, suffix=".py") as tmp:
            tmp.write(code_bytes)
            path = tmp.name

        try:
            proc = subprocess.run(["python3", path], capture_output=True, text=True)
        finally:
            try:
                os.unlink(path)
            except OSError:
                pass

        if proc.returncode != 0:
            return Response(
                {"error": "Script failed", "stderr": proc.stderr, "returncode": proc.returncode},
                status=status.HTTP_400_BAD_REQUEST
            )

        out = (proc.stdout or "")
        # if stdout is JSON, return it as JSON; otherwise return as plain text
        try:
            data = json.loads(out)
            return Response(data, status=200)
        except json.JSONDecodeError:
            return HttpResponse(out, content_type="text/plain; charset=utf-8", status=200)