# devices/utils/__init__.py
import re

def normalize_mac(s: str) -> str:
    hex_only = re.sub(r'[^0-9A-Fa-f]', '', s or '')
    if len(hex_only) != 12:
        return ''
    return hex_only.upper()