# Animal Ark Shelter (GTA V Mod)

A lightweight GTA V gameplay mod that lets you visit an Animal Ark pet store, adopt a pet (dogs/cat), manage it with an interaction menu, and monitor health with a compact HUD. Built with ScriptHookV + ScriptHookVDotNet 3 and LemonUI.

## Requirements
- ScriptHookV (Alexander Blade)
- ScriptHookVDotNet v3 (ScriptHookVDotNet3.dll)
- LemonUI for SHVDN3 (LemonUI.SHVDN3.dll)
- Grand Theft Auto V (latest PC build recommended)

## Installation
1. Ensure `ScriptHookV` and `ScriptHookVDotNet3` are installed correctly (game root has `dinput8.dll`, and a `scripts` folder exists).
2. Copy the following files into your GTA V `scripts` folder:
   - `AnimalArkShelter.dll`
   - `AnimalArkShelter.ini`
   - `LemonUI.SHVDN3.dll` (if you don’t already have LemonUI installed)
3. Launch the game. A shop blip will appear on the map near the Animal Ark Pet Store.

If you’re building from source, the repo contains `dist/` with a prebuilt DLL and INI you can copy into `scripts/` directly.

## How To Play
- Go to the Animal Ark shop marker. When nearby, press the in‑game Context key (`E` by default) to open the shop.
- Browse and adopt a pet. You’ll be prompted to name it.
- A health HUD appears when you have a pet.
- Press the Interaction Menu key (`J` by default) to control your pet:
  - Follow, Stay, Come Here
  - Sit, Lay Down
  - Enter Vehicle / Exit Vehicle (with pose)
  - Treat (+10 HP)
  - Dismiss Pet

## Controls (default)
- Interaction Menu: `J` (configurable in INI)
- Open Shop: Context key near shop (`E` by default in GTA V)

## Configuration (`AnimalArkShelter.ini`)
Edit this file in your `scripts` folder to customize behavior.

- [Keys]
  - `InteractionMenuKey`: Key for Pet Interaction Menu (default: `J`).
- [Shop]
  - `X`, `Y`, `Z`: World position of the shop marker/blip.
  - `CameraOffsetX/Y/Z`: Fixed world offset from the shop anchor (X=east, Y=north, Z=up).
  - `ShowcaseOffsetX/Y/Z`: Fixed world offset where the showcase animal spawns.
  - `FOV`: Camera field of view for the shop showcase.
  - `EaseTimeMs`: Camera ease time when entering/exiting showcase.
  - `EnableWalkOffAnim`, `WalkOffDistance`: Small walk sequence after adoption.
- [ComeHere]
  - `WarpIfFar`, `WarpDistance`: Warp the pet if it’s very far.
  - `RunSpeed`, `StopRange`, `TeleportIfStuckMs`: Tuning for the “Come Here” behavior.
- [Vehicle]
  - `SeatIndex`: Seat index used when entering vehicles.
  - `Pose`: 0 = sit, 1 = lay when inside vehicles.
- [HUD]
  - `Scale`, `X`, `Y`: HUD text scale and position.
  - `UseOutline`, `UseShadow`, `TextR/G/B/A`: Label styling.
  - `FillAlpha`, `BackAlpha`: Health bar fill and background transparency.

Tip: Camera and showcase offsets are fixed world offsets from the shop anchor. If the framing looks off, tweak `CameraOffset*` and `ShowcaseOffset*` in the INI.

## Features
- Pet adoption with a simple in‑game shop and blip.
- Clean LemonUI menus for purchasing and interactions.
- Pet HUD with centered health label.
- Interaction menu: following, staying, coming, sitting, laying, vehicle enter/exit, healing, dismissing.
- Ground‑aware spawning and shop camera that avoids clipping with terrain.

## Troubleshooting
- Mod not loading: verify `ScriptHookV` and `ScriptHookVDotNet3` are up‑to‑date and installed; ensure files are in `Grand Theft Auto V\scripts`.
- Shop or menu not showing: confirm `LemonUI.SHVDN3.dll` is present in `scripts`; check for conflicting UI mods; approach the shop marker within ~1.5m.
- Camera outside/odd angle: tweak `CameraOffset*` and `ShowcaseOffset*` in the INI.
- Pet missing or stuck: use the Interaction Menu > Come Here; if far, enable warping in `[ComeHere]`.

## Build From Source
- Open `AnimalArkShelter.sln` in Visual Studio (target .NET Framework 4.8, x64).
- External references are included under `ref/` (LemonUI.SHVDN3.dll, ScriptHookVDotNet3.dll). Update if you have newer versions.
- Build `Release|x64`. Copy the resulting `AnimalArkShelter.dll` and `AnimalArkShelter.ini` (auto-copied template exists under `AnimalArkShelter/`) into your GTA V `scripts` folder.

## Credits
- ScriptHookV — Alexander Blade
- ScriptHookVDotNet — crosire and contributors
- LemonUI — Lemon and contributors
- Animal Ark Shelter mod code — this repository

## Disclaimer
This project is a fan‑made mod for GTA V. Use at your own risk. Not affiliated with Rockstar Games.
