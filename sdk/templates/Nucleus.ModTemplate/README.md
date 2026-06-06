# MyMod — a Nucleus mod for Nuclear Option

Scaffolded by `dotnet new nucleus-mod`. A thin BepInEx plugin (`Plugin.cs`) that registers an `IMod`
(`Mod.cs`) with the Nucleus platform; the platform owns the shared overlay canvas, the per-frame tick, and
the game services.

## Build
1. **Bring your own game DLLs** — run the Nucleus `setup-sdk` script to populate `lib/` from your local
   Nuclear Option install (the game's assemblies are its IP and are never shipped in the SDK packages).
2. `dotnet build -c Release` — produces `MyMod.dll`.
3. Copy `MyMod.dll` into `BepInEx/plugins/MyMod/` alongside the installed **Nucleus Platform** (which carries
   the shared `Nucleus.*` libraries). Or publish to Thunderstore using `thunderstore/manifest.json`.

## Where to start
- `Mod.Initialize` — claim a map-bezel button (`ctx.Buttons.RegisterMapButton`) and create a UI layer.
- `Mod.Tick` — runs each map frame while enabled; read `ctx.Game.Roster()` / `ctx.Game.KnownEnemiesNear(...)`.
- `ctx.Log` — logs through BepInEx. `ctx.Ui.Theme` — the faction-themed palette.

The mod appears in the in-game **MODS** loader and can be toggled at runtime.
