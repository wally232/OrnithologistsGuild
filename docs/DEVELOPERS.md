# Creating a content pack

💡 **This documentation is a work in progress!**

## Complete example

See [`OrnithologistsGuild/assets/content-pack`](https://github.com/greyivy/OrnithologistsGuild/tree/main/OrnithologistsGuild/assets/content-pack) for a complete example.

## `manifest.json`

[Create your manifest](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest) as usual. Use prefix `[OG]` in your mod's name.

Add the following to your `manifest.json`:

```json
"ContentPackFor": {
    "UniqueID": "Ivy.OrnithologistsGuild"
}
```

## `content.json`

All content packs require a `content.json` file in their root.

| Key               | Description                                |
|-------------------|--------------------------------------------|
| `FormatVersion`   | Always `1`                                 |
| `Birdies`         | Array of [`Birdie object`](#birdie-object) |

## Birdie object

| Key                | Description                                                                                                                                                                                  | Required | Default                             |
|--------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------|-------------------------------------|
| `ID`               | Bird ID, unique to your mod                                                                                                                                                                  | `true`   |                                     |
| `AssetPath`        | Path to sprite                                                                                                                                                                               | `false`  | Stardew Valley `critters` tilesheet |
| `SoundAssetPath`   | Path to custom sound                                                                                                                                                                         | `false`  |                                     |
| `BaseFrame`        | Sprite base frame                                                                                                                                                                            | `false`  | `0`                                 |
| `Attributes`       | Number of discoverable attributes (see [Translations](#translations))                                                                                                                        | `true`   |                                     |
| `ShouldUseBath`    | *Not yet used*                                                                                                                                                                               | `true`   |                                     |
| `MaxFlockSize`     | Maximum number of birds to spawn in a single flock                                                                                                                                           | `true`   |                                     |
| `Cautiousness`     | How close a player must in, in tiles, for a bird to frighten                                                                                                                                 | `true`   |                                     |
| `FlapDuration`     | Duration, in ms, between flaps                                                                                                                                                               | `true`   |                                     |
| `FlySpeed`         | Speed of bird flight (4 is relatively slow, 6 is relatively fast)                                                                                                                            | `true`   |                                     |
| `BaseWt`           | Number determining how likely bird is to spawn globally (1 being extremely likely, 0 being not spawned)                                                                                      | `true`   |                                     |
| `FeederBaseWts`    | Object with feeder type keys and weight values determining how likely bird is to spawn nearby a feeder of this type (independant from `BaseWt` but added to `FoodBaseWts`)                   | `true`   |                                     |
| `FoodBaseWts`      | Object with feeder type keys and weight values determining how likely bird is to spawn nearby a feeder containing food of this type (independant from `BaseWt` but added to `FeederBaseWts`) | `true`   |                                     |
| `Conditions`       | Array of conditions affecting all of the above weights                                                                                                                                       | `true`   |                                     |
| `Conditions.When`  | [Content Patcher conditions](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/tokens.md#global-tokens)                                               | `true`   |                                     |
| `Conditions.AddWt` | Weight to add to `BaseWt`, `FeederBaseWts`, `FoodBaseWts` when condition is met                                                                                                              | `false`  |                                     |
| `Conditions.SubWt` | Weight to subtract to `BaseWt`, `FeederBaseWts`, `FoodBaseWts` when condition is met                                                                                                         | `false`  |                                     |
| `Conditions.NilWt` | Bird not spawned when condition is met                                                                                                                                                       | `false`  |                                     |

## Weight system examples

### Winter forest birds

- Spawn globally, but are somewhat rare
- Common at Hopper feeders with Fruit, somewhat rare at Platform feeders with Fruit
- More common in Winter, in the Forest location
- Less common in Summer

```json
{
    "BaseWt": 0.05,
    "FeederBaseWts": {
        "Hopper": 0.2,
        "Platform": 0.025
    },
    "FoodBaseWts": {
        "Fruit": 0.025,
    },
    "Conditions": [
        {
            "When": {
                "Season": "Winter"
            },
            "AddWt": 0.1
        },
        {
            "When": {
                "LocationName": "Forest"
            },
            "AddWt": 0.1
        },
        {
            "When": {
                "Season": "Summer"
            },
            "SubWt": 0.025
        }
    ]
}
```

### Vanilla Stardew Valley birds

- Spawn globally and are very common
- Visit at all feeder types with all food types
- Do not spawn after 3 PM
- Do not spawn in Desert, Railroad, or Farm locations
- Do not spawn in Summer

```json
{
    "BaseWt": 0.5,
    "FeederBaseWts": {
        "Hopper": 0.25,
        "Tube": 0.25,
        "Platform": 0.25
    },
    "FoodBaseWts": {
        "Corn": 0.25,
        "Fruit": 0.25,
        "MixedSeeds": 0.25,
        "SunflowerSeeds": 0.25
    },
    "Conditions": [
        {
            "When": {
                "Time": "{{Range: 1500, 2600}}"
            },
            "NilWt": true
        },
        {
            "When": {
                "LocationName": "Desert, Railroad, Farm"
            },
            "NilWt": true
        },
        {
            "When": {
                "Season": "Summer"
            },
            "NilWt": true
        }
    ]
}
```

## Translations

### `i18n/default.json`

Replace `{ID}` with the `ID` of your birdie.

| Key                          | Description                                                             |   |   |
|------------------------------|-------------------------------------------------------------------------|---|---|
| `birdie.{ID}.commonName`     | Your bird's common name                                                 |   |   |
| `birdie.{ID}.scientificName` | Your bird's scientific name                                             |   |   |
| `birdie.{ID}.attribute.{N}`  | A short attribute like "plump body" where `{N}` is the attribute number |   |   |
| `birdie.{ID}.funFact`        | A fun fact about your bird!                                             |   |   |