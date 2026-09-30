# Shimmer-Labview-API

LabVIEW support for Shimmer devices. **This repo is mid-migration**, and that shapes almost every
decision in it.

## The migration

LabVIEW support is moving onto a **new wrapper built over the core C# API** (`Shimmer-C-API`). The
old LabVIEW API is being retired and existing users have to migrate onto the wrapper.

The stated reason, from `Readme.md`: the C# API is widely used across research, industry and Shimmer's
own internal tools, so it is the most heavily developed and tested. Aligning LabVIEW to it gives
consistent behaviour and faster fixes rather than a second parallel implementation to maintain.

**The practical consequence:** this is a wrapper, not an implementation. A parsing, protocol,
calibration or timestamp fix belongs in `Shimmer-C-API` — fix it there and it reaches LabVIEW users
through the bridge. Reimplementing that logic here would recreate exactly the divergence the
migration exists to remove.

```
ShimmerLabviewC#Wrapper/   ShimmerBridge.cs, ShimmerLVWrapper.sln/.csproj — the C# bridge
ShimmerLabview/            Bin/, Examples/ — the LabVIEW side
```

The wrapper uses `packages.config`-style NuGet, so restore behaves differently from the
`PackageReference` projects elsewhere in `C:\dev\csharp\`.

## Before changing anything here

Check whether the change belongs in `Shimmer-C-API` instead. If it does, make it there; if it is
genuinely bridge-specific — marshalling, LabVIEW type mapping, the VI surface — it belongs here.
Say which you concluded and why, because the answer is not obvious from inside this repo.

Anything documenting the *old* LabVIEW API is being retired: prefer updating the wrapper's story over
extending the legacy one.
