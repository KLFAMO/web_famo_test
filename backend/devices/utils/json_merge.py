# devices/utils/json_merge.py
from copy import deepcopy

def deep_merge(base, override):
    """
    Joint two JSON-like structures (dicts, lists, scalars) recursively.
    - If both are dicts, merge their keys recursively.
    - Otherwise, the override value takes precedence.
    """
    if isinstance(base, dict) and isinstance(override, dict):
        result = deepcopy(base)
        for key, override_value in override.items():
            if key in result:
                result[key] = deep_merge(result[key], override_value)
            else:
                result[key] = deepcopy(override_value)
        return result

    # lists and scalars: override entirely
    return deepcopy(override)
