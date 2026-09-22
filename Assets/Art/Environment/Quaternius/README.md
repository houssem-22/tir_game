# Quaternius FBX drop-in (required for final art direction)

CDC target: **Modern Military / Realistic FPS**.  
Graybox cubes are **rejected** for shipping art. This phase uses Poly Haven CC0 materials + **enterable modular rooms**.

## What to download (CC0)

From [quaternius.com](https://quaternius.com) / itch:

1. **Ultimate Modular Buildings** or military modular pack (FBX)
2. **Ultimate Guns Pack** (40 weapons FBX)
3. **Universal Base Characters** + Animation Library

## Where to place files

```
Assets/Art/Environment/Quaternius/   ← modular walls, floors, props (.fbx)
Assets/Art/Weapons/Quaternius/       ← gun FBX
Assets/Art/Characters/Quaternius/    ← character FBX + anims
```

Then in Unity: select FBX → Rig Humanoid (characters) → Materials Import.

`FbxPropSpawner` (Editor menu **CIPHER → Refresh Quaternius Prefabs**) will wire prefabs when you add that tool in a later pass.

## Why Kenney Starter Kit was not used as primary art

Kenney FPS/City GLBs are stylized/cartoon — CDC §27 rejects that for final. Kenney remains OK for UI/prototyping only.
