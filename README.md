# Arcade Sample (Unity 6)

A small Unity 6 arcade sample built as a single-scene flow:

**Splash → Landing menu → mini-games**

## Games

| Menu item | Description |
|-----------|-------------|
| Click the Square | Click the moving square to score |
| Catch the Circle | Click spawning circles before they vanish |
| Coin Race | Top-down click-to-move coin race vs a simple raycast AI (first to 10) |
| Slot Machine | 3-reel slots with **100** starting coins |
| Exit | Quit Play Mode / application |

## Requirements

- Unity **6000.6** (or compatible Unity 6) with **URP**
- **Input System** package (project is Input System–only)

Open `Assets/Scenes/SampleScene.unity` and press **Play**.

## Project layout

```
Assets/
  Scripts/
    ArcadeApp.cs          # Splash / menu / UI mini-games
    CoinRace/             # Top-down coin race
    Slots/                # Slot machine
  Resources/Slots/        # Slot symbols + UI sprites
  Art/Slots/              # Source art + CREDITS
  Editor/                 # Optional editor helpers
```

## Art credits

See `Assets/Art/Slots/CREDITS.txt`.

- Slot symbols: OpenGameArt *Slot Machine Resource Pack* (CC-BY) — Molly "Cougarmint" Willits  
- UI chrome: Kenney UI Pack (CC0)

## Privacy / what is not in this repo

Local Unity junk and secrets are ignored via `.gitignore`:

- `Library/`, `Temp/`, `Logs/`, `UserSettings/`
- IDE files, generated `.csproj` / `.sln`
- MCP configs, env/credential files
- Raw art extracts under `Assets/Art/Slots/_raw/`

Unity Cloud `cloudProjectId` / `organizationId` were cleared for this public sample.

## Optional editor menus

Under **Tools → Arcade Sample**:

- **Configure Slot Sprites** — force PNG import as Sprites  
- **Restore SampleScene Build Settings** — ensure Build Settings starts on SampleScene  
- **Setup Scenes** — disabled by default (asks before regenerating old multi-scene files; does not change Build Settings)

## License

Sample code is provided as-is for learning. Respect third-party art licenses listed in `CREDITS.txt`.
