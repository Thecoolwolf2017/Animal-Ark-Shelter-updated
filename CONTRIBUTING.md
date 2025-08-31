# Contributing to Animal Ark Shelter

Thanks for your interest in contributing!

## Dev setup
- Install ScriptHookV and ScriptHookVDotNet v3
- Clone the repo, open `AnimalArkShelter.sln` in Visual Studio
- Target .NET Framework 4.8, x64
- Place required DLLs in `ref/` (LemonUI.SHVDN3.dll, ScriptHookVDotNet3.dll)

## Build
- Build `Release|x64`
- The GitHub Actions workflow also builds on push and uploads artifacts

## PR guidelines
- Keep changes small and focused
- Follow existing code style (braces, naming)
- Update docs and INI defaults if behavior/config changes
- Test in‑game before opening a PR

## Reporting issues
- Use the Bug report template with details: game build, SHV/SHVDN versions, steps, logs

