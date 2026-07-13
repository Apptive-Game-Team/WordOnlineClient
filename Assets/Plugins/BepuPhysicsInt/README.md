# BEPUphysicsint binaries

- Source: `https://github.com/sam-vdp/bepuphysics1int`
- Commit: `9237daa68c3014fd7c2e93c6a99326ba5248d60b`
- Build: Mono MSBuild, `Release|AnyCPU`, from a clean `git archive`
- Assemblies: `FixMath.NET.dll`, `BEPUutilities.dll`, `BEPUphysics.dll`
- License: see `LICENSE.md` in this directory

SHA-256:

- `BEPUphysics.dll`: `b11e8c5173464c55bfe99ff55f3ab619f1e80ea8546d110591453e0319d82ef7`
- `BEPUutilities.dll`: `dba59ba81f902f580099168e373c4e9067814291d2a1c919610ec4d25a7660fe`
- `FixMath.NET.dll`: `46bc707070083203b96c80430c89ff462171c1b20931e3f63d378b65fd9b60d0`

The checked-in binaries make Editor, CI, and WebGL builds independent from an
absolute local checkout. Replace them only through a reviewed, reproducible
source commit and rerun deterministic replay tests.
