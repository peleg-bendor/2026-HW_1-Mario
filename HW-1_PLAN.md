# HW-1 Plan — `2026-HW_1-Mario` (Exercise 1)

Shared working notes for building Exercise 1. The assistant may edit this file directly as we go (it's not "game code," just shared notes).

## Status Legend

- `[ ]` not started
- `[~]` in progress
- `[x]` done AND confirmed working in-editor (ready to be shown in the submission video)

## Git Workflow Reminder

- After any step that leaves the project in a working state, consider committing (small, working commits > one giant commit).
- After a whole stage is finished and confirmed working, push.

## Stage Order

Mirrors `Course/Exercises/Exercise 01.md`'s own numbering, with a few added side-steps that aren't part of the exercise's own numbering: Stage 0 for cleanup/setup before any new feature work, Stage 2.5 once Stage 2 surfaced things worth fixing before building further on top of them, and Stage 10 (Game over / Game won GUI - Peleg's own addition, not asked for in the exercise text) inserted after Stage 9 and before the exercise's own bonus item. That pushes the bonus spawner to Stage 11, and adds two closing stages that aren't feature work at all: Stage 12 for a final jump-feel tuning pass plus one last full test, and Stage 13 for writing the video script.

### Stage 0 — Clean up & steady base `[x]`

Before writing new features, went through the inherited Lesson 4 code together and decided what (if anything) to fix.

#### Step 1 — `WeaponsHandler.index` hardcoded `[x]`

`WeaponsHandler.index` is hardcoded (currently `1` in the scene), so only one weapon is reachable via Left-Ctrl at a time. Confirmed by reading `Scene_Physics.unity` directly: `index: 1`, and `TempInit.Start()` registers the fireball first (list position `0`) then the axe (list position `1`) — so right now Left-Ctrl always fires the **axe**; the fireball is unreachable via input, even fully equipped.

**Decision:** defer to the Pickable Axe stage — it naturally belongs there too.

#### Step 2 — Dead code in `SC_Death.cs` `[x]`

`SC_Death.cs` has a dead commented-out reference to a removed `SC_Player` class.

**Decision:** remove it.

#### Step 3 — Stray `Assets/InvoicePersistence` file `[x]`

Leftover from a different lecture example, not part of the game.

**Confirmed:** was never copied into this project's `Assets` folder in the first place. Nothing to delete.

#### Step 4 — `SC_` naming inconsistency `[x]`

Some scripts use the legacy `SC_` prefix, newer ones don't. Decide on one convention going forward and note the decision in the log below.

**Decision:** keep `SC_` as-is. `SC_Coin.cs`, `SC_CoinsManager.cs`, `SC_Death.cs`, `SC_Floor.cs` were byte-identical across all four course zips going back to the pre-SOLID "Mario Start" project at the time we made this decision — the prefix boundary tracks which scripts predate the SOLID refactors, not carelessness. Mention this as an accepted, known inconsistency in the video narration. Note: the Stage 0 cleanup pass below has since made real internal edits to `SC_Coin`, `SC_Death`, and `SC_Floor` (bug fixes, log cleanup, magic-number removal) — the "untouched" fact was only ever the *reason the naming split exists*, not a promise to freeze their internals, so this doesn't change the naming decision itself.

#### Step 5 — Full script scan `[x]`

Beyond the known list above, scanned the rest of the inherited scripts for the same categories of issue: organization, useful/informative comments, logging, null checks, magic numbers. Noted anything found here before deciding what to change.

**Done.** 13 findings reviewed and applied, plus a follow-up logging audit (7 more) and the `SC_Floor` landing-check rework — all in the Decisions Log below. `Coins Text` wired in the Inspector and confirmed working in Play mode; `SC_Floor`'s reworked landing check re-tested afterward and confirmed working. `BaseWeapon`/`TestBaseWeapon` kept (not deleted) with added explanatory comments.

#### Step 6 — Confirm the base is steady `[x]`

Project opens cleanly, `Scene_Physics.unity` runs with no Console errors/warnings, controls all work as expected.

**Confirmed** by Peleg.

#### Step 7 — Fix Build Settings `[x]`

`SampleScene.unity` deleted, Build Settings fixed to reference `Scene_Physics.unity`.

**Confirmed done.** `Assets/Scenes/` now only contains `Scene_Physics.unity`; `EditorBuildSettings.asset` lists only `Scene_Physics.unity`.

### Stage 1 — Coin-count GUI `[x]`

Already exists via `SC_CoinsManager` + `Txt_Coins`; confirmed still working after Stage 0 changes (the `Coins Text` serialized-field rewiring included).

### Stage 2 — Pickable Axe `[x]`

Collectible axes, GUI shows count, GUI updates when Mario throws one. Also fixes the Stage 0-deferred `WeaponsHandler.index` bug (axe was reachable only by list-position coincidence; fireball was completely unreachable via input).

#### Step 1 — Weapon selection `[x]`

`IWeapon` gains `bool IsAvailable()` (mandatory for anything the handler manages, unlike the genuinely-optional `Reload()`/`Equip()` split — cycling can't work unless every registered weapon can answer it). `AxeWeapon.IsAvailable()` always returns `true`; `FireballWeapon.IsAvailable()` returns `_isEquip`. `WeaponsHandler` keeps its generic `List<IWeapon>` + a private `selectedIndex` (starts at `0`/Axe — `TempInit` registers the axe before the fireball now), Left-Ctrl attacks the selected weapon, `Q` cycles to the next *available* weapon in the list (no-op, logged, if the only other one isn't available yet — e.g. Fireball before the Fire Flower is collected). Fires a static `OnWeaponSelected(string)` event on every change. `TempInit`'s old Q-key axe-reload polling is deleted (reload no longer happens via key press — see next step), and `Q` is now owned entirely by `WeaponsHandler`.

#### Step 2 — Selected-weapon GUI `[x]`

New manager mirroring `SC_CoinsManager` + new `Txt_SelectedWeapon` TMP text under `Canvas` ("Selected weapon: Axe" / "...Fireball"), subscribed to `WeaponsHandler.OnWeaponSelected`.

#### Step 3 — `AxeWeapon` ammo count `[x]`

Replace the `_loaded` bool with an `axesHeld` int (starts at `1` — Mario starts with one axe already). `Attack()` throws and decrements `axesHeld` when it's `> 0`; `Reload()` increments it (now called only from the pickup path, never from a key). Fires a static `OnAxeCountChanged(int)` event on both changes, including once on `Start()` so a GUI reading the initial count doesn't need its own hardcoded starting value.

#### Step 4 — `AxePowerUp` `[x]`

`AxePowerUp : IPowerUp` — mirrors `FireFlowerPowerUp`'s shape, using `GetComponentInChildren<IReloadWeapon>()` (interface, not the concrete `AxeWeapon`) and calling `Reload()`. Written before the next two steps since both need it.

#### Step 5 — `ProjectileAxe` / `Axe.prefab` landing `[x]`

Prefab currently has no `Collider2D`, and its `Rigidbody2D` constraints (`6` = FreezeRotation + FreezePositionY) freeze vertical movement, so it can't fall or land at all as-is. Fix: constraints → FreezeRotation only (matches Mario's rigidbody), add a solid (non-trigger) collider, and add `OnCollisionEnter2D` in `ProjectileAxe` that branches on the tag of whatever it hit — the floor freezes velocity/rests in place; the Player hands off `new AxePowerUp()` to `PlayerPowerUp.CollectPowerUp()` (picked back up, same as any other axe pickup) and destroys itself. A landed axe is walked into physically rather than passed through like a coin, since it needs a solid collider to land on the floor in the first place.

#### Step 6 — `AxePickupController` `[x]`

Mirrors `FireFlowerController`'s trigger-detect-and-hand-to-`PlayerPowerUp` shape directly (not generalized into a shared base class yet; revisit once Pickable Strike, Stage 4, adds a third instance of the same pattern). New `AxePickup` prefab (sprite + trigger collider + this script) — prefabbed from the start, unlike the Flower.

#### Step 7 — Axe-count GUI `[x]`

New manager mirroring `SC_CoinsManager` + new `Txt_Axes` TMP text under `Canvas`, subscribed to `AxeWeapon.OnAxeCountChanged`.

#### Step 8 — Core mechanic playtest `[x]`

Pick up axe(s), throw, confirm the thrown axe lands and rests instead of vanishing, confirm walking into a landed axe picks it back up, confirm the GUI count tracks pickup/reload (+1) and throw (-1), confirm the fireball is now reachable via its own key too (still gated behind the Fire Flower's `Equip()`). Confirmed clean via `OutputLogsTemp.txt` — full throw/land/pickup/GUI/fireball-switch sequence traced through with no errors.

#### Step 9 — Polish: despawn warning `[x]`

`ProjectileAxe` tracks its own `age` in `Update()` instead of relying on the built-in delayed `Destroy`; once `age >= lifetime - warningDuration` (`[SerializeField] float warningDuration = 3f`) it fades its `SpriteRenderer` alpha to `0` as a despawn warning, then is destroyed once `age >= lifetime` (bumped from `3` to `10`, giving a real window to walk over and reclaim it before it disappears). Originally planned as a color-to-white lerp, changed to an alpha fade — see Decisions Log.

### Stage 2.5 — Side-step: physics feel & map room `[x]`

Not part of `Exercise 01.md`'s numbering (like Stage 0) — a pause between Pickable Axe and Lives/Strikes to fix things Stage 2 surfaced that would only get more annoying to touch once later stages depend on the current map and movement feel.

#### Step 1 — `SC_Floor` repeated-landing log `[x]`

`SC_Floor` fired `"Mario landed on floor"` repeatedly during ordinary walking, not just on actual jumps — caused by the floor being 14 separate tile colliders rather than one continuous one, so crossing from one tile to the next re-triggers `OnCollisionEnter2D`. Confirmed pre-existing (present in the very first Stage 2 test log, before any axe code existed), not something Stage 2 introduced. Functionally harmless (`PlayerJump.OnFloorCollision()` just re-set `isJumping = false`) but noisy, especially for the video. Full diagnosis and fix in the Decisions Log below.

#### Step 2 — Expand the level map `[x]`

More floor tiles/room for the enemies, key/door, and final assembled level still to come. Floor grew from 14 to about 90 `Sprite_Floor` tiles, placed directly in the Editor. Tiles reorganized under a new `Tiles` child of `World` for Hierarchy tidiness. `World` (parent of Mario, every tile, and every pickup/hazard — everything except `Main Camera`) nudged `X +0.5` for visual alignment.

**Confirmed working** by Peleg.

#### Step 3 — Tune movement/physics feel `[x]`

`PlayerMovement` felt too slippery, `PlayerJump`'s jump felt too low, `ProjectileFireball`'s speed felt too slow. Root cause behind the jump/fireball complaints and the fix are in the Decisions Log below.

**Confirmed working** by Peleg: "now it all feels nice."

### Stage 3 — Lives/"strikes" system `[x]`

Start with 3, lose one per death, restart the game at 0.

#### Step 1 — `StrikesManager` core logic `[x]`

New `StrikesManager` (own GameObject under `Scripts` in the Hierarchy, mirrors `AxeCountManager`/`SelectedWeaponManager`'s shape) tracks `strikesRemaining`, subscribes to the existing `SC_Death.OnSpikeCollision` event independently of `PlayerDeath` (no changes needed to `PlayerDeath.cs` or `SC_Death.cs`), and reloads the scene once strikes hit 0. Fires a new static `OnStrikeCountChanged(int)` event for Stage 5's future GUI.

**Confirmed working** by Peleg via `OutputLogsTemp.txt`: two full playthroughs down to 0 strikes, both ending in a clean scene reload with no Console errors. The log also happened to prove the ordering fix from the design discussion was necessary, not just defensive: in the first playthrough's final hit, `StrikesManager`'s handler ran before `PlayerDeath`'s; in the second playthrough's final hit, the order flipped. Since the reload is deferred to `Update()` rather than firing inline from the event handler, both orders worked cleanly either way.

### Stage 4 — Pickable Strike `[x]`

An extra-life pickup.

#### Step 1 — `StrikePowerUp` / `StrikePickupController` / `StrikesManager` cap `[x]`

`StrikePowerUp : IPowerUp` mirrors `AxePowerUp`'s shape, but since strikes live on `StrikesManager` (a scene-level manager, not anything under the player), `ApplyPowerUp()` fires a static `OnStrikeGained` event instead of reaching into `player`'s children. `StrikesManager` subscribes to it the same way it already subscribes to `SC_Death.OnSpikeCollision`, and caps the gain at `startingStrikes` rather than a separately hardcoded max. `StrikePickupController` copies `AxePickupController`'s trigger-detect shape directly.

**Confirmed working** by Peleg via `OutputLogsTemp.txt`: both paths traced through cleanly — a pickup below the cap logs `"Strike gained - N remaining"`, a pickup at the cap logs `"Strike pickup ignored - already at max (3)"` instead, and a full 3-strike death sequence still ends in a clean reload, all with no Console errors. `Sprite_Strike`'s oversized-import issue (455px source image at the default 100 Pixels Per Unit) got sorted out along the way — fixed via the sprite's `Pixels Per Unit` import setting, not the Transform or Sprite Renderer.

### Stage 5 — Strikes-remaining GUI `[x]`

Show the current strike count on screen.

#### Step 1 — `StrikeCountManager` `[x]`

New manager mirroring `AxeCountManager` exactly + new `Txt_Strikes` TMP text under `Canvas` (positioned under `Txt_Axes`), subscribed to `StrikesManager.OnStrikeCountChanged`.

**Confirmed working** by Peleg via `OutputLogsTemp.txt` and in the Game view: `Txt_Strikes` tracks strike loss, strike gain, and the post-reload reset back to 3, all with no Console errors.

### Stage 6 — Simple moving enemy `[x]`

Patrols left-right, kills Mario on touch, destroyed by fireball or axe.

#### Step 1 — `SC_Death` cleanup: hazard-neutral naming `[x]`

`SC_Death`'s event/delegate (`OnSpikeCollision`/`SpikeCollisionHandler`) and its "Mario hit spikes" log both name spikes specifically, but Stage 6 needs to reuse this exact component on a physically different kind of hazard (the enemy). Renaming to something hazard-neutral now, before a second real consumer exists, keeps both consumers accurate instead of stale. Also removes the leftover unconditional `Debug.Log("OnCollisionEnter2D " + ...)` at the top of `OnCollisionEnter2D` (deferred from the Stage 2.5 `SC_Floor` cleanup since spikes don't move) — needed before attaching `SC_Death` to something that will cross many floor-tile boundaries while patrolling. `PlayerDeath` and `StrikesManager` update their subscriptions to match the renamed event; neither's actual logic changes.

#### Step 2 — `EnemyMovement` `[x]`

New script: two short `Physics2D.Raycast` checks each `FixedUpdate` (ground-below, wall-ahead in the current facing direction), both reading the enemy's own collider bounds live rather than a hardcoded offset (same approach as `SC_Floor`'s landing check). Grounded with no wall ahead keeps moving in the current direction; grounded with a wall ahead flips direction; not grounded stops horizontal movement and lets gravity fall, resuming once grounded again. Starting facing is exposed as a `Direction` enum (`Left`/`Right`) for a readable Inspector field, converted once to the `±1` float convention the rest of the movement/projectile code already uses. No prefab yet this step — just the script, confirmed to compile.

**Revised during Step 3's testing** — see the Decisions Log for what changed and why: the raycasts switched from the single-hit `Physics2D.Raycast` overload to a `ContactFilter2D` + hit list, a second `spriteNativeFacing` field got added, and grounded-state handling was reworked to preserve momentum over an edge instead of zeroing it.

#### Step 3 — `Sprite_Ghost` prefab: movement + touch `[x]`

New prefab (`SpriteRenderer` using the sourced ghost sprite, `Rigidbody2D` — dynamic, gravity, `FreezeRotation`, matching Mario/the axe — solid non-trigger `Collider2D`, `EnemyMovement`, and `SC_Death` reused as-is for the touch-Mario flow) placed as a `World > Enemies` child. Covers the first two of the exercise's three bullets: moves right to left, and costs Mario a strike on physical touch, reusing the existing `StrikesManager`/`PlayerDeath` flow completely unchanged. Playtest: patrol against the level's actual platform edges and step faces (confirm it turns around at a wall face and stops/resumes correctly at a platform edge), confirm touching it costs exactly one strike and (at 0) still ends in a clean scene reload.

**Confirmed working** by Peleg via `OutputLogsTemp.txt`, after several real bugs surfaced and got fixed along the way (full list in the Decisions Log): patrols correctly, turns at wall faces, falls through and past ledges instead of getting stuck, and lands cleanly afterward.

#### Step 4 — `IEnemy` / `EnemyHealth` `[x]`

New `IEnemy` interface (`Assets/Scripts/Interfaces/`) with a single `Kill()` method, deliberately narrow since "destroyed by a projectile" is the only trait actually shared by this stage's enemy and Stage 7's. New `EnemyHealth : MonoBehaviour, IEnemy` implements `Kill()` as `Destroy(gameObject)` plus a log, added to the `Sprite_Ghost` prefab alongside `EnemyMovement`/`SC_Death`.

#### Step 5 — `ProjectileFireball` recognizes enemies `[x]`

`Fireball.prefab` currently has no `Collider2D` at all, so it can't detect anything it flies through. Adds a trigger `Collider2D` and a new `OnTriggerEnter2D` that checks the other object for `IEnemy` — if present, calls `Kill()` and destroys the fireball; otherwise ignores the collision entirely, so it keeps flying through terrain exactly like it does today, no behavior change there.

**Confirmed working** by Peleg via `OutputLogsTemp.txt`. Two setup mistakes surfaced during testing before it worked, both worth knowing about: the new collider needs `Is Trigger` actually checked (left solid, the fireball physically bumps into walls and enemies and no trigger event ever fires), and `EnemyHealth` has to be on the prefab, which Step 4 specified but never got applied — with no `IEnemy` component anywhere on the ghost, the fireball's lookup correctly found nothing and passed straight through, silently and with no error.

#### Step 6 — `ProjectileAxe` recognizes enemies `[x]`

`OnCollisionEnter2D` currently treats *any* non-Player collision as "landed on the floor." Adds an `IEnemy` check before that fallback, only while the axe hasn't landed yet (`!hasLanded`) — a thrown axe still in flight that hits an enemy kills it and destroys the axe instead of resting on top of it. A landed, resting axe still ignores non-Player collisions entirely, unchanged from today.

**Confirmed working** by Peleg via `OutputLogsTemp.txt`: two ghosts killed by thrown axes, with the axe's own landing, resting and re-pickup behaviour all still intact afterwards. Also removed `ProjectileAxe`'s stray unconditional `Debug.Log("OnCollisionEnter2D " + ...)`, the fourth of that leftover we've found (after `SC_Floor`, `SC_Death` and `SC_Coin`), deliberately deferred back in Stage 2.5 as out of scope at the time.

#### Step 7 — Core mechanic playtest `[x]`

All three exercise bullets together: patrol, touch-costs-a-strike, and destroyed-by-fireball-or-axe. Confirm a thrown axe and a fireball both destroy the enemy; confirm a landed axe doesn't do anything unexpected if the enemy walks near it; confirm the enemy's position/patrol state doesn't reset on a non-fatal strike loss (only Mario repositions); confirm a full game-over reload resets the enemy along with everything else, for free, via the scene reload.

**Confirmed working** by Peleg via `OutputLogsTemp.txt`, all in one continuous session: the ghost falls once at spawn and logs nothing afterwards but wall turns, touching it costs exactly one strike while the ghost carries on patrolling from where it was, an axe kill and a fireball kill both land, a full three-strike game over reloads the scene with everything reset, and a landed axe visibly acts as an obstacle the ghost turns around at. No Console errors and no stray trigger logs left anywhere.

### Stage 7 — Static enemy `[x]`

Stationary, fires a fireball every X seconds.

#### Step 1 — `SC_Death`: trigger support `[x]`

Add `OnTriggerEnter2D` alongside the existing `OnCollisionEnter2D`, firing the same `OnHazardCollision`/`OnHazardCollisionGeneral` events. Needed because the garlic's collider has to be a trigger (same reason `Fireball.prefab`'s is) to fly through terrain, so a projectile costing Mario a strike can't reuse the collision-only path spikes/the ghost use. Purely additive — solid colliders never call `OnTriggerEnter2D` and trigger colliders never call `OnCollisionEnter2D`, so spikes, the ghost, `PlayerDeath`, and `StrikesManager` are all unaffected.

#### Step 2 — `ProjectileGarlic` `[x]`

New script (`Player/Projectiles/`), mirrors `ProjectileFireball`'s `Awake`/`Attack` shape directly rather than reusing the class. Reusing `ProjectileFireball` as-is would kill any `IEnemy` it overlaps on spawn — including the vampire that just fired it, since the garlic spawns at the vampire's own position. `ProjectileGarlic` doesn't check `IEnemy` at all; its `OnTriggerEnter2D` only destroys itself on hitting the Player, leaving whether that costs a strike entirely to a separately-attached `SC_Death`. No prefab yet this step, just the script confirmed to compile.

#### Step 3 — `EnemyRangedAttack` `[x]`

New script (`Enemy/`), a timer that waits `fireInterval` seconds then instantiates a projectile and calls `Attack()` on it — direction fixed rather than derived from facing, since the vampire never turns. Named for the behavior rather than the vampire, matching `EnemyMovement`'s naming reasoning. No `IWeapon`-style interface: nothing cycles or manages multiple instances of this the way `WeaponsHandler` does for Mario's weapons, so there's no real second implementation to justify one yet. No prefab yet this step either.

**Revised during Step 6's testing** — see the Decisions Log for what changed and why: direction now alternates every shot instead of staying fixed, and a single `firePoint` became `leftFirePoint`/`rightFirePoint`.

#### Step 4 — `Garlic.prefab` `[x]`

`SpriteRenderer` (`Sprite_Garlic`), `Rigidbody2D`, trigger `Collider2D`, `ProjectileGarlic`, `SC_Death`. No `Sprite_` prefix since it's spawned at runtime, matching `Axe.prefab`/`Fireball.prefab`.

#### Step 5 — `Sprite_Vampire` prefab `[x]`

New prefab under `World > Enemies`: `SpriteRenderer` (`Sprite_Vampire`), `Collider2D`, `EnemyHealth` (destroyed by fireball/axe, reused as-is), `SC_Death` (touch costs a strike, reused as-is — going beyond the exercise's literal text for item 7, Peleg's call), `EnemyRangedAttack` wired to `Garlic.prefab`, plus a child transform as the fire point, positioned clear of the vampire's own collider so the garlic doesn't spawn overlapping it.

**Revised during Step 6's testing** — see the Decisions Log: the single fire point became two (`FirePointLeft`/`FirePointRight`), repositioned further out after a real collider-overlap bug.

#### Step 6 — Core mechanic playtest `[x]`

Confirm the vampire stands still and alternates firing garlic left/right on a steady interval; confirm a garlic hit costs exactly one strike and destroys the garlic; confirm touching the vampire directly also costs a strike; confirm a thrown axe or fireball destroys the vampire; confirm both garlic and Mario's fireball stop dead on a wall/floor tile instead of flying through, while still passing through pickups (and, for garlic, enemies too); confirm nothing regressed for spikes (touch, strike loss, game-over reload) or the ghost, since `SC_Death`'s new trigger path and `ProjectileFireball` are both shared code.

**Confirmed working** by Peleg via `OutputLogsTemp.txt`, one continuous session covering everything above plus a full game-over/restart cycle: the vampire alternates garlic left/right on schedule, a garlic hit and a direct vampire touch each cost exactly one strike, both the axe and the fireball destroy the vampire (and, re-tested after the restart, the ghost too), garlic and fireball both stop dead at walls (`"Garlic hit a wall"` / `"Fireball hit a wall"`) while coins, axe pickups, the fire flower, and the strike pickup are all still collected normally by Mario walking through them, and a three-strike death against spikes reloads the scene cleanly with everything - weapons, enemies, pickups - back to its starting state. No Console errors anywhere in the log.

### Stage 8 — Level-end flow `[x]`

A Key pickup + a Door that only ends the level if the key was collected.

#### Step 1 — `KeyPickupController` `[x]`

New script (`Player/Pickable/`), modeled on `SC_Coin`/`SC_Death` rather than the `IPowerUp` pickups - one class, no interface, fires its own static `OnKeyCollected` event directly on pickup. New `Sprite_Key` prefab (`Sprite_Key.png` already sourced, not yet imported into `Assets/Sprites` - needs Texture Type set to "Sprite (2D and UI)" on import), placed in the level like a coin.

**Confirmed working** by Peleg: walking into `Sprite_Key` collects it and logs `"Key collected"`.

#### Step 2 — `Gateway` reacts to the key `[x]`

New script (`Assets/Scripts/` root, alongside `StrikesManager.cs`), attached to a new `Sprite_Gateway` prefab. Subscribes to `KeyPickupController.OnKeyCollected`; the only thing it does on that event is activate a nested `Sprite_Portal` child object (starts inactive in the prefab). No collider on the gateway itself - Mario walks straight through it, key or no key, until the portal turns on.

**Confirmed working** by Peleg: `Sprite_Gateway`/`Sprite_Portal` placed and wired, portal sprite sorts behind the gateway via `Order in Layer` (gateway `0`, portal `-1`).

#### Step 3 — `Portal` ends the level `[x]`

New script (`Assets/Scripts/` root), attached to the nested `Sprite_Portal` child. Owns the trigger collider Mario actually reaches; `OnTriggerEnter2D` checks the `Player` tag, logs "game won", and defers `SceneManager.LoadScene()` to `Update()` the same way `StrikesManager` does. No `hasKey` check needed anywhere - the portal can't be touched before `Gateway` activates it, so "nothing happens without the key" falls out of the GameObject being inactive rather than a conditional in code.

**Confirmed working** by Peleg via `OutputLogsTemp.txt`: `"Mario reached the portal"` followed by `"Game won - restarting"`, then a clean scene reload (`"Starting with 1 axe(s)"` right after, same as every other reload).

#### Step 4 — Core mechanic playtest `[x]`

Confirm walking through the gateway before collecting the key does nothing at all; confirm the key can be collected anywhere in the level; confirm the portal appears the moment it's collected; confirm walking into the portal logs "game won" and restarts the game; confirm nothing else (strikes, weapons, other pickups) regressed.

**Confirmed working** by Peleg via `OutputLogsTemp.txt`, across two separate playtests: the full key → portal → "game won" → reload sequence traced cleanly, and a second run covering the rest of the game (axes, coins, fire flower, both enemies, a full 2-strike hazard sequence) alongside it with no regressions and no Console errors anywhere in either log.

### Stage 9 — Final assembled level `[x]`

Camera follows Mario on both axes. Confirmed directly from the scene file before writing any
code: every mechanic from Stages 1-8 already has exactly one instance placed in the map (a
roughly 17x9 unit space, full-width ground strip at Y=-1, platforms stepping up to Y=8) - so
this stage is mostly about wiring the camera, then confirming the whole thing plays as one
path start to finish.

#### Step 1 — `CameraFollow` `[x]`

New script (`Assets/Scripts/`, alongside `StrikesManager`/`Gateway`/`Portal` - attached
directly to `Main Camera` itself, not a `Scripts`-folder manager, matching how `Gateway`/
`Portal` attach to their own placed objects rather than a manager pattern). Reads a
`[SerializeField] Transform target` (assigned to `Sprite_Mario`) and follows it on both X and
Y every `LateUpdate`, smoothed via `Vector3.SmoothDamp` instead of snapping straight to
Mario's position, with `smoothTime` exposed as a serialized field instead of hardcoded.
Preserves the camera's own starting Z (read once in `Awake`, not hardcoded to `-10`) so depth
is untouched. Follows freely with no level-bounds clamping - Peleg's call, in scope for a
basic level.

No World-offset special-casing needed: `Transform.position` is always world-space regardless
of parenting, so reading Mario's live position already accounts for `World`'s `X +0.5` shift
automatically. This resolves the Stage 2.5 note about the camera sitting off from the level,
without any Hierarchy change - `Main Camera` stays at the root, unparented, matching its
existing spot as a sibling of `World`/`Canvas`/`Scripts`.

**Confirmed working** by Peleg: "very nice feel."

#### Step 2 — Core mechanic playtest `[x]`

Full playthrough from Mario's start position to the gateway/portal, camera following the
whole way. Confirm every mechanic from Stages 1-8 still works together in the assembled
level: coins, axe pickup/throw/land/reclaim, weapon switching, strikes lost/gained, both
enemies (patrol/touch/kill-by-projectile), key pickup, and the portal ending the level. Also
confirm the camera itself: smooth follow on both axes, no jitter, and how it looks near the
level's edges given free-follow (expected to show some space past the tiles at the corners -
not a bug, that's what "follow freely" means here).

**Confirmed working** by Peleg via `OutputLogsTemp.txt`: one continuous playthrough from
start to the portal, covering coins, the fire flower and weapon switch, axe pickup/throw/
reclaim (`axesHeld` tracked correctly through the pickups, up to 2 then 3), a strike gained
and correctly capped at the max of 3, the ghost turning around at a wall and dying to a
fireball, the vampire firing garlic both directions and dying to a fireball, garlic and
Mario's own fireball both stopping dead at walls, the key collected and the gateway lighting
up, and finally `"Mario reached the portal"` / `"Game won - restarting"` followed by a clean
reload (`"Starting with 1 axe(s)"` right after, same as every other reload). No Console
errors anywhere in the log.

### Stage 10 — Game over / Game won signaling `[x]`

Plain on-screen text for 1 second: red "GAME OVER" on death, green "GAME WON" on reaching
the active portal. Not asked for in the exercise text - Peleg's own addition.

#### Step 1 — `GameEndManager` `[x]`

New manager (`Assets/Scripts/`, own GameObject under `Scripts`) decides which of two new
events came first - `StrikesManager.OnGameOver` or `Portal.OnGameWon` - and locks in that
outcome's message/color. A win reached the same frame as a death always wins the tie -
Peleg's call - handled by giving `OnGameWon` priority to override an already-latched
game-over state, never the other way around. Reloads the scene right away (deferred one
`Update()` tick, same pattern `StrikesManager`/`Portal` always used, no added delay) rather
than waiting for the message to finish - see the Decisions Log for why that changed from the
original design.

#### Step 2 — `StrikesManager` / `Portal` simplification `[x]`

Both lose their own deferred-to-`Update()` reload flag and `SceneManager.LoadScene()` call -
now safe to fire inline, since neither calls `LoadScene` anymore and it's `GameEndManager`'s
job now, still deferred to its own `Update()`. `StrikesManager` fires a new `OnGameOver`
event once strikes hit 0; `Portal` fires a new `OnGameWon` event when Mario reaches it. This
is a net simplification, not just a relocation - both classes get shorter.

#### Step 3 — `GameEndMessageManager` + `Txt_GameEndMessage` GUI `[x]`

Manager living under `Scripts` (unchanged Hierarchy spot throughout this stage's revisions),
paired with `Txt_GameEndMessage` under `Canvas`, top-center, bold, separate from the top-left
stack the other four `Txt_` counters sit in. Reads `GameEndManager`'s pending message once in
`Start()` - a plain static field, not an event, is what lets the message survive the scene
reload that already happened by the time this object exists. Runs its own 1-second countdown
independent of anything else, then clears the text back to empty.

#### Step 4 — Core mechanic playtest `[x]`

Confirmed via `OutputLogsTemp.txt`: a normal death and reaching the portal (with the key
already collected) each traced cleanly through to their own message and a single clean
reload, no stray errors, nothing regressed in either path (strikes still track and cap
correctly, coins/fire flower/axe pickup/both enemies/key+portal all still fired normally
around the two endings).

### Stage 11 — Bonus: enemy spawner `[x]`

Destroyed enemies respawn after X seconds. Scoped narrower than the exercise's literal generic
"enemies": a spawner tied to a grave sprite, spawning ghosts only - no vampire spawner.

#### Step 1 — `EnemySpawner` script `[x]`

New script (`Assets/Scripts/Enemy/EnemySpawner.cs`). One `async`/`Task`-driven timer modeled on
Lesson 5's own `EnemySpawner.cs` (`async`/`await` + `CancellationTokenSource`), spawning a
serialized `enemyPrefab` immediately on start and then every `spawnInterval` seconds after,
capped at `maxAlive` - counted from a private list of what this spawner has itself made, pruned
of destroyed entries before each check, not a scene-wide lookup or an `EnemyHealth` event. Data-
driven rather than subclassed per enemy type - see the Decisions Log for the OCP discussion.

**Revised during Step 4's testing** - the lesson's `Task.Delay` had to be replaced with a
game-time wait (`WaitGameSecondsAsync`, yielding once per frame and accumulating
`Time.deltaTime`). See the Decisions Log for the full investigation; it took three passes and
two wrong diagnoses to get there.

#### Step 2 — `Sprite_Grave` + Inspector wiring `[x]`

New GameObject (sprite renderer using `Sprite_Grave.png` + `EnemySpawner`, no collider - it's
not a hazard or a pickup) placed under `World`, as a sibling of `Enemies` rather than inside it.
`Enemy Prefab` → `Sprite_Ghost.prefab`, `Spawn Interval` → `3`, `Max Alive` → `3`.

**Confirmed working** by Peleg via `OutputLogsTemp.txt` - the first attempt had `EnemySpawner`
sitting on `Canvas` instead (same "wrong Hierarchy selection" mistake as Stage 8's stray
`Gateway`), traced from ghosts spawning at Canvas's screen-space position and never appearing
in the level; moving the component onto a proper `Sprite_Grave` under `World` fixed it.

#### Step 3 — Remove the pre-placed `Sprite_Ghost` `[x]`

Delete the existing hand-placed ghost from `World > Enemies`. With the spawner as the sole
source of ghosts, its own cap is the level's true ghost total.

**Confirmed working** by Peleg - every fall/land log in `OutputLogsTemp.txt` reads
`Sprite_Ghost(Clone)`, never plain `Sprite_Ghost`, confirming the original instance is gone.

#### Step 4 — Core mechanic playtest `[x]`

Confirm a ghost spawns immediately on level load, then again every 3 seconds up to the cap of
3; confirm killing one opens a slot for the next tick rather than the count staying stuck;
confirm newly spawned ghosts fall and land cleanly near the grave, including near tile seams
(the Stage 6 note about tolerating momentary wrong grounding answers applies here too); confirm
no Console errors anywhere, including across a scene reload (expect one clean
`"Enemy spawner stopped"` log per reload, not an error - see the Decisions Log).

**Confirmed working** by Peleg over several sessions, the last of which finally matched on
screen what the logs claimed: the cap holding at 3 and freeing a slot the moment a fireball
killed one, every spawned ghost falling and landing normally, and a clean
`"Enemy spawner stopped"` log with no exceptions on scene end. No Console errors anywhere. The
final log's dual clocks read 2.980s game / 5.394s real for the first interval and ~3.00s on
both clocks for every one after it, which is the timing bug's own signature after the fix -
the freeze is still there, it just no longer eats the wait.

### Stage 12 — Final jump tuning & full test `[x]`

A final pass on Mario's jump and walking feel, plus one last complete playthrough before
recording. Not feature work - everything here is either a bug in existing movement code or an
Inspector value that needs a different number.

#### Step 1 — Mario can jump in mid-air `[x]`

`PlayerJump.Jump()` only gated on `isJumping == false`, and `isJumping` was only ever set back
to `false` by `OnFloorCollision()`. So the flag meant "has an unfinished jump in progress",
never "is airborne", and the two differ in exactly one case: walking off a ledge without
pressing Space leaves it `false` for the whole fall, and Space still fires a full impulse from
mid-air.

Fix: a new `IsGrounded()` check in `PlayerJump`, sampled once at the moment Space is pressed
rather than tracked per frame, gating the jump alongside the existing `isJumping` flag. Both
conditions stay, doing two different jobs. Geometry and the reasoning behind sampling on
demand are in the Decisions Log below.

**Confirmed working** by Peleg via `OutputLogsTemp.txt`: jumping from flat ground, from
platform edges and across tile seams all work; pressing Space mid-fall after walking off a
ledge is refused four separate times in the log; mashing Space at the apex is refused; landing
and immediately jumping again works. No Console errors.

#### Step 2 — Wall-stick and walking slipperiness `[x]`

Two symptoms, one shared cause: physics was doing jobs `PlayerMovement` should own.

Wall-stick (holding a direction key against a wall face left Mario hanging in mid-air):
`PlayerMovement` sets `linearVelocity.x` directly every `FixedUpdate`, so pressing into a wall
makes the solver cancel that velocity every step, producing a large contact normal impulse.
Friction is capped at `friction * normal impulse`, which with the default 0.4 works out about
seven times larger than gravity's pull per step, so Mario didn't slide down.

Slipperiness: `Linear Damping` was already at `5` from Stage 2.5, and under exponential damping
the coast distance after releasing a key is `speed / damping` = 5/5 = one full floor tile.

Fix: a new frictionless `PhysicsMaterial2D` (`Assets/Physics/Frictionless`, friction and
bounciness both `0`) on `Sprite_Mario`'s `CircleCollider2D` for the wall-stick, plus a
`deceleration` field on `PlayerMovement` (starting at `40`) braking horizontal velocity with
`Mathf.MoveTowards` when no key is held. Applied in the air as well as on the ground, Peleg's
call, which is what keeps `PlayerMovement` free of a per-frame ground check. `Linear Damping`
and `jumpSpeed` stay where Stage 2.5 tuned them so the jump feel already signed off on didn't
need redoing.

**Confirmed working** by Peleg via `OutputLogsTemp.txt`: Mario slides down a wall face instead
of hanging, walking and running jumps both feel right ("very nice feel"), the jump arc is
unchanged, and a spikes hit still costs exactly one strike with both `SC_Death` subscribers
firing. No Console errors.

#### Step 3 — Full playthrough `[x]`

One last complete run covering everything from Stages 1-11 together, confirming nothing
regressed before recording - in particular that no platform became un-jumpable now that a
landed axe and an enemy's head deliberately don't count as ground.

**Confirmed working** by Peleg directly in the Editor rather than via a pasted log.

### Stage 13 — Video script `[ ]`

Write out the recording script/checklist covering every requirement, per the submission
rules below.

## Notes / Decisions Log

_(append entries here as we make design decisions, e.g. "Chose to model lives via a GameManager singleton because ...")_

- Kept the `SC_` prefix on `SC_Coin`, `SC_CoinsManager`, `SC_Death`, `SC_Floor` rather than renaming to match the newer scripts. Reason: these four are untouched since the pre-SOLID "Mario Start" project (confirmed byte-identical across all four course zips), so the prefix marks real lesson history rather than sloppiness. New scripts going forward will not use the prefix.
- `WeaponsHandler.index` (which weapon Left-Ctrl fires) stays hardcoded for now; fixing it properly is part of the Pickable Axe stage, since that stage changes how weapons get registered anyway.
- Stage 2 design decisions (Pickable Axe):
    - `AxePowerUp` will look up `IReloadWeapon` (interface), not the concrete `AxeWeapon` type — matches the `FireFlowerPowerUp`/`IUseableWeapon` fix from Stage 0. This is the ISP/DIP point in practice: `AxePowerUp` only needs "can this be reloaded," not the whole concrete weapon.
    - The axe becomes a real stockpile: `AxeWeapon._loaded` (bool) is replaced by `axesHeld` (int, starts at `0`). `Reload()` now only means "gained an axe" and is called exclusively from the pickup path — the old Q-key manual reload in `TempInit` is removed.
    - Considered splitting `ProjectileAxe`'s throw force into separate classes the way `PlayerMovement`/`PlayerJump` are split — rejected. That split exists because movement and jumping already had two independent reasons to change (different keys, different persistent state). The axe's throw is one physical event (a single `AddForce` with both components); there's no independent axis of change to split along.
    - `WeaponsHandler.index` fix, revised: first considered giving the axe and fireball their own dedicated attack keys (dropping `index` entirely), but reconsidered after re-reading the original code. `index` was a bare `public int` over a generic `List<IWeapon>`, with no method anywhere to change it and no call to `UnEquip()` anywhere either — that shape reads as "one active/selected weapon," not "every weapon independently attackable." Went with keeping that shape and actually finishing it: `WeaponsHandler` keeps `List<IWeapon>` + a private `selectedIndex`, Left-Ctrl attacks the selection, `Q` (freed up now that axe reload no longer needs it) cycles to the next weapon in the list that's actually available. Needed `IWeapon.IsAvailable()` for this — added to the base interface rather than a segregated one, since (unlike `Reload()`/`Equip()`) every weapon the handler manages must be able to answer it for cycling to work at all.
    - `FireFlowerController`'s trigger-detect-and-hand-off shape is being copied directly for `AxePickupController` rather than generalized into a shared base class now — two instances isn't enough to justify the abstraction yet. Revisit once Pickable Strike (Stage 4) makes it a third.
    - Confirmed `Axe.prefab` currently has no `Collider2D` and its `Rigidbody2D` constraints freeze vertical movement (`m_Constraints: 6` = FreezeRotation + FreezePositionY) — it could not land even before this stage. Both get fixed as part of making the throw arc and land.
    - `axesHeld` starts at `1`, not `0` — Peleg wanted Mario to start with one axe already, not needing to find one first. `AxeWeapon.Start()` fires `OnAxeCountChanged` once so the GUI picks up the correct starting value without a separately hardcoded "1" in the Inspector.
    - A landed, resting axe is re-collectible (walking into it grants it back via the same `AxePowerUp`/`PlayerPowerUp` pipeline as any other pickup), not just inert scenery — this is also why it eventually fades and despawns (`lifetime = 10s`, last `warningDuration = 3s` fades to transparent) rather than sitting forever, giving a real but bounded window to reclaim it.
    - Found and fixed a real bug during testing: a freshly-thrown axe was colliding with Mario himself (spawns essentially at his position) and immediately treating that as "picked back up," making it look like it vanished instantly. Fixed with a `hasLanded` flag on `ProjectileAxe` — Player collisions are ignored entirely until *after* the axe has actually landed on something else first.
    - The despawn warning was originally planned as lerping `SpriteRenderer.color` toward white — doesn't work, since `SpriteRenderer.color` is a multiplicative tint already sitting at pure white (`1,1,1,1`), so lerping toward white is a no-op with the current sprite/material. Switched to fading alpha toward `0` instead, which is achievable without a custom shader and reads just as clearly as "about to disappear."
    - Named the world-placed pickup GameObject/prefab `Sprite_Axe` (not `AxePickupController`-style) to match the existing convention for in-world objects (`Sprite_Mario`, `Sprite_Floor`, `Sprite_Spikes`, `Sprite_Coin`, `Sprite_Flower`) — that naming convention is about what the GameObject *is* in the world, separate from and unrelated to the `SC_` script-prefix convention.
    - Re-examined whether `ProjectileAxe` (now owning throw, landing, re-pickup handoff, and the fade/despawn timer) still holds up as one SRP-coherent class — yes, because the actual "what does a pickup grant" logic was already factored out into `AxePowerUp`/`IPowerUp` earlier, so what's left is genuinely one thing: this one axe instance's physical lifecycle. The fade/despawn timer is the one part that's plausibly reusable (e.g. for `ProjectileFireball` later) but isn't generalized now — no second real consumer exists yet, and extracting from a single case risks guessing its shape wrong.
- Stage 0 script scan (13 findings) — applied directly to the project files instead of copy-paste blocks, at Peleg's request for this batch:
    - `AxeWeapon`/`FireballWeapon`: `new Quaternion(0,0,0,0)` → `Quaternion.identity` (the old value wasn't a valid rotation).
    - `TempInit.Update()`: axe reload now uses `wasPressedThisFrame` instead of `isPressed`, so holding Q no longer spams `Reload()`/the console every frame.
    - `WeaponsHandler.Update()`: dropped the meaningless `weapons.Count >= 0` check; `AddWeapon()` now null-checks its `weapon` parameter.
    - `SC_CoinsManager`: replaced the `GameObject.Find("Txt_Coins")` runtime lookup with a `[SerializeField] private TextMeshProUGUI coinsText` field. **Needs Inspector wiring:** select `Scripts/SC_CoinsManager` in the Hierarchy, drag `Canvas/Txt_Coins` onto the new `Coins Text` field on the `SC_CoinsManager` component.
    - `SC_Floor`: the `0.45f` landing-height magic number turned out to be exactly Mario's `CircleCollider2D` radius (confirmed by reading `Scene_Physics.unity` directly: `m_Radius: 0.45`). Rather than keep it as a hand-tuned constant, replaced it with `col.collider.bounds.extents.y` read live off the colliding collider at the moment of impact — so the "landed on top vs. bumped the side" check now derives from the player's actual collider size instead of a number that would silently go stale if that collider is ever resized. No `[SerializeField]`, no Inspector step needed. Also cleaned up the collision logging (see below).
    - `FireFlowerPowerUp.ApplyPowerUp()`: now looks up `IUseableWeapon` instead of the concrete `FireballWeapon` type — same DIP style as `WeaponsHandler`/`PlayerPowerUp`.
    - Logging cleanup: `SC_Coin`/`FireFlowerController` were both trigger callbacks logging the string `"OnCollisionEnter2D"` (copy-paste leftover) — fixed to `"OnTriggerEnter2D"`. Generic `"Mario Collision!"` lines replaced with per-script messages (`"Coin collected: ..."`, `"Mario hit spikes"`, `"Mario landed on floor"` / `"...from the side"`, `"Fire flower collected"`).
    - `SC_Death.cs`: removed the dead commented-out `SC_Player`/`ResetMarioPosition()` reference.
    - `LaserWeapon.cs` deleted (script + `.meta`) — unused stub, not attached to any GameObject, not required by the exercise.
    - `BaseWeapon`/`TestBaseWeapon` (the LSP demo) — **resolved: keeping it**, since it's the only place in the codebase demonstrating LSP and the exercise requires showing all SOLID principles taught so far. Added explanatory comments to both files (why this exists, what it proves, how it differs from the real `IWeapon` system) instead of deleting. Also dropped `TestBaseWeapon`'s empty, auto-generated `Update()` method (did nothing, just noise) while in there. Nothing to build for the video beyond narrating the Console output when the scene starts.
- Stage 0 logging audit (7 findings, all approved and applied):
    - `PlayerJump.Jump()` now logs `"Mario jumped"` on an actual jump.
    - `PlayerDeath.OnSpikeCollision()` now logs `"Mario respawned at start position"` (message will need revisiting once the Stage 3 lives system exists).
    - `PlayerPowerUp.CollectPowerUp()` now logs `"Power-up collected: " + type name` generically, in addition to each power-up's own specific log (e.g. `FireFlowerPowerUp`'s).
    - `AxeWeapon.Attack()` / `FireballWeapon.Attack()` now log `"Axe thrown"` / `"Fireball shot"` when they actually fire, and `"...ignored - not loaded"` / `"...ignored - not equipped"` when the attack is a no-op (reload/equip gating was previously silent).
    - `WeaponsHandler.AddWeapon()` now logs `"Weapon registered: " + type name` when a weapon is actually added.
    - Left silent on purpose: `ProjectileAxe`/`ProjectileFireball.Attack()` (would duplicate the weapon-level log), `PlayerMovement` (continuous per-frame state, not a discrete transition), `SC_Death`'s unused `OnSpikeCollisionGeneral` branch (nothing subscribes to it yet).
- IDE flagged `col.gameObject.tag == "Player"` (used throughout) as slightly less efficient than `CompareTag`. Decided to leave as-is — negligible real-world cost for this project's scale, not worth touching.
- Stage 2.5 Step 1 (`SC_Floor` repeated-landing log): Peleg first asked whether just tweaking comments would be enough. It wouldn't — the noise is `Debug.Log` actually firing on every tile crossing during a walk, and comments don't run, so they can't change what shows up in the Console or the video. Root cause stays as diagnosed: the floor is 14 separate tile colliders, so walking across a tile boundary re-triggers `OnCollisionEnter2D` even though Mario never left the ground. Fix: `SC_Floor` still detects "landed on top vs. bumped the side" per collision and still raises `OnFloorCollision` on every top-touch — that detection is correct, since it genuinely is a new tile each time — but it no longer decides whether that's log-worthy. `PlayerJump.OnFloorCollision()` already tracks `isJumping`, so it now only logs `"Mario landed on floor"` (and only resets `isJumping`) when `isJumping` was actually `true` — a real post-jump landing, not a walking step. Also removed `SC_Floor`'s unconditional `Debug.Log("OnCollisionEnter2D " + ...)`, which logged every collision regardless of tag — a separate leftover from before the `Player`-tag check existed, same noisy symptom. Left the identical pattern in `SC_Death.cs`/`ProjectileAxe.cs` alone (out of scope here) and left `SC_Floor`'s "touched from the side" log alone (never reported as noisy). Merging the 14 tile colliders into fewer/one — which would fix the repeated `OnCollisionEnter2D` calls at the source instead of just their logging — deferred to Step 2's map-expansion work rather than done twice.
- Stage 2.5 Step 2 (map expansion): floor grew from 14 to about 90 `Sprite_Floor` tiles (still plain prefab instances on a 1-unit grid, not a Tilemap — each tile's `BoxCollider2D` footprint is `1x1` even though its `SpriteRenderer` draws slightly larger at `2.04x2.04` to hide seams, so new tiles need to land exactly 1 unit apart). Tiles moved under a new `Tiles` child of `World` for organization — safe because nothing in `Assets/Scripts` looks up objects by hierarchy path or name (confirmed by search), so reparenting doesn't risk breaking anything, and dragging in the Hierarchy preserves world position automatically. Separately, `World` itself (parent of Mario, every tile, and every pickup/hazard — everything except `Main Camera`) was moved `X +0.5` for visual alignment. Confirmed safe against `PlayerDeath`'s respawn: `startPosition` (`PlayerDeath.cs:18`) is captured live from `transform.position` in `Awake()`, not hardcoded, so it automatically follows wherever Mario's `Transform` actually ends up. Note: `Main Camera` is not a child of `World`, so its fixed framing sits `0.5` unit off from the level now; harmless since camera-follow isn't wired up until Stage 9, but worth knowing if the framing ever looks slightly off before then.
- Stage 3 design decisions (Lives/Strikes):
    - `StrikesManager` is a new, independent subscriber to `SC_Death.OnSpikeCollision`, alongside `PlayerDeath` — neither class knows the other exists. `PlayerDeath` keeps its existing unconditional reposition-on-hit behavior unchanged; `StrikesManager` owns strike bookkeeping and the game-over/restart decision. Kept them separate rather than folding strike-counting into `PlayerDeath`, since "reposition Mario" and "track lives, end the game" are genuinely two different responsibilities.
    - "Restart" means reloading `Scene_Physics.unity` (`SceneManager.LoadScene`) rather than hand-resetting each piece of state (position, coins, axes held, weapon selection) — a full reload gets all of that for free and is a more literal match for "the game ends and restarts" anyway. No Build Settings changes needed since it's still the sole scene.
    - Found a real hazard before writing any code: `SC_Death.OnSpikeCollision` now has two independent subscribers, and C# doesn't guarantee which runs first. `SceneManager.LoadScene()` is blocking, not deferred like `Destroy()` — so if `StrikesManager`'s handler ran before `PlayerDeath`'s on the killing hit, the scene (and `PlayerDeath`'s own GameObject) would already be gone by the time `PlayerDeath.OnSpikeCollision()` ran, throwing a `MissingReferenceException`. Fixed by not reloading inline from the event handler: `OnSpikeCollision()` just sets a `strikesDepleted` flag, and the actual `LoadScene()` call happens in `Update()`, which is guaranteed to run only after every subscriber for that frame's hit has already finished. No visible delay, just removes the dependency on subscriber order.
    - `StrikesManager` will need a public method for gaining a strike back once Stage 4 (Pickable Strike) exists. Not added yet — no caller for it.
    - Game over is silent for now beyond a `Debug.Log` — no on-screen message before the reload. Can revisit as polish later; not tied to Stage 8, which is level-*completion* via Key + Door, a separate flow from dying out of strikes.
- Stage 4 design decisions (Pickable Strike):
    - `StrikePowerUp.ApplyPowerUp()` doesn't touch the `player` argument at all — unlike `AxePowerUp`/`FireFlowerPowerUp`, which reach into `player.GetComponentInChildren<...>()` for something that lives under Mario, strikes are tracked on `StrikesManager`, a standalone scene-level manager. Considered a `FindObjectOfType<StrikesManager>()` lookup instead, rejected — that's the same runtime-lookup pattern Stage 0 already moved away from (`SC_CoinsManager`'s old `GameObject.Find("Txt_Coins")`). Went with `StrikePowerUp` firing a static `OnStrikeGained` event instead, which `StrikesManager` subscribes to exactly like it already subscribes to `SC_Death.OnSpikeCollision` — no reference needed either direction, and `StrikesManager` stays the sole owner of strike state.
    - Upper cap on strikes: capped at `startingStrikes` (so 3, matching the starting count) rather than a separate hardcoded number — Peleg's call, matching how most games cap extra lives at (or near) the starting amount, and derived from the existing field instead of duplicating the value.
    - Sprite: none of the existing pickups' art matches a consistent style anyway (`Sprite_Axe`/`Sprite_Coin` are flat icon-style, `Sprite_FireFlower` is retro pixel art), so no obligation to match either — Peleg sourcing a heart sprite separately.
- Stage 2.5 Step 3 (physics/movement feel): the jump-too-low and fireball-too-slow complaints turned out to share one root cause. `PlayerJump.Jump()` and `ProjectileFireball.Attack()` each called `Rigidbody2D.AddForce()` exactly once (on key-press / on throw) using the default `ForceMode2D.Force`, which is meant for a force reapplied every `FixedUpdate` and integrates as `velocity += (force / mass) * Time.fixedDeltaTime`. Called only once, that means just `Time.fixedDeltaTime` (`0.02` by default) of the configured number ever became real velocity — confirmed empirically, since `ProjectileFireball.speed` had already been manually bumped to `50` and still felt slow. Fix: both `AddForce` calls now pass `ForceMode2D.Impulse` (`PlayerJump.cs:45`, `ProjectileFireball.cs:20`), the mode meant for a one-shot push, so the speed values map directly to actual velocity instead of being silently scaled down by ~98%. Retuned afterward: `PlayerJump.jumpSpeed` `100 → 10` (Inspector, on `Sprite_Mario`), `ProjectileFireball.speed` `50 → 5` (Inspector, on the `Fireball` prefab). `PlayerMovement`'s slipperiness turned out to be a separate, simpler issue: `PlayerMovement.cs`'s velocity-setting code (lines 26-28) only runs while a movement key is held and never explicitly resets velocity on release, so with `Sprite_Mario`'s `Rigidbody2D` `Linear Damping` at its default `0`, Mario just coasted after letting go. Fixed purely via Inspector — `Linear Damping` `0 → 5` — no code change needed; `PlayerMovement.cs` itself is untouched. Also raised `Sprite_Mario`'s `Rigidbody2D` `Gravity Scale` `1 → 1.5` for a snappier fall paired with the smaller `jumpSpeed`. One loose end: the `Fireball` prefab's own `Rigidbody2D` `Gravity Scale` was also changed `1 → 0.5` during tuning, but it has no actual effect — the fireball's `Rigidbody2D` constraints freeze its Y position (`m_Constraints: 6`), so gravity can't move it regardless of scale. Harmless, just noting it's a no-op if it ever comes up.
- Stage 6 design decisions (Moving enemy):
    - The enemy is three independent components rather than one class: a movement/patrol script (ground+wall raycast checks, not a fixed patrol distance), `SC_Death` reused as-is for "touching Mario costs a strike" (same event, same two existing subscribers as spikes), and a new `EnemyHealth` for "destroyed by a projectile." Kept separate because they're genuinely independent axes of change - confirmed by Stage 7's static enemy needing the same destructibility but dropping movement, and (per the exercise text) touch damage, entirely.
    - `SC_Death`'s stray unconditional `Debug.Log("OnCollisionEnter2D " + ...)` - left alone during the Stage 2.5 `SC_Floor` cleanup since spikes don't move - gets removed now, alongside renaming `OnSpikeCollision`/`SpikeCollisionHandler` to something hazard-neutral. Reusing `SC_Death` on a moving object would otherwise spam the Console every time the enemy crosses a floor-tile boundary, the same repeated-collision bug class `SC_Floor` already hit once for Mario's own walking.
    - New `IEnemy` interface (`Assets/Scripts/Interfaces/`) stays to a single `Kill()` method rather than also covering touch-damage - per the exercise text, "destroyed by fireball/axe" is the only trait actually shared by both enemy types (item 6 and item 7), while touch-damage is specific to this one. Worth calling out in the video as the ISP point: a hypothetical enemy that shouldn't hurt Mario on touch just wouldn't get `SC_Death` attached, without `IEnemy` needing to know or care.
    - Movement uses two short `Physics2D.Raycast` checks per `FixedUpdate` (ground-below, wall-ahead) rather than a fixed patrol distance or waypoint markers - the level's actual layout (confirmed by looking at the Scene view directly, not the flat-floor assumption from the Stage 2.5 notes) already has multi-tile-tall platforms and floating single tiles at different heights, so real ground edges and wall faces exist to interact with, no new invisible wall objects needed. Starting facing direction is exposed as a `Direction` enum (`Left`/`Right`) for a readable Inspector field, converted once to the same `±1` float convention the rest of the codebase already uses (`PlayerMovement`, `ProjectileAxe.Attack(float direction)`) for the actual runtime math.
    - Enemies are not repositioned or reset on a non-fatal strike loss, matching every other piece of live game state (coins, axes, weapon selection) - only Mario's position resets there. Confirmed by re-reading `PlayerDeath.OnSpikeCollision()` directly: it only ever touches `transform.position`. Full reset (including enemies) only happens via the game-over scene reload, a different mechanism entirely, not something built specially for enemies.
    - Found during Step 3 testing: this project has `Physics2D`'s `Queries Start In Colliders` setting enabled (`ProjectSettings/Physics2DSettings.asset`), so a raycast whose origin sits inside a collider counts that collider as a hit. Both `EnemyMovement` checks cast outward from the enemy's own collider center, so the single-hit `Physics2D.Raycast` overload always returned the enemy itself, never the floor beneath it - `IsGrounded()` returned `false` permanently. Fixed by switching to the list-returning overload with a `ContactFilter2D`, then walking the results and skipping the enemy's own collider explicitly. Worth remembering for Stage 7 and Stage 10 - any script that raycasts outward from its own collider will hit this the same way.
    - Added a second field, `spriteNativeFacing`, alongside `startingFacing` - Mario's sprite is drawn facing right, the implicit assumption behind the `±1` flip convention used everywhere in this project, but the sourced ghost sprite is drawn facing left. `UpdateSpriteFacing()` now combines both fields to get the actual scale sign instead of assuming every sprite shares Mario's native orientation. Stage 7's vampire sprite will need the same field set correctly, whichever way it's drawn.
    - Found and fixed a real bug during testing: an enemy walking off a platform edge got stuck hovering on the last tile's corner rather than falling or continuing. `FixedUpdate` zeroed horizontal velocity outright whenever `IsGrounded()` was `false`, but a collider whose center has just crossed a tile edge is still physically resting on that tile's corner - neither falling (still supported) nor moving (velocity forced to zero), a genuine deadlock. Fixed by only ever *driving* velocity while grounded and leaving it untouched while airborne, so existing momentum carries the enemy over the edge instead of pinning it there.
    - `Sprite_Ghost`'s `CircleCollider2D` radius ended up at `0.4` (matching `Axe.prefab`), not `0.5` - the level's gaps are exactly one tile (1.0 unit) wide, and a `0.5` radius means a `1.0` diameter, precisely as wide as the gap, so the enemy wedged into every single-tile hole instead of falling through it.
    - `SC_Coin` had the same stray unconditional `Debug.Log("OnTriggerEnter2D " + ...)` already removed from `SC_Death` (Step 1) and `SC_Floor` (Stage 2.5) - found while testing, since the ghost walks through coins on patrol and the leftover log fired every time. Fixed the same way.
    - `EnemyMovement.IsGrounded()` was reworked a second time, from a downward raycast to reading `Collider2D.GetContacts()` directly. The raycast answered "is there ground below my centre?" while physics answered "is anything holding me up?", and those two disagree whenever a round collider rests on a tile's corner: physics refuses to let it fall, the script refuses to let it walk, and it hangs there permanently. Preserving momentum (see above) only rescued the case where the enemy *walked* off a ledge still carrying speed; an enemy that *lands* on a corner has its horizontal velocity killed by the impact and stayed stuck. Reading contacts removes the disagreement at the source rather than narrowing the window where it happens. The check uses `Mathf.Abs(normal.y)` deliberately, since it only needs to tell a floor contact from a wall contact and the absolute value makes that independent of which way round Unity reports the normal.
    - The `Fireball` prefab's new `Collider2D` must have `Is Trigger` checked. Left solid, both it and the enemy's collider are solid, so Unity raises no trigger event at all and the fireball simply bumps into things physically - it visibly sticks to walls and shoves the enemy around instead of destroying it.
    - A thrown axe kills an enemy only while it is still in flight. This is deliberate, not an oversight: the `IEnemy` check sits inside `ProjectileAxe`'s `!hasLanded` branch, so once an axe has landed it goes back to ignoring everything except Mario. A landed axe freezes all its `Rigidbody2D` constraints, which makes it an immovable solid object, so an enemy that walks into one reads it as a wall and turns around exactly as it would at a tile face. Worth mentioning in the video, since it looks like a bug otherwise.
    - `IsGrounded()` needed a third and final revision after the contact rework: a grace period (`groundLossGrace`, 0.15s) before "no ground contact right now" is allowed to mean "airborne". Contact reporting turned out to be noisy on this floor, which is ~90 separate 1x1 tile colliders rather than one surface - a round collider crossing a seam loses its contact, or gets a briefly tilted normal, for a physics step or two while the solver hands it from one tile to the next. Diagnosed by temporarily logging the enemy's Y alongside each transition: roughly fifteen "falling"/"landed" pairs all reported the identical `y=-0.09`, proving the enemy never moved vertically and the readings were wrong rather than the enemy genuinely stepping down ledges. A real fall lasts far longer than a seam glitch, so waiting 0.15s separates them cleanly. This is the same idea as "coyote time" in platformers generally, and it smooths a noisy measurement rather than making the measurement exact. Merging the tiles into one `CompositeCollider2D` would fix it at source but would break `SC_Floor`, which lives on each tile and reports per-tile collisions.
    - The tiled floor has now caused three separate bugs across the project: Stage 2.5's repeated `OnCollisionEnter2D` landing logs, this stage's corner-perch deadlock, and this stage's grounding flicker. Worth knowing for Stage 11's spawner: anything that asks "am I on the ground" per-frame on this floor needs to tolerate momentary wrong answers.
    - Kept the name `EnemyMovement` rather than something ghost-specific like `EnemyMovementGhost`. The script has no ghost-specific logic at all - everything that makes it behave like this particular ghost lives in Inspector fields (`startingFacing`, `spriteNativeFacing`, `speed`, the buffers), so naming it after one creature would describe the consumer instead of the behaviour and discourage reusing it for Stage 11's spawner. It also matches the existing `PlayerMovement`/`PlayerJump`/`PlayerDeath` convention. Revisit only if a second *style* of enemy movement appears (flying, jumping), at which point both want behaviour-based names; Stage 7's enemy is stationary so it gets no movement script at all.
    - `Canvas` and all four `Txt_` GUI objects were accidentally deleted mid-stage and rebuilt from scratch. The four manager scripts survived intact, so recovery was purely re-creating the TMP objects and re-assigning each manager's serialized field: `SC_CoinsManager.coinsText` → `Txt_Coins`, `SelectedWeaponManager.selectedWeaponText` → `Txt_SelectedWeapon`, `AxeCountManager.axeCountText` → `Txt_Axes`, `StrikeCountManager.strikesText` → `Txt_Strikes`. `Canvas` sits at the root of the Hierarchy, a sibling of `World`/`Scripts`/`Main Camera`/`EventSystem`, since screen-space UI isn't part of the game world. Anchors matter for positioning: new TMP objects default to a centre anchor, so Alt-clicking the top-left anchor preset is what makes the position values behave as "offset from the corner".
- Stage 7 design decisions (Static enemy):
    - The enemy is a vampire throwing garlic rather than a literal fireball - mechanically the same thing the exercise asks for (item 7). Sprites already sourced: `Sprite_Vampire.png` for the enemy, `Sprite_Garlic.png` for the projectile.
    - Touching the vampire also costs a strike, on top of the garlic hit - not required by the exercise's literal text for item 7, Peleg's own call. Confirmed this doesn't touch the `IEnemy`-stays-narrow decision from Stage 6 at all: `IEnemy` only ever covered "destroyed by a projectile," and touch damage has always lived entirely in whether `SC_Death` is attached, independent of it. Adding it here is just attaching the existing `SC_Death` to a second prefab, unchanged.
    - Reusing `ProjectileFireball` directly for the garlic was considered and rejected - it kills anything with `IEnemy` on contact, and the garlic spawns at the vampire's own position, so it would kill its own vampire the instant it fires (the same class of self-collision bug Stage 2 hit with the thrown axe landing on Mario). Went with a separate `ProjectileGarlic` that doesn't check `IEnemy` at all instead.
    - `SC_Death` gains `OnTriggerEnter2D` alongside its existing `OnCollisionEnter2D`, both firing the same `OnHazardCollision`/`OnHazardCollisionGeneral` events. Needed because the garlic's collider has to be a trigger (same reason `Fireball.prefab`'s is, per Stage 6) to fly through terrain, so a projectile costing Mario a strike can't reuse the collision-only path spikes/the ghost use. Purely additive - a solid collider never raises `OnTriggerEnter2D` and a trigger collider never raises `OnCollisionEnter2D`, so nothing about spikes, the ghost, `PlayerDeath`, or `StrikesManager` changes.
    - `ProjectileGarlic` destroys itself on hitting Mario, rather than flying through - matches the instinct that a "got hit" moment should read as a completed event, the same way the axe/fireball destroy themselves on killing an enemy.
    - No `IWeapon`-style interface for the vampire's fire-timer script (`EnemyRangedAttack`). Mario's weapons go through `IWeapon`/`IUseableWeapon`/`IReloadWeapon` because `WeaponsHandler` cycles and dispatches across several concrete weapons polymorphically; nothing manages a list of enemy attacks the same way, so there's no real second implementation to justify an interface yet.
    - `EnemyRangedAttack` (not `VampireShooter` or similar) - named for the behavior rather than the creature, same reasoning as keeping `EnemyMovement` un-ghost-specific in Stage 6.
    - Known non-issue, not being fixed: if a projectile kills the vampire the same frame its fire-timer would've fired, `Destroy()` doesn't take effect until end of frame, so one last garlic could theoretically launch from an already-dead vampire. Same category as the harmless Fireball gravity-scale no-op from Stage 2.5.
    - Step 6 playtest surfaced four real issues, all fixed before this stage counted as done:
    - Direction was a misunderstanding, not a bug: the vampire was meant to alternate left/right on every shot from the start, not always throw left as the original design discussion concluded and got written up as. `EnemyRangedAttack.startingDirection` (renamed from `shootDirection`) now only sets which way the very first shot goes; `Shoot()` flips it after every shot.
    - Alternating direction meant one `firePoint` could no longer work: a shot fired right from a left-side spawn point would fly back through the vampire's own solid collider on its way past. Split into `leftFirePoint`/`rightFirePoint`; `Shoot()` picks whichever matches the current direction.
    - The garlic disappearing the instant it fired - a separate bug, found before the alternating-direction fix even mattered - was a real collider-overlap problem, confirmed against the actual saved numbers: `Sprite_Vampire`'s `BoxCollider2D` half-width is `0.25`, `Garlic`'s `CircleCollider2D` radius is `0.25`, so anything spawning closer than `0.5` units from the vampire's center overlaps it and immediately counts as a hit. The original `FirePoint` sat at `-0.4`, `0.1` short. Both fire points now sit at `±0.65` for a safety margin.
    - `ProjectileFireball`/`ProjectileGarlic`'s wall-detection was first written as a denylist ("destroy on anything that isn't Enemy/Player"), which meant coins and other untagged pickups got treated as walls too - confirmed in `OutputLogsTemp.txt`, where a fired fireball died immediately with no enemy or tile anywhere near it. Rewritten as an allowlist checking for `SC_Floor` specifically, the only component that actually marks a tile as a tile, since none of the pickups or hazards carry a distinguishing tag. Deliberate side effect matching what was actually asked for: both projectiles now also fly through spikes and a landed axe, since neither has `SC_Floor` either.
    - `ProjectileFireball` gained wall-stopping in the same pass as the garlic - a deliberate change from the "flies through terrain unchanged" behavior documented in Stage 6's own log, which that note is now superseded by.
- Stage 8 design decisions (Key + Door / level-end):
    - The gateway has no collider of its own; only its nested `Sprite_Portal` child does, and the portal starts inactive. This makes "nothing happens without the key" literal - Mario walks straight through the gateway sprite with no trigger firing at all, rather than a trigger that fires and no-ops. Peleg's own idea, and it also settles the class split cleanly: `Gateway` only reacts to the key being collected (activates the portal), `Portal` only reacts to Mario physically reaching it (ends the level) - two separate reasons to change, landing on two separate classes because of the physics setup, not an arbitrary split.
    - `Portal`'s `OnTriggerEnter2D` needs no `hasKey` check - it can't be touched before `Gateway` activates it, so "if the key wasn't collected, nothing happens" comes from the GameObject being inactive, not a conditional in code.
    - `Portal` defers its `SceneManager.LoadScene()` call to `Update()`, the same pattern `StrikesManager` uses, for a real (if unlikely) version of the same reason: reaching the door and losing the last strike on the exact same physics frame would otherwise race a synchronous door-reload against `StrikesManager`'s already-deferred one, and the door would win purely because it doesn't wait for `Update()` - not something anyone actually decided. Deferring both means neither can cut the other off mid-dispatch. Which of the two wins if both are true in the same frame is still down to whichever component's `Update()` Unity happens to run first - left as a known, harmless edge case rather than built out with explicit priority, the same way Stage 7 left the vampire's already-dead last-garlic shot alone.
    - Considered routing the key through `IPowerUp`/`PlayerPowerUp.CollectPowerUp()` (`KeyPowerUp`, mirroring `StrikePowerUp`'s shape) and rejected it - not because it wouldn't work structurally, but because a key isn't a power-up conceptually, and the interface's actual job (letting `PlayerPowerUp` treat several concrete power-ups polymorphically) doesn't even apply here: every pickup controller already hardcodes exactly which concrete power-up it hands over, so there's no real polymorphic call site being served. Modeled `KeyPickupController` on `SC_Coin`/`SC_Death` instead - one class, no interface, fires its own static `OnKeyCollected` event directly. `Gateway` subscribes to it the same independent-subscriber way `StrikesManager` already subscribes to `SC_Death.OnHazardCollision` and `StrikePowerUp.OnStrikeGained`.
    - `Gateway.cs`/`Portal.cs` live at `Assets/Scripts/` root, alongside `StrikesManager.cs` - matches precedent that file location tracks "not Player-specific, not Enemy-specific" rather than Hierarchy placement; both scripts are attached directly to their own placed world objects (`Sprite_Gateway`/`Sprite_Portal`), not to a `Scripts`-folder manager GameObject.
    - `Sprite_Key` and `Sprite_Gateway` (with `Sprite_Portal` nested under it) sit flat under `World`, matching every other pickup - no new Hierarchy subgroup, unlike `World > Enemies`, which only got introduced once there were two enemies to group.
    - The gate stays passable in both directions and isn't a locked door in the traditional sense, on purpose - the exercise only specifies "nothing happens" without the key, not that Mario is physically blocked, and a toggle-solid-collider locked door would be solving a requirement that wasn't asked for.
    - Found a real bug during Step 3's testing: collecting the key logged both `"Gateway lit up - key collected"` and a `"Gateway has no portal assigned"` warning from the same pickup. `KeyPickupController.OnKeyCollected` is a static event, so every subscribed `Gateway` reacts to it, not just the one Mario's near - and reading `Scene_Physics.unity` directly turned up a second `Gateway` component sitting on `Canvas`, `portal` field unassigned (`fileID: 0`), unrelated to the actual `Sprite_Gateway` prefab instance's own correctly-wired one. Almost certainly `Canvas` was the active Hierarchy selection by accident when "Add Component -> Gateway" was used during Step 2. Fixed by removing that stray component from `Canvas`; no code changes involved.
    - Sprite asset naming ended up not matching the `Sprite_X.png` convention documented for placed-in-level art: the sourced files are `Gateway.png` and `Portal.PNG` (no `Sprite_` prefix, and the portal one has an uppercase extension). Harmless functionally - Unity references sprites by GUID, not filename - but worth naming as a known inconsistency, the same spirit as the `SC_` prefix note from Stage 0, rather than silently leaving it unexplained. Not renamed, Peleg's call.
- Stage 9 design decisions (Final assembled level / camera follow):
    - Before writing any code, read the scene file directly (not the Scene view, since the assistant can't touch the Editor) to confirm the level was already assembled rather than needing to be built from scratch: every mechanic from Stages 1-8 had exactly one instance placed in a roughly 17x9 unit map, no duplicates, no strays outside `World` (checked `Canvas` and scene root specifically for a repeat of the Stage 8 stray-`Gateway` bug - clean this time). So this stage ended up being camera work plus one full-level playtest, not new level-building.
    - The Stage 2.5 worry about the camera sitting `0.5` off from the level (since `Main Camera` isn't a child of `World`) turned out to be a non-problem once real follow logic exists: `Transform.position` is always world-space regardless of parenting, so a script reading Mario's live position already has `World`'s `X +0.5` shift baked in. No reparenting needed, no hardcoded offset to keep in sync - `Main Camera` stays at the root, matching its existing spot as a sibling of `World`/`Canvas`/`Scripts`.
    - `CameraFollow` follows both X and Y (the level has real verticality, platforms up to Y=8, so X-only would let Mario climb off the top of the frame), smoothed via `Vector3.SmoothDamp` rather than snapping, with `smoothTime` exposed as a serialized field instead of hardcoded. Follows freely with no level-bounds clamping - Peleg's call, in scope for a basic level; the edges will show some space past the tiles when Mario's near a corner, and that's accepted as what "follow freely" means rather than something to fix.
    - Noticed before touching anything: the old fixed camera (position `6,4,-10`, orthographic size `5`) was already sized and centered close enough to the level's actual footprint that it was showing almost the entire map at once, not a close shot near Mario's start. Flagged this since a follow-cam at that same zoom would barely visibly move, which would undercut demonstrating "camera follows Mario" on video. Recommended tightening the orthographic size as a Play-mode tuning pass, same as the Stage 2.5 physics-feel numbers - Peleg tried a lower value and confirmed "very nice feel."
    - No new SOLID material in this stage - `CameraFollow` is a single-purpose class (SRP) with no second implementation to justify an interface, same reasoning as `EnemyRangedAttack` in Stage 7. Agreed with Peleg not to force an OCP/LSP/ISP/DIP angle here; the video's SOLID coverage points back to what earlier stages already demonstrated.
- Stage 10 design decisions (Game over / Game won GUI):
    - The Stage 3/Stage 8 deferred-to-`Update()` reload pattern only worked because the window between "game end triggered" and the actual `SceneManager.LoadScene()` call was near-instant. A 1-second on-screen message blows that window open enough that `StrikesManager` and `Portal` independently racing toward their own reload call stopped being a theoretical edge case - so pulled "how the game ends" out of both entirely into a new `GameEndManager`, rather than giving each of them a longer version of the same private timer. `StrikesManager`/`Portal` now only report their own condition via a plain static event (`OnGameOver`/`OnGameWon`) and no longer call `LoadScene` at all - the thing Stage 3's fix was protecting against (a same-frame reload racing another subscriber to the same collision event) can't happen from either of them anymore, by construction, not by convention.
    - Reaching the portal and losing the last strike on the same frame: Peleg's call is that the win always wins the tie. `GameEndManager` tracks which outcome it's currently ending as and lets `OnGameWon` override an already-latched game-over state, but never the reverse - so whichever of the two physics callbacks Unity happens to dispatch first that frame no longer matters, only which one the design says should matter.
    - (Superseded by the revision note below - `GameEndMessageManager` ended up owning its own 1-second countdown after all, just running after the reload instead of before it.)
    - `gameOverColor`/`gameWonColor` are serialized fields on `GameEndManager` (default red/green) rather than hardcoded `Color.red`/`Color.green` - matches the project's general no-magic-values habit, and Peleg agreed even though a color is a softer case than a tuning number.
    - No new interface introduced for this - `StrikesManager` and `Portal` firing a plain static event that `GameEndManager` subscribes to matches the same publisher/subscriber shape used everywhere else in this project (`SC_Death.OnHazardCollision`, `StrikePowerUp.OnStrikeGained`, `KeyPickupController.OnKeyCollected`). An `IGameEndTrigger`-style interface would have exactly two implementers doing the same thing an event already does - not a real second use case, just a second name for the first one.
    - Revised during testing - the original version above waited the full `displayDuration` before reloading, so the message would finish showing before the scene reset. Playing it back, that read as the game visibly resetting twice: `PlayerDeath` repositions Mario to the start instantly on the killing hit (pre-existing, untouched by this stage), and then a full second later the actual scene reload happened on top of that - long enough for the two resets to feel like separate events instead of one. Peleg's call: don't gate the reload on the message at all; reload immediately like `StrikesManager`/`Portal` always did, and let the message show for its second on top of whatever the freshly reloaded level is already doing. First pass at this went further than it needed to - a dedicated persistent `Canvas` prefab, `Instantiate`, `DontDestroyOnLoad` - to keep the message alive across the reload as a living object. Landed on something smaller: `GameEndManager` stashes the message/color in a plain `static` field and reloads right away; since static fields aren't tied to any scene, they carry across `SceneManager.LoadScene()` on their own. The freshly loaded `GameEndMessageManager` reads that field once in `Start()`, shows the message, and counts down 1 second locally before clearing it - no persisted GameObject, no second `Canvas`, no `Instantiate` needed anywhere.
- Stage 11 design decisions (Bonus: enemy spawner) - in progress, more to come as the remaining open questions get settled:
    - `Task`/`async` chosen over a coroutine or `EnemyRangedAttack`'s plain float-accumulator-in-`Update()` pattern, mirroring `EnemySpawner.cs` from the actual Lesson 5 project (`Task.Delay` + `CancellationTokenSource`) directly. Not the obvious call: Lesson 5's own slide deck (`Async & Tasks.pdf`) lists "timed events" under "When to use Coroutines" by name, and reserves `Task` for non-Unity/CPU-bound/parallel/file-I-O/network work - none of which describes a gameplay spawner on its own, and the slides' one stated reason to prefer `Task` for a Unity-side timer is needing real cancellation, which this spawner doesn't actually need (nothing in the game ever has to stop it early - a scene reload already tears down everything regardless of which mechanism spawned it). Went with `Task` anyway because the lesson's own `EnemySpawner.cs` is a class built for exactly this feature - same name, same shape, explicitly left unfinished (no real `Instantiate` call, no real enemy prefab, its own write-up calls it a placeholder) - which reads as a stronger signal of what the assignment actually wants shown than the slide deck's general-purpose guidance. `CancellationTokenSource` still gets cancelled from `OnDestroy()` for idiomatic cleanup, matching the lesson's own script, even without a real gameplay trigger for it.
    - Spawner behavior: trickle-to-cap, not respawn-on-death. Every X seconds, if the number of enemies this particular spawner has made that are still alive is under its own cap, spawn one more - it does not react to any specific enemy's death, and the count gets checked live each tick rather than tracked via an event, so `EnemyHealth.Kill()` needs no changes at all for this. This is a deliberate departure from the exercise's own literal text ("ברגע שהם מושמדים אחרי X זמן הם יחזרו" - "the moment they're destroyed, after X time they return") - Peleg's call, worth calling out explicitly in the video script as an intentional reinterpretation of the bonus item rather than a literal implementation of it.
    - One `EnemySpawner` class, not a base `Spawner` type with a `GhostSpawner` child - considered, rejected. Every enemy spawner would do the exact same thing (timer, check its own live count against a cap, `Instantiate` a configured prefab at its own position); the only difference between "a ghost spawner" and some hypothetical future spawner is *which prefab*, which is a value, not a behavior, so it belongs in a serialized field rather than a subclass. A second spawner later (if one's ever added) is just a second `GameObject` running this same script with different Inspector values - same reasoning that already kept `EnemyRangedAttack` without an `IWeapon`-style interface and `CameraFollow` single-purpose, no second implementation exists yet to justify a hierarchy.
    - Cap tracking: each `EnemySpawner` keeps its own private list of what it has spawned, pruning entries that are now Unity's "fake-null" (`Destroy()`'d) before every cap check, rather than a scene-wide lookup or a death event from `EnemyHealth`. Stays agnostic to what kind of enemy it made or how it died - it only ever notices "the thing I made is gone now."
    - Starting values: `spawnInterval = 3f`, `maxAlive = 3` - both plain serialized fields, tunable directly in the Inspector without a code change if 3 turns out to feel wrong once it's actually running.
    - The very first spawn happens immediately when the level loads, not after waiting one full interval - matches Lesson 5's own `EnemySpawner.cs` loop shape exactly (do the thing, then `await Task.Delay(...)`, repeat), which also turned out to simplify the loop itself once the order matched.
    - The originally hand-placed `Sprite_Ghost` gets removed from the level. With the spawner as the sole source of ghosts, its own cap is the true total ghost count in the level, not "cap plus whatever was already standing there" - simpler than the population math needed to reconcile a spawner-tracked count against a pre-existing, separately-placed instance.
    - Found during Step 2's testing: ghosts were being created on schedule but never appeared in the level. `Instantiate` places the new enemy at the spawner's own `transform.position`, and `EnemySpawner` had been added to `Canvas` rather than to `Sprite_Grave` - the log gave it away by naming its owner (`... from Canvas`). `Canvas` is Screen Space - Overlay with an 800x600 reference resolution, so its `RectTransform` sits nowhere near the game world's small-unit coordinates; the ghosts were alive and patrolling far off-screen the whole time, which their `EnemyMovement` fall/land logs kept confirming. Same mistake as Stage 8's stray `Gateway` component, from the same cause: `Canvas` happened to be the active Hierarchy selection when Add Component was used. Fixed by moving the component, no code involved. Worth noting that including the owning GameObject's name in the spawn log is the only reason this was a quick diagnosis rather than a hunt.
    - Timing, found during Step 4's testing and only correctly diagnosed on the third pass - two wrong answers came first, both worth recording. The symptom was that the second ghost visibly appeared under a second after the first, despite a 3-second interval. First measurement logged `Time.time`, which showed that first interval as 0.02s in one run and 0.97s in another while every later interval read a clean 3.00s; that got read as `Time.time` being an unreliable ruler during Play-mode startup, concluding the spawning itself was fine. Wrong conclusion drawn from a correct measurement. Second attempt switched the log to a real-clock `System.Diagnostics.Stopwatch`, which showed every interval at a clean 3.00s and looked like confirmation, but only proved the two clocks disagreed with each other. Peleg pushing back a third time (the ghosts still arrived together on screen, whatever the logs said) is what forced the real diagnosis: both readings were accurate simultaneously, and the gap between them *was* the bug. `Task.Delay` counts real seconds and keeps counting while Unity isn't rendering a single frame, and the editor sits frozen for a couple of seconds right after Play while it warms up - so the delay expired mid-freeze and most of the wait was already spent before anything reached the screen. Fixed by replacing `Task.Delay` with `WaitGameSecondsAsync`, which yields once per rendered frame via `await Task.Yield()` and accumulates `Time.deltaTime`, so the wait can only advance while the game is genuinely running. Confirmed afterwards by logging both clocks on every line: the first interval reads 2.980s game / 5.394s real (≈2.4s of invisible freeze, exactly the shortfall that was showing on screen), and every interval after it reads ~3.00s on both. This is the precise tradeoff Lesson 5's slide deck names when it files "timed events" under coroutines rather than `Task` - hit for real rather than read about. Kept `Task`/`async`/`CancellationToken` anyway, since the lesson's own `EnemySpawner.cs` is still the template and cancellation still comes free, while being upfront that the fix makes the `Task` version do by hand what a coroutine's `WaitForSeconds` does natively.
    - One `EnemySpawner` class, confirmed after a real back-and-forth (not settled on the first pass): Peleg's instinct was that a base `Spawner` type with a `GhostSpawner` child fit OCP better, since OCP is literally "open for extension." Countered with the actual OCP examples already in this codebase (`IWeapon` → `AxeWeapon`/`FireballWeapon`, `IPowerUp` → its three implementers) - those exist because the concrete classes have genuinely different *behavior*. Nothing about a ghost-spawner and a hypothetical future spawner would differ in behavior, only in which prefab/interval/cap it's configured with, which is data, not code - so a single class extended via Inspector fields satisfies "add a new case without modifying existing code" more directly than a subclass would, not less. Agreed to revisit as a real subclass (or a smaller virtual hook) only once a second spawner actually needs to *do* something differently, not just spawn something different - same "wait for a second/third real use case" reasoning already used for `AxePickupController` and `EnemyRangedAttack`.
- Stage 12 design decisions (Final jump tuning & full test):
    - The mid-air jump bug was a naming/meaning mismatch rather than broken physics. `PlayerJump.isJumping` was being used as if it meant "airborne", but it was only ever set `true` by `Jump()` itself and only ever cleared by `SC_Floor`'s landing event, so it actually meant "has an unfinished jump in progress". Those two agree everywhere except one case: walking off a ledge, where Mario is airborne with `isJumping` still `false` the whole way down. Kept the flag and added a separate ground check next to it rather than replacing it, since the two now block genuinely different things - `isJumping` stops a second jump during an ascent (the ground probe still finds the tile for a few frames after take-off), `IsGrounded()` stops jumping out of a fall that was never jumped into.
    - `IsGrounded()` is sampled once, at the moment Space is pressed, not tracked per frame. This is the deliberate lesson from Stage 6's `EnemyMovement` saga: everything that went wrong there (corner-perch deadlock, grounding flicker, needing a 0.15s grace period) came from deriving a *state transition* out of a noisy per-frame reading on a floor made of ~90 separate 1x1 tile colliders. Sampling on demand carries no state that can go stale, and a momentarily wrong reading costs one jump input and nothing else, which is invisible in play.
    - The probe is a thin box spanning Mario's footprint, not a small circle under his centre. The first attempt used a circle at the collider's bottom point and failed at platform edges, exactly the corner geometry from Stage 6: Mario's `CircleCollider2D` (radius `0.45`) can rest on a tile's corner with his centre hanging up to that far past the tile edge, so a centre-only probe sees open air while physics is still holding him up. Growing the circle instead was rejected - a circle wide enough to reach back onto the tile also reaches sideways, which would let him jump off a wall he's pressed against in mid-air. The box is kept at 90% of his collider width (`GroundProbeWidthFactor`, a named `const` with the reason in a comment) so it covers the corner case without ever touching a wall beside him. Its centre and width are read live from `Collider2D.bounds`, so no collider size is duplicated in `PlayerJump`, same approach `SC_Floor` already uses for its landing-height check.
    - Ground is defined as "carries an `SC_Floor` component", reusing the allowlist Stage 7 landed on for projectile wall-detection rather than inventing a second rule. Convenient side effect: Mario's own collider, every pickup, a landed axe and an enemy's head are all excluded without naming any of them. Peleg's call that a landed axe and an enemy head shouldn't be jumpable, so this is the wanted behaviour, not a limitation to work around.
    - `Jump()` now logs both refusal paths, not just one. The second line (`"Jump ignored - Mario has not landed from his last jump yet"`) was added mid-testing purely as a diagnostic, to tell "the probe can't find the floor" apart from "`SC_Floor` never reported the landing" without guessing, and kept afterwards since it only fires on a real key press (not per frame) and matches the existing `"Axe throw ignored - not loaded"` precedent.
    - Wall-stick diagnosis (holding a direction key against a wall face leaves Mario hanging in mid-air, pre-existing and unrelated to the ground check): `PlayerMovement` *sets* `linearVelocity.x` every `FixedUpdate` instead of pushing, so a wall forces the solver to cancel that velocity every step, which means a contact normal impulse of about `speed` per step. Friction is capped at `friction * normal impulse`, and with no `PhysicsMaterial2D` anywhere (confirmed: `m_Material: {fileID: 0}` on Mario's collider, `m_DefaultMaterial: {fileID: 0}` in `Physics2DSettings.asset`) Unity's built-in default friction of `0.4` gives roughly `2` per step against gravity's `0.29`. Friction wins by about seven to one, which is why pressing harder into the wall makes him stick harder rather than slide.
    - The wall-stick and the leftover slipperiness are being fixed together because they share one cause: stopping Mario is currently split between a `Rigidbody2D` damping value and a physics-material default he never explicitly set, instead of being owned by `PlayerMovement`. Removing friction alone would make walking *more* slippery, so a frictionless material and explicit deceleration in the movement script only make sense as one change. `Linear Damping` (`5`) and `jumpSpeed` (`10`) stay where Stage 2.5 put them - damping bleeds roughly 9% off vertical velocity per physics step, so changing it would force a jump re-tune that's already signed off.
    - Deceleration uses `Mathf.MoveTowards` (linear, actually reaches zero) rather than another damping-style multiplier (exponential, approaches zero without arriving) - the residual drift is precisely what "slippery" meant here. Starting value `40` units per second squared against a `speed` of `5` works out to a stop in 0.125s over about a third of a tile, versus a full tile before. Left as a plain serialized field so it's tunable in Play mode like Stage 2.5's numbers.
    - Braking applies in the air as well as on the ground, Peleg's call after weighing both. Grounded-only would preserve a running jump's forward momentum, but `PlayerMovement` would then need its own per-frame ground check, which is exactly the flakiness Step 1 was designed to avoid and Stage 6 spent three revisions on. `Linear Damping` at `5` was already eating most airborne momentum anyway, so the simple version turned out to cost nothing that was there to lose.
    - `EnemySpawner`'s dual-clock timing suffix (`" at 2.985s game / 8.722s real"`, plus the `Stopwatch` field and `TimingSuffix()` behind it) removed at Peleg's request while finishing this stage - it was instrumentation for Stage 11's timing investigation, that investigation is written up above, and it read as debug output in a Console about to be recorded. `WaitGameSecondsAsync` itself is untouched, since that's the fix rather than the instrumentation, and each log line still names its owning GameObject, which is what made the `Canvas`-instead-of-`Sprite_Grave` mistake a quick diagnosis.

## Notes for the Video

_(build this up per stage, so recording at the end is just following a checklist rather than a scramble to remember what to show)_

- Submission is graded only on what the video shows. A section not shown in the video counts as not done, even if it's implemented. So the video should visibly cover every requirement below, each clearly identifiable (e.g. call out on-screen or narrate which exercise item you're demonstrating).
- For each requirement, plan to show: (a) the relevant code briefly, (b) it actually running/working in Play mode.
- Per-stage capture notes:
    - Stage 0: not a graded item on its own, nothing needs its own segment. Optional: a line in the narration noting the `SC_` prefix is kept intentionally (pre-SOLID legacy scripts) rather than an oversight.
    - Coin GUI: works as inherited from Lesson 4, confirmed after Stage 0's `Coins Text` rewiring - show the counter incrementing as coins are collected.
    - Pickable Axe + GUI: show `Sprite_Axe` pickups in the level and the starting axe; show `Txt_Axes` incrementing on pickup and decrementing on throw; show the thrown axe arcing, landing, and being walked back into to reclaim it; show it fading out and despawning if left alone too long; show `Txt_SelectedWeapon` and `Q` switching to the Fireball once the Fire Flower's collected, demonstrating the `WeaponsHandler.index` bug fix (both weapons reachable, not just whichever happened to sit at a hardcoded list position). Worth narrating the ISP/DIP points explicitly: `IWeapon.IsAvailable()`, `AxePowerUp`/`FireFlowerPowerUp` depending on `IReloadWeapon`/`IUseableWeapon` rather than concrete weapon types.
    - Stage 2.5: not a graded item on its own, nothing needs its own segment. The tighter jump, throw, and running feel plus the bigger map will just be visible naturally throughout whatever stage segments get recorded.
    - Lives/Strikes system:
    - Pickable Strike: show `Sprite_Strike` in the level; lose a strike, then walk into the pickup and show the Console's `"Strike gained - N remaining"`; show the cap by collecting one while already at 3 (`"...already at max (3)"`).
    - Strikes GUI: show `Txt_Strikes` under `Txt_Axes`, updating live alongside the Lives/Strikes segment above — losing and gaining strikes, and resetting to 3 after a game-over reload.
    - Moving enemy: show it patrolling using the level's actual platform edges/walls, not a fixed distance; show it turning around at a wall face and falling off a ledge; show it costing a strike on touch (Console: `"Mario hit hazard: Sprite_Ghost"`, `"Strike lost - N remaining"`) while other ghosts keep patrolling undisturbed; show it destroyed by both a thrown axe and a fireball (`"Enemy destroyed: ..."`). Worth narrating the ISP point on `IEnemy` staying to just `Kill()`: it only ever covers "destroyed by a projectile," so touch damage is a separate, optional `SC_Death` attachment that both this enemy and Stage 7's vampire opt into independently. Also worth calling out that a landed axe becomes a solid obstacle the ghost bumps into, so it reads as intentional rather than a glitch.
    - Static enemy: show the vampire standing still and alternating garlic left/right on a fixed interval; show a garlic hit costing a strike, separately from touching the vampire directly also costing one; show it destroyed by a thrown axe or a fireball, same as the ghost; show garlic and Mario's own fireball both stopping dead at a wall instead of flying through, while still passing straight through a coin or other pickup. Worth narrating the OCP point: `EnemyHealth` and both projectiles' `IEnemy` checks needed zero changes to support a second enemy type, and `SC_Death` gaining trigger support extends it without touching its two existing subscribers or the ghost/spikes' existing behavior.
    - Key + Door: show the gateway with the portal off, and Mario walking straight through it with nothing happening; show `Sprite_Key` collected somewhere else in the level; show the portal turning on the moment the key's collected; show walking into the portal logging "game won" and restarting the game. Worth narrating the SRP point on splitting `Gateway`/`Portal` into two classes along the same line the collider setup already draws (react-to-key vs. detect-Mario), and the choice not to route the key through `IPowerUp` since it isn't a power-up.
    - Final level + camera follow: show the camera tracking Mario on both axes across the level's platforms - flat ground, the staircase climbs on the left and right ends, the two big elevated shelves - and confirm it stays smooth rather than snapping. Worth narrating that this is a genuinely assembled level, not a fresh test scene: every mechanic from every earlier stage already lives in this one map. Worth noting `CameraFollow` as a small, single-purpose SRP example, and that the World-offset issue flagged back in Stage 2.5 resolved itself once the camera reads Mario's live position instead of a fixed number.
    - Game over / Game won GUI: show a normal death down to 0 strikes - red "GAME OVER" on screen for about a second right after the level restarts; show reaching the portal with the key already collected - green "GAME WON" the same way. Worth narrating that the message intentionally shows after the reload rather than before it (SRP point: `GameEndManager` only ever decides *which* outcome happened and reloads immediately, same as `StrikesManager`/`Portal` always did; `GameEndMessageManager` owns displaying it, on its own timer, entirely decoupled from the reload). Also worth a line on the tie-break: reaching the portal and losing the last strike on the same frame is handled explicitly (win always wins), not left to whichever collision Unity happens to process first.
    - Bonus spawner: show `Sprite_Grave` in the level with no ghosts around it at the start, then ghosts appearing from it one at a time on a steady 3-second beat up to the cap of 3; kill one with a fireball or axe and show the next one arriving to refill the slot, with the Console's `"Enemy spawn skipped - already at cap (3)"` visible while the level is full. Two things worth narrating deliberately. First, this is an intentional reinterpretation of the bonus item rather than a literal one: the exercise says destroyed enemies come back after X seconds, and this instead keeps a steady population topped up to a cap, which reads better in play and means the spawner never needs to know how or whether any particular enemy died. Second, the `Task` timing story, which is the most interesting thing in this stage - it's built on Lesson 5's own `EnemySpawner.cs` (`async`/`await`/`CancellationTokenSource`), but `Task.Delay` had to go, because it counts real time and kept running while the editor sat frozen at startup, so the first two ghosts arrived on screen almost together. Show `WaitGameSecondsAsync` and explain that it only advances while frames are actually rendering. Good place to note this is exactly what the Lesson 5 slides mean by filing timed events under coroutines, and that the fix amounts to hand-building what `WaitForSeconds` gives for free. Also worth a line on the single-class design: one `EnemySpawner` configured by Inspector fields rather than a `Spawner`/`GhostSpawner` hierarchy, since a second spawner would differ only in which prefab it points at, and that's data rather than behavior.
    - Stage 12: not a graded item on its own, so no dedicated segment. The tighter stopping and the fixed wall-stick will just be visible throughout whatever else gets recorded. Two optional narration lines if there's room: Mario can no longer jump out of a fall he walked into, because `PlayerJump`'s flag was tracking "has an unfinished jump" rather than "is airborne"; and the ground check samples once on key press instead of every frame, which is the direct lesson from the ghost's grounding troubles on a floor made of ~90 separate tile colliders.
