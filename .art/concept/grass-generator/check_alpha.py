#!/usr/bin/env python3
"""Validate a cutout per .agents/skills/make-game-art/SKILL.md 'Verifying the cutout'."""
import sys
from pathlib import Path
from PIL import Image

def check(path):
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    alpha = im.getchannel("A")
    extrema = alpha.getextrema()
    px = im.load()
    corners = [px[0, 0][3], px[w - 1, 0][3], px[0, h - 1][3], px[w - 1, h - 1][3]]
    total = w * h
    transparent = sum(1 for y in range(h) for x in range(w) if px[x, y][3] < 8)
    transparent_share = transparent / total
    bright_desat = 0
    opaque = 0
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a < 8:
                continue
            opaque += 1
            mx, mn = max(r, g, b), min(r, g, b)
            sat = 0 if mx == 0 else (mx - mn) / mx
            val = mx / 255
            if sat < 0.15 and val > 0.75:
                bright_desat += 1
    bright_share = bright_desat / opaque if opaque else 0
    print(f"{path}: size={w}x{h} mode={im.mode}")
    print(f"  alpha extrema: {extrema}")
    print(f"  corner alphas: {corners}")
    print(f"  transparent share: {transparent_share:.3f}")
    print(f"  bright-desaturated share of opaque px: {bright_share:.4f} ({bright_desat}/{opaque})")
    ok = all(c == 0 for c in corners) and transparent_share > 0.15 and bright_share < 0.05
    print(f"  PASS" if ok else "  FAIL - inspect visually")
    return ok

if __name__ == "__main__":
    for p in sys.argv[1:]:
        check(p)
