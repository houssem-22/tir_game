# CIPHER

Competitive objective-based FPS prototype (**MVP 1**).  
Find 3 coherent clues → reach the bunker → enter the code. Kills are not the win condition.

## Requirements

- **Unity 6000.0 LTS** (Unity 6) via Unity Hub  
- Built-in Render Pipeline for this slice (URP can be added later; primitives stay readable)  
- Platform: Windows / macOS / Linux Editor

> **Compile gate:** this cloud VM has **no Unity Editor**. C# was authored for Unity 6 APIs but **not batchmode-compiled here**. Verify compile locally on first open (see below).

## Open & Play

1. Install **Unity 6 (6000.0.x)** with Unity Hub.
2. Hub → **Open** → select this repository root (folder containing `Assets/` + `ProjectSettings/`).
3. Let Unity import packages / scripts (first open may upgrade mild ProjectSettings diffs — accept if prompted).
4. Open scene `Assets/Scenes/MVP1_Blacksite.unity`.
5. Press **Play**.

If the scene is missing or empty after import, use menu **CIPHER → Create MVP1 Scene**, then Play.

### Runtime bootstrap

`Mvp1Bootstrap` builds the graybox map, player, bots, clue interactables, HUD, and match on Awake. You do not need to place prefabs manually for MVP1.

## Controls

| Input | Action |
|-------|--------|
| WASD | Move |
| Mouse | Look |
| Left Shift | Sprint |
| Space | Jump |
| LMB | Fire |
| R | Reload |
| 1–5 / Scroll | Swap weapons (Pistol, SMG, Assault Rifle, Shotgun, Sniper) |
| E / F | Interact (clues, bunker code pad) |
| Esc | Unlock / lock cursor |

## MVP1 loop (how to win)

1. Read **OBJECTIF** + **INTEL** on the HUD (seed default `847291`).
2. Follow clue 1 → interact with the highlighted object (e.g. Hospital **Camera 12**).
3. Clue 2 → **Building 04** panel.
4. Clue 3 → **Terminal C**.
5. Go to **Bunker 07**, press **E** on the code pad, enter the 4-digit code.
6. **ACCESS GRANTED** = victory.

The match code is printed once in the **Console** at start for QA (`[CIPHER MVP1] ... CODE=xxxx`). It is not shown on the HUD.

## Project layout

```
Assets/
  Art/           # Quaternius / Kenney drop folders (empty placeholders)
  Gameplay/      # Player, weapons, combat, interactables
  Clues/         # Database, generator, validator, MatchManager
  AI/            # Bot patrol / combat / seek-clue
  UI/            # IMGUI HUD
  Bootstrap/     # Mvp1Bootstrap graybox builder
  Scenes/        # MVP1_Blacksite
  Editor/        # CIPHER → Create MVP1 Scene
```

## Local compile check (when Editor is available)

```bash
UNITY="/path/to/Unity/Hub/Editor/6000.0.x/Editor/Unity"
"$UNITY" -batchmode -nographics -quit \
  -projectPath "/path/to/this/repo" \
  -logFile /tmp/cipher-compile.log
# Fail if log contains: error CS | Scripts have compiler errors
```

## Out of scope (by design)

- Multiplayer / Netcode  
- Full BLACKSITE art pass  
- Teams, loot, steal-on-death INTEL  
- URP asset pipeline
