<!-- vale SectionTN.Headings = NO -->
# Discover Tunisia
<!-- vale SectionTN.Headings = YES -->

A Unity app that presents three Tunisian governorates: Nabeul, Bizerte and Kairouan. It runs in the Unity Editor and uses Vuforia Engine for augmented reality.

## Features

- Home, region list, card scan, globe and AR screens
- A printed region card in front of the webcam selects its governorate
- Monuments stand on a printed target, or turn on a stage when the camera is off
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
| `05_AR` | Places the governorate's monuments on the printed target. |

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

The monuments, the avatar, the globe and its markers are placeholder prefabs in `Assets/DiscoverTunisia/Art/Prefabs/`, built from simple shapes.

To use a real model, drag its prefab into the monument's `Model` field in the governorate asset. The globe and AR screens scale it to fit.

**Discover Tunisia > Rebuild Placeholder Models** rebuilds the placeholders and overwrites the prefabs in `Art/Prefabs/`.

## Tests

Open **Window > General > Test Runner** and run the EditMode tests. They check:

- The content: unique ids, texts, positions inside Tunisia, a photo and a model for every entry, readable scan cards
- The globe math that places a latitude and longitude on the sphere
- The Vuforia configuration: no license key, delayed initialization on

## Project layout

Folders in `Assets/DiscoverTunisia/`:

| Folder | Contents |
| --- | --- |
| `AR` | Contains the printed target and the scan cards. |
| `Art` | Contains the prefabs, meshes, materials and textures. |
| `Content` | Contains the catalog, the governorate assets and their photos. |
| `Scenes` | Contains one scene per screen. |
| `Scripts/Runtime` | Contains the screens, the content types and the globe. |
| `Scripts/AR` | Contains the Vuforia tracker, which compiles only when Vuforia is installed. |
| `Scripts/Editor` | Contains the placeholder model builder. |
| `Tests/EditMode` | Contains the EditMode tests. |
| `UI` | Contains the UXML screens, styles, fonts and icons. |

## Credits

Photos, texts and fonts are listed in [CREDITS.md](CREDITS.md), in French.

## License

MIT
