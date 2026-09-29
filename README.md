# Valheim Seed Crafting

BepInEx mod for [Valheim](https://www.valheimgame.com/) that adds a workbench
recipe to convert one fully-grown crop back into 2 seeds of the same type,
so you don't have to keep buying seeds from the trader.

Recipes (require a workbench nearby):

| Input (1x)  | Output (2x)   |
|-------------|---------------|
| Carrot      | CarrotSeeds   |
| Turnip      | TurnipSeeds   |
| Onion       | OnionSeeds    |
| Kale        | KaleSeeds     |
| Poteitr     | PoteitrSeeds  |

No other resources are consumed. Doesn't touch crafting stations you're
not using, doesn't patch anything unrelated.

> **Unofficial mod.** This is a fan-made mod, not affiliated with or endorsed by
> Iron Gate. It marks your game as modded (the game shows this in the main menu),
> as Iron Gate asks mod authors to do.

## Requirements

- Valheim (tested on 1.0.15)
- [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) 5.4.x

## Installation (players)

1. Install BepInEx for Valheim if you haven't already (see link above, or
   use [r2modman](https://valheim.thunderstore.io/package/ebkr/r2modman/)).
2. Download `SeedCrafting.dll` from the
   [latest release](../../releases/latest).
3. Drop it into `<Valheim install folder>\BepInEx\plugins\SeedCrafting\`.
4. Launch the game.

**Upgrading from 0.1.0:** the DLL used to be called `Mod1-SeedCrafting.dll`.
Delete the old `BepInEx\plugins\Mod1-SeedCrafting\` folder after installing
the new version (the mod keeps no data there).

## Building from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) (or newer)
and a local Valheim install with BepInEx installed.

```bash
git clone https://github.com/Ab5oluteZer0/valheim-seed-crafting.git
cd valheim-seed-crafting
dotnet build -c Release -p:ValheimPath="C:\Path\To\Valheim"
```

If you don't pass `-p:ValheimPath`, the build looks for a `VALHEIM_PATH`
environment variable, then falls back to the default Steam location
(`C:\Program Files (x86)\Steam\steamapps\common\Valheim`).

The build automatically copies the built DLL into
`<Valheim>\BepInEx\plugins\SeedCrafting\` for quick in-game testing.

## Known limitations

- This mod bypasses the [Jötunn](https://valheim-modding.github.io/Jotunn/)
  modding library's recipe API on purpose: as of Jötunn 2.30.2, its
  `ItemManager.OnVanillaItemsAvailable` event fires once, too early (on an
  empty `ObjectDB`, before a world is loaded) and never fires again -
  registered recipes never resolve to real item prefabs and stay invisible
  in-game. This mod registers the recipe directly against the game's own
  `ObjectDB.m_recipes` from a Harmony postfix on `ObjectDB.Awake`, guarded to
  only run once real item data is present.
- Kale/Poteitr item names are assumed to follow the game's own
  `{CropName}` / `{CropName}Seeds` convention; if a future game update
  renames them, that specific recipe is silently skipped (logged as a
  warning) rather than crashing.

## License

MIT - see [LICENSE](LICENSE).
