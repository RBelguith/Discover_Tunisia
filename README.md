<!-- vale SectionTN.Headings = NO -->
# Discover Tunisia
<!-- vale SectionTN.Headings = YES -->

A Unity app that presents three Tunisian governorates: Nabeul, Bizerte and Kairouan. It runs in the Unity Editor and uses Vuforia Engine for augmented reality.

## Features

- Home, region list, card scan, globe and AR screens
- A printed region card in front of the webcam selects its governorate
- Monuments turn on a stage, and stand on a printed target once the visitor turns the camera on
- Texts, photos and 3D models live in assets you edit in the Inspector
- Interface in French, built with UI Toolkit

## Setup

Requirements:

- Unity 6000.6.4f1
- Vuforia Engine 11.4.4 and a Vuforia license key (a free Basic key works)
- A webcam for the scan and AR screens

Then:

1. Put `com.ptc.vuforia.engine-11.4.4.tgz` in `Packages/`. The repository doesn't include it. Get it with Vuforia Engine 11.4.4 for Unity on [developer.vuforia.com](https://developer.vuforia.com/downloads/sdk), or copy it from the `Packages/` folder of a project that already uses that version.
2. Create `Assets/DiscoverTunisia/Settings/Resources/VuforiaLicenseKey.txt` with your license key on one line. Git ignores this file.
3. Open the project in Unity 6000.6.4f1.
4. Accept the Vuforia developer agreement in **Help > Vuforia Engine > Show Developer Agreement**. Vuforia doesn't start in the Editor until you do.

Leave the license key field of `Assets/Resources/VuforiaConfiguration.asset` empty. The app reads the key from the text file, and a test fails if the configuration contains a key.

## Usage

Open `Assets/DiscoverTunisia/Scenes/01_Accueil.unity` and press Play.

| Scene | Screen |
| --- | --- |
| `01_Accueil` | Shows the globe and opens the region list. |
| `02_Identification` | Lists the governorates and opens the globe or the scan. |
| `03_Scan` | Recognizes a region card with the webcam and opens its governorate on the globe. |
| `04_Globe` | Shows the governorate on the globe with its monuments, food and clothes. |
| `05_AR` | Turns the governorate's monuments on a stage, and places them on the printed target once the camera is on. |

Each scene also plays on its own, with Nabeul selected.

## Printed targets

Print these files from `Assets/DiscoverTunisia/AR/` at the sizes below:

| File | Width | Use |
| --- | --- | --- |
| `Targets/CIBLE_AR.png` | 20 cm | Anchors the monuments on the AR screen. |
| `ScanCards/nabeul.jpg`, `bizerte.jpg`, `kairouan.jpg` | 85.6 mm | Selects a governorate on the scan screen. |

To print at another size, change `targetWidth` on the `ARScreen` in `05_AR`, or `cardWidth` on the `ScanScreen` in `03_Scan`. Both are in meters.

## Content

Each governorate is a `GovernorateData` asset in `Assets/DiscoverTunisia/Content/Governorates/`, listed in `Content/ContentCatalog.asset`. It holds the texts, the map position, the accent color, the monuments, food and clothes, and the scan card.

To add a governorate:

1. Create the asset with **Assets > Create > Discover Tunisia > Governorate**.
2. Fill in its fields and add it to the catalog's list.
3. Run the tests. They catch a missing photo, model or text, and a position outside Tunisia.

## Models

The monuments, the avatar, the globe markers and the display stands are low-poly models that `Tools/Blender/models.py` builds in Blender. Their files are in `Assets/DiscoverTunisia/Art/Models/`, and the prefabs in `Art/Prefabs/` use them.

To change a model, edit its function in the script, then export all models again with Blender 5.3:

```sh
blender --background --python Tools/Blender/models.py -- --out Assets/DiscoverTunisia/Art/Models
```

Unity imports the new files and the prefabs follow. Each material in the script must have the name of a material in `Art/Materials/`, or the tests fail.

To use a model from elsewhere, export it as an FBX into `Art/Models/` with materials named after the ones in `Art/Materials/`, then point the monument's prefab at it. The globe and AR screens scale it to fit.

The avatar's outfit takes the color of the chosen governorate. Any part of a model that uses `M_Outfit` or `M_OutfitDark` changes color with it.

## Tests

Open **Window > General > Test Runner**. The EditMode tests check:

- The content: unique ids, texts, positions inside Tunisia, a photo and a model for every entry, readable scan cards
- That every monument has its own model, and that the monuments, the avatar, the markers and the stand use the models exported by the Blender script
- That the avatar's outfit takes the governorate's color
- The globe math that places a latitude and longitude on the sphere
- The Vuforia configuration: no license key, delayed initialization on

The Play Mode tests press the buttons the way a visitor does, with the camera turned off. They check:

- The way from Home through the region list and the globe to the AR screen
- That every monument stands centered on the AR preview and uses the project's materials
- That the globe shows a pin on the chosen governorate only
- The screens shown when the camera is off, on Scan and on AR

## Project layout

Folders in `Assets/DiscoverTunisia/`:

| Folder | Contents |
| --- | --- |
| `AR` | Contains the printed target and the scan cards. |
| `Art` | Contains the Blender models, the prefabs, the globe meshes, materials, shaders and textures. |
| `Content` | Contains the catalog, the governorate assets and their photos. |
| `Scenes` | Contains one scene per screen. |
| `Scripts/Runtime` | Contains the screens, the content types and the globe. |
| `Scripts/AR` | Contains the Vuforia tracker, which compiles only when Vuforia is installed. |
| `Scripts/Editor` | Contains the import rules for the Blender models. |
| `Tests/EditMode` | Contains the EditMode tests. |
| `Tests/PlayMode` | Contains the Play Mode tests. |
| `UI` | Contains the UXML screens, styles, fonts and icons. |

## Credits

Photos, texts and fonts are listed in [CREDITS.md](CREDITS.md), in French.

## License

MIT
