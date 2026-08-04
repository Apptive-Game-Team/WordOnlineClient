#!/usr/bin/env python3
"""Post-process a generated SFX candidate into an audition-ready one-shot.

Chain: decode -> mono mix -> strip leading silence -> trim to the event
(optional --target-duration) -> fade the tail to silence -> normalize peak ->
write mono 16-bit 44.1 kHz WAV next to the input as `<stem>_proc.wav`.

The original file is never modified. Requires numpy + soundfile
(`python3 -m venv venv && venv/bin/pip install numpy soundfile`).

This prepares candidates for USER listening. It is a measurement/processing
step, not a listening pass.
"""

import argparse
import json
import math
import sys
from pathlib import Path

try:
    import numpy as np
    import soundfile as sf
except ImportError:
    sys.exit(
        "postprocess_candidate.py needs numpy and soundfile:\n"
        "  python3 -m venv /tmp/sfx-venv && /tmp/sfx-venv/bin/pip install numpy soundfile\n"
        "  /tmp/sfx-venv/bin/python postprocess_candidate.py ..."
    )

TARGET_SR = 44100
SILENCE_DB = -60.0
FADE_MS = 15.0
LEAD_PAD_MS = 3.0


def db_to_lin(db: float) -> float:
    return 10 ** (db / 20)


def resample_linear(mono: "np.ndarray", sr: int, target_sr: int) -> "np.ndarray":
    if sr == target_sr:
        return mono
    duration = len(mono) / sr
    target_len = int(round(duration * target_sr))
    src_t = np.linspace(0.0, duration, num=len(mono), endpoint=False)
    dst_t = np.linspace(0.0, duration, num=target_len, endpoint=False)
    return np.interp(dst_t, src_t, mono)


def process(path: Path, peak_db: float, target_duration: float | None) -> dict:
    data, sr = sf.read(str(path), always_2d=True)
    mono = data.mean(axis=1)
    if not len(mono):
        raise ValueError("empty audio")

    mono = resample_linear(mono, sr, TARGET_SR)
    sr = TARGET_SR

    src_peak = float(np.max(np.abs(mono)))
    if src_peak <= 0:
        raise ValueError("silent file (peak 0)")
    src_peak_db = 20 * math.log10(src_peak)
    if src_peak_db < -20:
        raise ValueError(
            f"source peak {src_peak_db:.1f} dBFS is too quiet (auto-reject rule: below -20 dBFS); "
            "regenerate instead of boosting"
        )

    # strip leading silence, keep a tiny pre-transient pad
    thresh = src_peak * db_to_lin(SILENCE_DB)
    loud = np.where(np.abs(mono) > thresh)[0]
    start = max(0, int(loud[0] - sr * LEAD_PAD_MS / 1000))
    end = int(loud[-1]) + 1
    mono = mono[start:end]

    # optional hard cap to the event length
    if target_duration is not None:
        cap = int(sr * target_duration)
        if len(mono) > cap:
            mono = mono[:cap]

    # fade the tail to true silence
    fade_len = min(len(mono), max(8, int(sr * FADE_MS / 1000)))
    mono[-fade_len:] *= np.linspace(1.0, 0.0, fade_len) ** 2

    # normalize peak
    peak = float(np.max(np.abs(mono)))
    mono = mono * (db_to_lin(peak_db) / peak)

    out = path.with_name(path.stem + "_proc.wav")
    sf.write(str(out), mono.astype(np.float64), sr, subtype="PCM_16")
    return {
        "input": str(path),
        "output": str(out),
        "source_peak_dbfs": round(src_peak_db, 1),
        "output_duration_s": round(len(mono) / sr, 3),
        "output_peak_dbfs": peak_db,
        "sample_rate_hz": sr,
        "format": "mono PCM_16 WAV",
    }


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("files", nargs="+", type=Path)
    parser.add_argument("--peak", type=float, default=-3.0, help="target peak dBFS (default -3)")
    parser.add_argument(
        "--target-duration", type=float, default=None,
        help="hard cap in seconds; tail past this is trimmed then faded",
    )
    args = parser.parse_args()
    failed = False
    for path in args.files:
        try:
            print(json.dumps(process(path, args.peak, args.target_duration), ensure_ascii=False))
        except Exception as error:  # report per file, keep batch going
            failed = True
            print(json.dumps({"input": str(path), "error": str(error)}, ensure_ascii=False))
    sys.exit(1 if failed else 0)


if __name__ == "__main__":
    main()
