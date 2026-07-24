#!/usr/bin/env python3
"""Generate a deterministic procedural forest-air review candidate."""

import argparse
import math
import random
import wave
from array import array
from pathlib import Path


PRESETS = {
    "gentle": {
        "base": 0.045,
        "gain": 0.92,
        "gusts": [(5.0, 3.0, 0.42), (16.0, 3.6, 0.48), (26.0, 1.8, 0.32)],
    },
    "moderate": {
        "base": 0.055,
        "gain": 0.92,
        "gusts": [(4.2, 3.4, 0.55), (12.8, 2.2, 0.38), (20.5, 4.6, 0.62), (27.2, 1.5, 0.30)],
    },
    "breezy": {
        "base": 0.065,
        "gain": 1.95,
        "gusts": [(2.8, 4.2, 0.68), (9.6, 2.8, 0.46), (15.2, 4.0, 0.58), (21.8, 5.1, 0.72), (28.2, 1.5, 0.40)],
    },
}


def smooth_envelope(time: float, start: float, duration: float) -> float:
    position = (time - start) / duration
    if position <= 0.0 or position >= 1.0:
        return 0.0
    return math.sin(math.pi * position) ** 2


def generate(output: Path, preset_name: str, duration: float, sample_rate: int, seed: int) -> None:
    settings = PRESETS[preset_name]
    rng = random.Random(seed)
    samples = array("h")
    slow = 0.0
    medium = 0.0
    leaf_envelope = 0.0
    leaf_target = 0.0

    for index in range(round(duration * sample_rate)):
        time = index / sample_rate
        noise = rng.uniform(-1.0, 1.0)
        slow += 0.0012 * (noise - slow)
        medium += 0.014 * (noise - medium)

        gust = sum(
            strength * smooth_envelope(time, start, gust_duration)
            for start, gust_duration, strength in settings["gusts"]
        )
        air = settings["base"] * (0.55 * slow + 0.22 * medium)
        wind = gust * (0.40 * slow + 0.14 * medium)

        if rng.random() < 0.00003 + gust * 0.00012:
            leaf_target = rng.uniform(0.025, 0.055) * (0.35 + gust)
        leaf_target *= 0.99965
        leaf_envelope += 0.003 * (leaf_target - leaf_envelope)
        leaf = (medium - slow) * leaf_envelope * 0.75

        value = max(-1.0, min(1.0, (air + wind + leaf) * settings["gain"]))
        samples.append(round(value * 32767))

    output.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(output), "wb") as stream:
        stream.setnchannels(1)
        stream.setsampwidth(2)
        stream.setframerate(sample_rate)
        stream.writeframes(samples.tobytes())


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--preset", choices=PRESETS, default="moderate")
    parser.add_argument("--duration", type=float, default=30.0)
    parser.add_argument("--sample-rate", type=int, default=48000)
    parser.add_argument("--seed", type=int, default=377)
    args = parser.parse_args()
    if args.duration <= 0 or args.sample_rate < 8000:
        parser.error("duration must be positive and sample-rate must be at least 8000")
    generate(args.output, args.preset, args.duration, args.sample_rate, args.seed)


if __name__ == "__main__":
    main()
