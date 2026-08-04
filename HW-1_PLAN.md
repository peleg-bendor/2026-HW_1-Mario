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

Mirrors `Course/Exercises/Exercise 01.md`'s own numbering, with an added Stage 0 for cleanup/setup before any new feature work, and an added Stage 2.5 side-step (like Stage 0, not part of the exercise's own numbering) once Stage 2 surfaced things worth fixing before building further on top of them.

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

### Stage 5 — Strikes-remaining GUI `[ ]`

Show the current strike count on screen.

### Stage 6 — Simple moving enemy `[ ]`

Patrols left-right, kills Mario on touch, destroyed by fireball or axe.

### Stage 7 — Static enemy `[ ]`

Stationary, fires a fireball every X seconds.

### Stage 8 — Level-end flow `[ ]`

A Key pickup + a Door that only ends the level if the key was collected.

### Stage 9 — Final assembled level `[ ]`

Combining everything, with the camera following Mario.

### Stage 10 — Bonus: enemy spawner `[ ]`

Destroyed enemies respawn after X seconds.

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
    - Strikes GUI:
    - Moving enemy:
    - Static enemy:
    - Key + Door:
    - Final level + camera follow:
    - Bonus spawner: