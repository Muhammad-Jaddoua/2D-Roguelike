# 2D Roguelike – SWE 402, Assignment 2

**Chosen option: Option 1 – 2D Roguelike Game**

A turn-based roguelike with procedural levels, enemies and food as a resource, built in Unity from the *Create a 2D Roguelike Game* course plus the Section 7 enhancements.

## Play it online
**Unity Play (WebGL):** https://play.unity.com/en/games/fdcce95e-fbdd-44e7-94e6-e2fb74bfa8d3/web

## How to run the game

**Option A – built versions (in the `Builds/` folder)**
| Platform | How to run |
|---|---|
| Windows | Run `Builds/Windows/2D Roguelike.exe` (keep the other files in that folder next to it) |
| macOS | Open `Builds/Mac.app`. If macOS refuses to open it, right-click the app and choose **Open**; if it says the app can't be opened, run `chmod +x "Builds/Mac.app/Contents/MacOS/2D Roguelike"` in a terminal first |
| WebGL | `Builds/Web/index.html` has to be served by a web server (it won't work by double-clicking), e.g. run `python -m http.server` inside `Builds/Web` and open `http://localhost:8000`. A zipped copy is in `Builds/WebBuild.zip`. Or just use the Unity Play link above |

**Option B – open the Unity project**
1. Install **Unity 6000.3.22f1** (Unity 6.3 LTS).
2. In Unity Hub choose **Add > Add project from disk** and select this folder.
3. Open `Assets/Scenes/Main.unity` and press **Play**.

**Controls:** Arrow keys to move, attack walls and attack enemies. Press **Enter** to restart after Game Over.

## Bonus features implemented
1. **Dynamic Board Scaling** – the board grows every 2 levels (up to 14x14) and the number of walls, food items and enemies increases as the level goes up.
2. **Save & Load System** – the best level reached and games played are saved as JSON (`JsonUtility` + `PlayerPrefs`) and shown on the Game Over screen.

## Section 7 enhancements
Background music and sound effects (`SoundManager.cs`), pooled particle effects (`VfxManager.cs`), coroutine-based smooth player movement (`PlayerController.cs`), object pooling for food, walls and enemies (`CellObjectPool.cs`, `BoardManager.cs`), and `[SerializeField]` / `[Range]` / `[Tooltip]` on all Inspector fields.
