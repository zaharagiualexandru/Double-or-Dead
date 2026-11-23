# Double or Dead

**20 seconds. Grab the loot. Get out.**

A first-person dungeon score attack built in Unity. Plan your route, collect treasure, and escape before time runs out. Take the risk of entering the crown room to double your haul—or lose everything collected during the run.

🎮 **[Play Double or Dead on itch.io](https://alexziou.itch.io/double-or-dead)**

Created for the **20 Second Game Jam 2026**.

## 🏰 Overview

Double or Dead takes place in a fixed dungeon where every second matters. Success depends on learning the layout, managing limited sprint stamina, and choosing when to stop collecting treasure and head for the exit.

Each attempt is a self-contained run lasting up to 20 seconds. Compete on a global leaderboard ranked by secured loot, with faster escapes breaking ties.

## 📖 How to Play

1. **Plan your route.** Look around before starting. The timer begins when you move or jump.
2. **Collect treasure.** Touch coins and bars to add their value to your carried loot.
3. **Manage your stamina.** Sprint to cover ground quickly, but your stamina does not regenerate during the run.
4. **Choose your risk.** Find a key, unlock the crown room, and collect the crown for a chance to double your loot.
5. **Escape in time.** Reach the exit before the timer reaches zero to secure your score.

Touching spikes or running out of time ends the run and forfeits its loot. Your previously saved leaderboard best remains unchanged.

## 🎮 Controls

| Input | Action |
|---|---|
| WASD | Move |
| Mouse | Look |
| Left Shift | Sprint |
| Space | Jump |
| R | Restart the run |
| M | Return to the main menu after escape or death |
| U | Retry a failed score upload from the escape screen |
| Escape | Release the mouse cursor |

Click inside the game to capture the mouse. In the browser version, Escape may also leave fullscreen.

## ⚔️ Game Mechanics

### ⏱️ Timer and Route Planning

The countdown starts on your first movement or jump, allowing you to look around before committing to a route.

The fixed dungeon layout makes repeated attempts an opportunity to improve: learn treasure locations, identify shortcuts, and balance collection time against the distance to the exit.

### 💰 Treasure Values

Treasure values are added together as you collect them.

| Treasure | Value |
|---|---:|
| Silver coin | 1,000 |
| Gold coin | 2,500 |
| Silver bar | 10,000 |
| Gold bar | 25,000 |

Carried treasure only becomes a secured score when you escape successfully.

### 👑 Key and Crown

A key unlocks the gate to the crown room. Collecting the crown applies a **×2 multiplier to the total loot secured on escape**.

For example:

- Escape with 50,000 loot without the crown: **50,000**
- Escape with 50,000 loot and the crown: **100,000**
- Fail to escape: **0**, whether or not you collected the crown

The crown offers a larger score, but reaching it and returning to the exit costs valuable time.

### ⚡ Sprint Stamina

Sprinting consumes a limited stamina reserve. Stamina does not regenerate during a run, so using it strategically is part of planning a successful route.

Restarting begins a fresh run with restored stamina.

### 🏆 Global Leaderboard

Successful escapes are submitted to the online leaderboard.

- **Higher secured loot ranks first.**
- **Equal loot is ranked by faster escape time.**
- **Only your best result is retained.**
- Failed runs do not replace your saved best.

Register a unique player name before playing. Names support **1–8 letters or numbers**.

An internet connection is required for online name registration and leaderboard features.

## ⚙️ Settings

The Options menu includes:

- Mouse sensitivity
- Master volume
- Music volume
- Sound effects volume
- Fullscreen
- Reset to defaults

Volume and sensitivity preferences are saved locally.

## 🌐 Playing in a Browser

The browser version is designed for desktop play with a keyboard and mouse.

For the best experience:

- Enable hardware acceleration in your browser.
- Click inside the game to capture the mouse.
- Use fullscreen if preferred.
- Keep an internet connection available for online features.

The game currently uses anonymous Unity Authentication. Browser and desktop installations can have separate player identities. Clearing browser site data or local game data may remove access to an anonymous account; the game does not automatically use your itch.io account as its player identity.

## 🛠️ Technical Overview

Built with **Unity 6.3** and **C#**.

| Technology | Purpose |
|---|---|
| Unity Input System | Keyboard and mouse input |
| Universal Render Pipeline | Rendering |
| TextMeshPro | Interface text |
| Unity Audio Mixer | Separate master, music and sound-effects volume controls |
| Unity Authentication | Player identity |
| Unity Cloud Code | Name registration and score-submission endpoints |
| Unity Cloud Save | Shared player-name registry |
| Unity Leaderboards | Online rankings and personal-best results |

Score submissions pass through Cloud Code, and access-control rules block direct player writes to the leaderboard.

The client supplies run results. This is not a fully server-authoritative gameplay or anti-cheat system.

## 🚀 Running the Project

1. Clone this repository.
2. Add the project through Unity Hub.
3. Use the Unity Editor version specified in `ProjectSettings/ProjectVersion.txt`.
4. Allow Unity to restore packages and import assets.
5. Open `Assets/Scenes/MainMenu.unity`.
6. Enter Play Mode.

Start from the MainMenu scene to initialize the game’s services and persistent managers.

Online functionality requires a configured Unity Gaming Services project, the relevant Cloud Code scripts, a Cloud Save name registry, and the `top_thieves` leaderboard. A local checkout alone does not provision these services.
