#!/usr/bin/env python3
"""Print technical measurements for audio review candidates.

WAV files are decoded with the standard library. Other formats (MP3) use
ffprobe when available, else the optional `soundfile` package (libsndfile
1.1+ decodes MP3; install with `pip install numpy soundfile` in a venv).

Reported per file: duration, channels, sample rate, peak/RMS dBFS,
trailing silence, end-of-file click level, and tail re-rise after the main
decay. These are measurements, not a listening pass — only the user's
listening approves a candidate.
"""

import argparse
import json
import math
import shutil
import subprocess
import sys
import wave
from array import array
from pathlib import Path


def db(value: float, full_scale: float = 1.0) -> float:
    if value <= 0:
        return float("-inf")
    return 20.0 * math.log10(value / full_scale)


def pcm_values(frames: bytes, sample_width: int):
    if sample_width == 1:
        return [value - 128 for value in frames]
    if sample_width in (2, 4):
        values = array("h" if sample_width == 2 else "i")
        values.frombytes(frames)
        if sys.byteorder != "little":
            values.byteswap()
        return values
    if sample_width == 3:
        return [
            int.from_bytes(frames[index:index + 3], "little", signed=True)
            for index in range(0, len(frames), 3)
        ]
    raise ValueError(f"unsupported PCM sample width: {sample_width}")


def tail_metrics(samples, sample_rate: int) -> dict:
    """Envelope-based tail checks on float samples in [-1, 1]."""
    if not len(samples):
        return {}
    win = max(1, int(sample_rate * 0.005))
    env = []
    for start in range(0, len(samples) - win + 1, win):
        chunk = samples[start:start + win]
        env.append(math.sqrt(sum(v * v for v in chunk) / len(chunk)))
    if not env:
        return {}
    env_db = [db(v) for v in env]
    peak_win = max(range(len(env_db)), key=lambda i: env_db[i])
    peak_db = env_db[peak_win]
    decay_end = next(
        (i for i in range(peak_win, len(env_db)) if env_db[i] < peak_db - 40),
        len(env_db) - 1,
    )
    rerise = max(
        (env_db[i] - peak_db for i in range(decay_end, len(env_db))),
        default=float("-inf"),
    )
    tail3ms = samples[-max(1, int(sample_rate * 0.003)):]
    end_click_db = db(max(abs(v) for v in tail3ms))
    last_loud = next(
        (i for i in range(len(samples) - 1, -1, -1) if abs(samples[i]) > 10 ** (-60 / 20)),
        -1,
    )
    trailing_silence = (len(samples) - 1 - last_loud) / sample_rate if last_loud >= 0 else None
    return {
        "decay_to_-40dB_s": round(decay_end * win / sample_rate, 3),
        "tail_rerise_db_rel_peak": None if rerise == float("-inf") else round(rerise, 1),
        "end_last3ms_dbfs": round(end_click_db, 1),
        "trailing_silence_s": round(trailing_silence, 4) if trailing_silence is not None else None,
    }


def summarize(samples, channels: int, sample_rate: int, fmt: str, path: Path) -> dict:
    peak = max((abs(v) for v in samples), default=0.0)
    rms = math.sqrt(sum(v * v for v in samples) / len(samples)) if len(samples) else 0.0
    payload = {
        "file": str(path),
        "format": fmt,
        "duration_seconds": round(len(samples) / sample_rate, 3),
        "channels": channels,
        "sample_rate_hz": sample_rate,
        "peak_dbfs": round(db(peak), 2),
        "rms_dbfs": round(db(rms), 2),
    }
    payload.update(tail_metrics(samples, sample_rate))
    return payload


def probe_wav(path: Path) -> dict:
    with wave.open(str(path), "rb") as stream:
        channels = stream.getnchannels()
        sample_width = stream.getsampwidth()
        sample_rate = stream.getframerate()
        frames = stream.readframes(stream.getnframes())
    values = pcm_values(frames, sample_width)
    full_scale = float((1 << (sample_width * 8 - 1)) - 1)
    mono = [
        sum(values[i:i + channels]) / channels / full_scale
        for i in range(0, len(values) - channels + 1, channels)
    ]
    return summarize(mono, channels, sample_rate, "wav", path)


def probe_with_soundfile(path: Path) -> dict:
    import soundfile as sf  # optional dependency
    data, sample_rate = sf.read(str(path), always_2d=True)
    mono = data.mean(axis=1).tolist()
    return summarize(mono, data.shape[1], sample_rate, path.suffix.lstrip("."), path)


def probe_with_ffprobe(path: Path) -> dict:
    command = [
        "ffprobe", "-v", "error",
        "-show_entries", "format=duration,format_name:stream=channels,sample_rate",
        "-of", "json", str(path),
    ]
    result = subprocess.run(command, check=True, capture_output=True, text=True)
    payload = json.loads(result.stdout)
    stream = payload.get("streams", [{}])[0]
    audio_format = payload.get("format", {})
    return {
        "file": str(path),
        "format": audio_format.get("format_name"),
        "duration_seconds": round(float(audio_format["duration"]), 3),
        "channels": stream.get("channels"),
        "sample_rate_hz": int(stream["sample_rate"]) if stream.get("sample_rate") else None,
        "note": "Peak/RMS/tail require decoded PCM; install soundfile or inspect the final WAV.",
    }


def probe(path: Path) -> dict:
    if path.suffix.lower() == ".wav":
        return probe_wav(path)
    try:
        return probe_with_soundfile(path)
    except ImportError:
        pass
    if shutil.which("ffprobe"):
        return probe_with_ffprobe(path)
    return {
        "file": str(path),
        "error": "non-WAV probe needs the soundfile package or ffprobe",
    }


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("files", nargs="+", type=Path)
    args = parser.parse_args()
    for path in args.files:
        try:
            payload = probe(path)
        except (OSError, ValueError, subprocess.SubprocessError, wave.Error) as error:
            payload = {"file": str(path), "error": str(error)}
        print(json.dumps(payload, ensure_ascii=False))


if __name__ == "__main__":
    main()
