#!/usr/bin/env python3
"""프레임 쌍이 교체될 때 대상이 튀는지 world 좌표로 잰다.

`OnAttackSpriteSwapper` 와 `AttackSpriteSwapController` 는 Sprite 만 바꾸고
Transform 을 건드리지 않는다. 그래서 두 프레임에서 몸이 같은 world 위치에
있어야 한다. 캔버스 크기와 `spritePixelsToUnits` 가 프레임마다 다를 수 있으니
픽셀이 아니라 world 단위로 비교해야 한다. `TreeGolem` 은 실제로 100 과 80 으로
갈려 있었다.

    python3 .art/tools/check-frame-pair.py <base.png> <attack.png>

가로 차이는 공격 프레임이 아래로 뻗는 요소(날아가는 돌, 물줄기)를 가지면
실제보다 크게 나온다. 숫자가 크면 두 프레임을 겹쳐 보고, 몸통이 옮겨간 것인지
던지는 요소 때문인지 구분해라.
"""

from __future__ import annotations

import re
import sys

import numpy as np
from PIL import Image

# 이 안쪽이면 교체할 때 눈에 띄지 않는다.
VERTICAL_LIMIT = 0.03
HORIZONTAL_LIMIT = 0.05


def read(path: str) -> dict:
    meta = open(path + '.meta').read()
    ppu = int(re.search(r'spritePixelsToUnits: (\d+)', meta).group(1))
    pivot = re.search(r'spritePivot: \{x: ([\d.]+), y: ([\d.]+)\}', meta)
    px, py = (float(pivot.group(1)), float(pivot.group(2))) if pivot else (0.5, 0.5)

    a = np.array(Image.open(path).convert('RGBA'))
    ys, xs = np.where(a[:, :, 3] > 10)
    h, w = a.shape[0], a.shape[1]
    ymax = ys.max()
    # 바닥 10% 구간의 가로 중심을 접지점으로 본다. 몸 전체 중심을 쓰면
    # 치켜든 팔이나 날아가는 돌에 끌려간다.
    band = ys >= ymax - max(2, int((ymax - ys.min()) * 0.1))
    return {
        'name': path.split('/')[-1],
        'size': (w, h),
        'ppu': ppu,
        'bottom': (h * py - (ymax + 1)) / ppu,
        'foot_x': (xs[band].mean() - w * px) / ppu,
    }


def main(base_path: str, attack_path: str) -> int:
    base, attack = read(base_path), read(attack_path)
    for frame, label in ((base, 'base  '), (attack, 'attack')):
        print(f'{label} {frame["name"]:26s} {frame["size"][0]}x{frame["size"][1]} '
              f'PPU {frame["ppu"]}  바닥 {frame["bottom"]:+.3f}  접점 x {frame["foot_x"]:+.3f}')
    dy = attack['bottom'] - base['bottom']
    dx = attack['foot_x'] - base['foot_x']
    ok = abs(dy) < VERTICAL_LIMIT and abs(dx) < HORIZONTAL_LIMIT
    print(f'차이   세로 {dy:+.3f} unit, 가로 {dx:+.3f} unit  -> '
          f'{"통과" if ok else "튄다, 고쳐야 한다"}')
    return 0 if ok else 1


if __name__ == '__main__':
    sys.exit(main(*sys.argv[1:3]))
