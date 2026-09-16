#!/usr/bin/env python3
"""새로 그린 스프라이트를 옛 파일의 크기와 바닥 위치에 맞춘다.

교체본은 옛 파일의 이름만이 아니라 기하도 물려받는다. prefab 이 sprite 크기로
화면 크기를 정하므로, 캔버스가 같아도 그림이 그 안에서 작으면 게임에서 작게
보이고, 바닥에 여백이 생기면 지면에서 뜬다. 정사각으로 생성해 비정사각
캔버스에 넣는 과정에서 매번 생기는 일이다.

    python3 .art/tools/fit-to-original.py <old.png> <new.png> <out.png>

옛 그림의 bounding box 안에 비율을 유지한 채 넣고, 바닥 여백을 옛 값에 맞춘다.
가로세로 비 자체가 달라진 경우에는 늘이지 않는다 — 그건 그림을 다시 뽑을
문제다.

프레임 쌍에는 프레임마다 따로 쓰지 마라. 두 프레임의 배율이 갈리면 공격할 때
크기가 튄다. 쌍은 한 배율을 공유해야 한다.
"""

from __future__ import annotations

import sys

import numpy as np
from PIL import Image


def bbox(im: Image.Image) -> tuple[int, int, int, int]:
    a = np.array(im)
    ys, xs = np.where(a[:, :, 3] > 10)
    return xs.min(), ys.min(), xs.max(), ys.max()


def main(old_path: str, new_path: str, out_path: str) -> None:
    old = Image.open(old_path).convert('RGBA')
    new = Image.open(new_path).convert('RGBA')
    if old.size != new.size:
        raise SystemExit(f'캔버스 크기가 다르다: {old.size} vs {new.size}')

    ox0, oy0, ox1, oy1 = bbox(old)
    nx0, ny0, nx1, ny1 = bbox(new)
    old_w, old_h = ox1 - ox0 + 1, oy1 - oy0 + 1
    new_w, new_h = nx1 - nx0 + 1, ny1 - ny0 + 1

    scale = min(old_w / new_w, old_h / new_h)
    content = new.crop((nx0, ny0, nx1 + 1, ny1 + 1))
    target = (max(1, round(new_w * scale)), max(1, round(new_h * scale)))
    content = content.resize(target, Image.LANCZOS)

    canvas = Image.new('RGBA', old.size, (0, 0, 0, 0))
    x = (old.size[0] - target[0]) // 2
    y = old.size[1] - (old.size[1] - 1 - oy1) - target[1]
    canvas.paste(content, (x, max(0, y)), content)
    canvas.save(out_path)

    a = np.array(canvas)
    ys, _ = np.where(a[:, :, 3] > 10)
    print(f'{out_path.split("/")[-1]}: 내용 {new_w}x{new_h} -> {target[0]}x{target[1]} '
          f'(옛 {old_w}x{old_h}), 배율 {scale:.2f}, '
          f'바닥 여백 {a.shape[0] - 1 - ys.max()}px (옛 {old.size[1] - 1 - oy1}px)')


if __name__ == '__main__':
    main(*sys.argv[1:4])
