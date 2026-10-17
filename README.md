# The Lighthouse That Remembers Sounds

Unity WebGL storybook built for **PROG2006 Designing the User Experience, Assessment 2** (Southern Cross University, 2026 trimester 4).

The reader follows Mila through four coastal locations to bring back four missing sounds. Each story page has one primary clue that changes the story text, plays a matching sound and lights one memory marker; other objects answer with their own short line and sound without changing progress.

## What is in this repository

| Path | Contents |
|---|---|
| `Assets/Scripts` | Runtime behaviour: page controller and sound-memory progress, story-object triggers, background music, sprite motion, Mila's expressions, scene loading. |
| `Assets/Editor` | Build tooling that configures the six scenes (copy, banners, button theme, object triggers, audio import settings, memory marks) and exports the WebGL player. |
| `Assets/Scenes` | The six scenes: `HomePage`, `RockyShore`, `Harbour`, `Dunes`, `MilasHome`, `Credits`. |
| `Assets/Art` | Project artwork, including the A1 concept sheets, character expressions, props and the A2 layered backgrounds and modular props. |
| `Assets/Audio/A2` | Story sounds: the four clue effects, the theme and ten supporting effects, generated procedurally for this project. `Generated_SFX_Notes.txt` records how they were produced. |
| `Assets/Materials`, `Assets/Prefabs`, `Assets/Sprites` | Interface and scene materials, prefabs and sprite settings. |
| `ProjectSettings`, `Packages` | Unity project settings and package manifest (Unity 2022.3.62f2, WebGL build target). |

## Running the project

1. Open the folder in **Unity 2022.3.62f2** (Unity Hub, Add project from disk).
2. Open `Assets/Scenes/HomePage.unity` and press Play, or use
   `Tools > PROG2006 A2 > Apply polished interactive storybook and build` to re-apply the scene configuration and export the WebGL player to `Builds/A2_FinalPreview`.
3. The published build runs in a browser: `https://ruoxi-wang.itch.io/prog2006-lighthouse-a2`.

## What is deliberately not in this repository

The Unity project uses licensed **Unity Asset Store** content that may not be redistributed, so it is excluded here:

* `Nature Sounds` ambient loops (Nox_Sound, *Nature - Essentials*, Unity Asset Store).
* `FeonY` animated pixel-art backgrounds.
* `Planet Icons`.
* The Unity *TextMesh Pro* package (Unity restores it automatically from `Packages/manifest.json`).

A clone therefore opens and compiles, but scene objects that reference the excluded ambient audio or background packs appear as missing references until those packages are imported again by a licensed user. All scripts, scene layout, interface art, generated story sounds and documentation in this repository are the author's own work.

## Sound and artwork provenance

* Story clue effects, the theme and the ten supporting effects were generated procedurally with Python code (`Assets/Audio/A2/Generated_SFX_Notes.txt`).
* Coastal and interior artwork was created for this project with generative AI image tools, as disclosed in the assessment report's GenAI acknowledgement (Appendix B).
* Interface assets (`ButtonPill.png`, `IconBack/Home/Next/Play/Search.png`) were produced for this project's A1 interface on 1 October 2026.

## Assessment context

* Assessment 1 (design plan and low-fidelity prototype) and Assessment 2 (final functional build, documentation and recorded presentation) share this project.
* The submitted report documents the updated interface, storyboards, the object UI/UX map, the media list and the module journals.
* Author: Ruoxi Wang, Southern Cross University.
