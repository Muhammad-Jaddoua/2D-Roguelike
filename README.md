# 2D Roguelike (SWE 402 – Assignment 2, Option 1)

A turn-based 2D roguelike built in Unity from the *Create a 2D Roguelike Game* course, plus the Section 7 enhancements and two bonus features.

**Chosen option:** Option 1 – 2D Roguelike Game

## How to open and run
1. Install **Unity 6000.3.22f1** (Unity 6.3 LTS) with the Universal 2D template.
2. In Unity Hub choose **Add > Add project from disk** and select this folder.
3. Open `Assets/Scenes/Main.unity` and press **Play**.

**Controls:** Arrow keys move / attack walls and enemies. **Enter** restarts after Game Over.

**Builds** (in the `Builds/` folder):
- Windows: `Builds/Windows/Roguelike.exe`
- macOS: `Builds/macOS/Roguelike.app`
- WebGL: `Builds/WebGL/index.html` (must be served by a web server, it will not run by double-clicking; e.g. `python -m http.server` inside that folder)
- Published WebGL link: _(add itch.io / Unity Play link here, if uploaded)_

## Section 7 enhancements
| Requirement | Where |
|---|---|
| Background music (looping, reduced volume) | `AudioSource` on the Main Camera, controlled by `SoundManager` |
| Sound effects with `PlayOneShot()` (move, wall attack, food, enemy attack, enemy death, game over) | `SoundManager.cs`, called from `PlayerController`, `WallObject`, `FoodObject`, `EnemyCellObject`, `GameManager` |
| Particle effects (wall destruction, food collection, enemy death), controlled with `.Play()` / `.Stop()` and pooled | `VfxManager.cs` |
| Smooth cell movement with a coroutine + Lerp, input blocked while moving | `PlayerController.MoveRoutine` |
| Object pooling for food, walls, enemies (pre-instantiated at start, recycled with `SetActive`) | `CellObjectPool.cs`, `BoardManager.cs` |
| Code standards: `[SerializeField]` private fields, `[Range]`, `[Tooltip]` | all scripts |

## Bonus features
1. **Dynamic Board Scaling** – the board grows every 2 levels (up to 14x14) and the number of walls, food and enemies increases with the level; the camera re-frames the board each level (`BoardManager.Init`, `FrameCamera`).
2. **Save & Load System** – the best level and games played are saved as JSON (`JsonUtility`) in `PlayerPrefs` and shown on the Game Over screen (`SaveSystem.cs`).
