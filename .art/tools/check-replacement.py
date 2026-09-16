#!/usr/bin/env python3
"""교체한 스프라이트가 옛 파일의 기하를 물려받았는지 검사한다.

대조 시트는 모든 항목을 한 칸에 맞춰 보여 주므로 크기·바닥·방향을 전부
가린다. 이 세 가지는 눈이 아니라 숫자로 걸러야 한다. 2026-09-15 의 교체
작업에서 `crater_ember` 가 절반 크기로, `LifeTree` 가 지면에서 30px 뜬 채로,
소환 슬라임 네 장이 왼쪽을 본 채로 시트 검수를 통과했다.

    python3 .art/tools/check-replacement.py <base-rev> [<head-rev>]

`<base-rev>` 와 `<head-rev>` 사이에서 바뀐 PNG 를 모두 찾아, 각 파일의 옛
버전과 새 버전을 비교한다. `<head-rev>` 를 생략하면 작업 트리의 파일을 쓴다.
"""

from __future__ import annotations

import io
import subprocess
import sys

import numpy as np
from PIL import Image

# 8% 이상 작아지면 게임 안에서 눈에 띈다.
SHRINK_LIMIT = 1.08
# 바닥 여백이 이보다 늘면 지면에서 뜬 것으로 본다.
BOTTOM_LIMIT = 3
# 눈 위치가 몸통 중심에서 이만큼 벗어나야 방향이 있다고 본다.
FACING_DEADZONE = 5.0


def show(rev: str, path: str) -> bytes | None:
    result = subprocess.run(['git', 'show', f'{rev}:{path}'], capture_output=True)
    return result.stdout if result.returncode == 0 else None


def load(rev: str | None, path: str) -> Image.Image | None:
    if rev is None:
        try:
            return Image.open(path).convert('RGBA')
        except FileNotFoundError:
            return None
    data = show(rev, path)
    return Image.open(io.BytesIO(data)).convert('RGBA') if data else None


def measure(im: Image.Image) -> dict:
    a = np.array(im)
    opaque = a[:, :, 3] > 10
    ys, xs = np.where(opaque)
    if xs.size == 0:
        return {}
    rgb = a[:, :, :3].astype(int)
    # 눈 잉크만 고른다. 밝기만 보면 나무 골렘의 그림자 면이 눈으로 잡혀
    # 방향이 뒤집힌 것처럼 읽힌다. 눈은 무채색이라 세 채널이 붙어 있다.
    spread = rgb.max(axis=2) - rgb.min(axis=2)
    eyes = opaque & (rgb.mean(axis=2) < 70) & (spread < 40)
    width = max(xs.max() - xs.min() + 1, 1)
    centre = (xs.min() + xs.max()) / 2
    return {
        'canvas': im.size,
        'content': (width, ys.max() - ys.min() + 1),
        'bottom': a.shape[0] - 1 - ys.max(),
        'facing': (np.where(eyes)[1].mean() - centre) / width * 100 if eyes.sum() >= 20 else None,
    }


def compare(name: str, old: dict, new: dict) -> list[str]:
    problems = []
    if old['canvas'] != new['canvas']:
        problems.append(f'캔버스가 {old["canvas"]} 에서 {new["canvas"]} 로 바뀌었다')
    scale = min(old['content'][0] / new['content'][0], old['content'][1] / new['content'][1])
    if scale >= SHRINK_LIMIT:
        problems.append(
            f'{scale:.2f}배 작다 (내용 {new["content"][0]}x{new["content"][1]}, '
            f'옛 {old["content"][0]}x{old["content"][1]})')
    if new['bottom'] - old['bottom'] >= BOTTOM_LIMIT:
        problems.append(f'바닥 여백이 {old["bottom"]}px 에서 {new["bottom"]}px 로 늘었다')
    of, nf = old['facing'], new['facing']
    if of is not None and nf is not None:
        if abs(of) > FACING_DEADZONE and abs(nf) > FACING_DEADZONE and (of > 0) != (nf > 0):
            problems.append(f'보는 방향이 뒤집혔다 ({of:+.0f}% -> {nf:+.0f}%)')
    return problems


def main(base: str, head: str | None) -> int:
    rev_range = f'{base}..{head}' if head else base
    changed = subprocess.run(
        ['git', 'diff', '--name-only', rev_range, '--', '*.png'],
        capture_output=True, text=True).stdout.split()
    failed = 0
    for path in changed:
        old_im, new_im = load(base, path), load(head, path)
        if old_im is None or new_im is None:
            continue
        old, new = measure(old_im), measure(new_im)
        if not old or not new:
            continue
        problems = compare(path, old, new)
        if problems:
            failed += 1
            print(f'{path.split("/")[-1]}')
            for p in problems:
                print(f'    {p}')
    print(f'검사 {len(changed)}장, 문제 {failed}장')
    return 1 if failed else 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else None))
