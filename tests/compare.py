#!/usr/bin/env python3
"""Compare two recordings (folders of <fixture>.json or .json.gz from tests/rhino/record.py).

    tests/compare.py tests/goldens tests/out/latest [--rtol 1e-6] [--max 20]

Reports, per fixture and component: changed outputs (first differing path and values), changed runtime
messages, components added or removed, and solve time ratios. Exit code 1 if any output or message differs.
"""
import argparse
import gzip
import json
import math
import os
import sys


def close(a, b, rtol):
    if isinstance(a, bool) or isinstance(b, bool):
        return a == b
    if isinstance(a, (int, float)) and isinstance(b, (int, float)):
        return a == b or abs(a - b) <= rtol * max(1.0, abs(a), abs(b))
    return a == b


def diff(a, b, rtol, path=""):
    """First difference between two JSON values as (path, a, b), or None."""
    if isinstance(a, dict) and isinstance(b, dict):
        for k in sorted(set(a) | set(b)):
            if k not in a or k not in b:
                return (path + "/" + k, a.get(k, "<missing>"), b.get(k, "<missing>"))
            d = diff(a[k], b[k], rtol, path + "/" + k)
            if d:
                return d
        return None
    if isinstance(a, list) and isinstance(b, list):
        if len(a) != len(b):
            return (path + "#len", len(a), len(b))
        for i, (x, y) in enumerate(zip(a, b)):
            d = diff(x, y, rtol, path + "[%d]" % i)
            if d:
                return d
        return None
    return None if close(a, b, rtol) else (path, a, b)


def load(path):
    opener = gzip.open if path.endswith(".gz") else open
    with opener(path, "rt") as f:
        return json.load(f)


def recordings(folder):
    """fixture name -> path, for <name>.json and <name>.json.gz"""
    out = {}
    for n in os.listdir(folder):
        for ext in (".json.gz", ".json"):
            if n.endswith(ext):
                out[n[: -len(ext)]] = os.path.join(folder, n)
    return out


def short(v, n=120):
    s = json.dumps(v)
    return s if len(s) <= n else s[:n] + "…"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("base")
    ap.add_argument("new")
    ap.add_argument("--rtol", type=float, default=1e-6)
    ap.add_argument("--partial", action="store_true", help="ignore fixtures missing from the new recording")
    ap.add_argument("--max", type=int, default=40, help="max differences printed per fixture")
    args = ap.parse_args()
    failed = False
    base, new = recordings(args.base), recordings(args.new)
    for name in sorted(set(base) | set(new)):
        if args.partial and name not in new:
            continue
        if name not in base or name not in new:
            print("%-22s only in %s" % (name, args.base if name in base else args.new))
            failed = True
            continue
        A, B = load(base[name])["components"], load(new[name])["components"]
        lines, timing = [], []
        for comp in sorted(set(A) | set(B)):
            if comp not in A or comp not in B:
                lines.append("  %-28s %s" % (comp, "added" if comp in B else "removed"))
                continue
            a, b = A[comp], B[comp]
            if a["messages"] != b["messages"]:
                lines.append("  %-28s messages: %s -> %s" % (comp, short(a["messages"]), short(b["messages"])))
            for out in sorted(set(a["outputs"]) | set(b["outputs"])):
                d = diff(a["outputs"].get(out), b["outputs"].get(out), args.rtol)
                if d:
                    lines.append("  %-28s %s%s: %s -> %s" % (comp, out, d[0], short(d[1]), short(d[2])))
            if a["ms"] >= 5 or b["ms"] >= 5:
                timing.append((comp, a["ms"], b["ms"]))
        status = "DIFF" if lines else "ok"
        failed |= bool(lines)
        print("%-22s %s" % (name, status))
        for l in lines[: args.max]:
            print(l)
        if len(lines) > args.max:
            print("  … %d more" % (len(lines) - args.max))
        for comp, ta, tb in sorted(timing, key=lambda t: -max(t[1], t[2]))[:6]:
            ratio = (ta / tb) if tb > 0 else math.inf
            print("    time %-26s %9.1f -> %9.1f ms  (%.1fx)" % (comp, ta, tb, ratio))
    sys.exit(1 if failed else 0)


if __name__ == "__main__":
    main()
