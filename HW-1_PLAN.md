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

Mirrors `Course/Exercises/Exercise 01.md`'s own numbering, with an added Stage 0 for cleanup/setup before any new feature work.

0. `[x]` **Stage 0: clean up & steady base.** Before writing new features, go through the inherited Lesson 4 code together and decide what (if anything) to fix:
   - `WeaponsHandler.index` is hardcoded (currently `1` in the scene), so only one weapon is reachable via Left-Ctrl at a time. Confirmed by reading `Scene_Physics.unity` directly: `index: 1`, and `TempInit.Start()` registers the fireball first (list position `0`) then the axe (list position `1`) — so right now Left-Ctrl always fires the **axe**; the fireball is unreachable via input, even fully equipped.
     - **Decision:** defer to the Pickable Axe stage — it naturally belongs there too.
   - `SC_Death.cs` has a dead commented-out reference to a removed `SC_Player` class.
     - **Decision:** remove it.
   - Stray unrelated `Assets/InvoicePersistence` file (leftover from a different lecture example, not part of the game).
     - **Confirmed:** was never copied into this project's `Assets` folder in the first place. Nothing to delete.
   - Inconsistent naming: some scripts use the legacy `SC_` prefix, newer ones don't. Decide on one convention going forward and note the decision in the log below.
     - **Decision:** keep `SC_` as-is. `SC_Coin.cs`, `SC_CoinsManager.cs`, `SC_Death.cs`, `SC_Floor.cs` were byte-identical across all four course zips going back to the pre-SOLID "Mario Start" project at the time we made this decision — the prefix boundary tracks which scripts predate the SOLID refactors, not carelessness. Mention this as an accepted, known inconsistency in the video narration. Note: the Stage 0 cleanup pass below has since made real internal edits to `SC_Coin`, `SC_Death`, and `SC_Floor` (bug fixes, log cleanup, magic-number removal) — the "untouched" fact was only ever the *reason the naming split exists*, not a promise to freeze their internals, so this doesn't change the naming decision itself.
   - Beyond this known list, scan the rest of the inherited scripts for the same categories of issue: organization, useful/informative comments, logging, null checks, magic numbers. Note anything found here before deciding what to change.
     - **Done.** 13 findings reviewed and applied, plus a follow-up logging audit (7 more) and the `SC_Floor` landing-check rework — all in the Decisions Log below. `Coins Text` wired in the Inspector and confirmed working in Play mode; `SC_Floor`'s reworked landing check re-tested afterward and confirmed working. `BaseWeapon`/`TestBaseWeapon` kept (not deleted) with added explanatory comments.
   - Confirm the base is steady: project opens cleanly, `Scene_Physics.unity` runs with no Console errors/warnings, controls all work as expected.
     - **Confirmed** by Peleg.
   - `SampleScene.unity` deleted, Build Settings fixed to reference `Scene_Physics.unity`.
     - **Confirmed done.** `Assets/Scenes/` now only contains `Scene_Physics.unity`; `EditorBuildSettings.asset` lists only `Scene_Physics.unity`.
1. `[x]` Coin-count GUI (already exists via `SC_CoinsManager` + `Txt_Coins`); confirmed still working after Stage 0 changes (the `Coins Text` serialized-field rewiring included).
2. `[~]` Pickable Axe: collectible axes, GUI shows count, GUI updates when Mario throws one. Starting prompt written for a fresh conversation to tackle this stage; also where the Stage 0-deferred `WeaponsHandler.index` fix belongs (axe currently unreachable via Left-Ctrl at all - see Decisions Log).
3. `[ ]` Lives/"strikes" system: start with 3, lose one per death, restart the game at 0.
4. `[ ]` Pickable Strike: an extra-life pickup.
5. `[ ]` Strikes-remaining GUI.
6. `[ ]` Simple moving enemy: patrols left-right, kills Mario on touch, destroyed by fireball or axe.
7. `[ ]` Static enemy: stationary, fires a fireball every X seconds.
8. `[ ]` Level-end flow: a Key pickup + a Door that only ends the level if the key was collected.
9. `[ ]` Final assembled level combining everything, with the camera following Mario.
10. `[ ]` Bonus: enemy spawner (destroyed enemies respawn after X seconds).

## Notes / Decisions Log

_(append entries here as we make design decisions, e.g. "Chose to model lives via a GameManager singleton because ...")_

- Kept the `SC_` prefix on `SC_Coin`, `SC_CoinsManager`, `SC_Death`, `SC_Floor` rather than renaming to match the newer scripts. Reason: these four are untouched since the pre-SOLID "Mario Start" project (confirmed byte-identical across all four course zips), so the prefix marks real lesson history rather than sloppiness. New scripts going forward will not use the prefix.
- `WeaponsHandler.index` (which weapon Left-Ctrl fires) stays hardcoded for now; fixing it properly is part of the Pickable Axe stage, since that stage changes how weapons get registered anyway.
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

## Notes for the Video

_(build this up per stage, so recording at the end is just following a checklist rather than a scramble to remember what to show)_

- Submission is graded only on what the video shows. A section not shown in the video counts as not done, even if it's implemented. So the video should visibly cover every requirement below, each clearly identifiable (e.g. call out on-screen or narrate which exercise item you're demonstrating).
- For each requirement, plan to show: (a) the relevant code briefly, (b) it actually running/working in Play mode.
- Per-stage capture notes:
  - Stage 0: not a graded item on its own, nothing needs its own segment. Optional: a line in the narration noting the `SC_` prefix is kept intentionally (pre-SOLID legacy scripts) rather than an oversight.
  - Coin GUI: works as inherited from Lesson 4, confirmed after Stage 0's `Coins Text` rewiring - show the counter incrementing as coins are collected.
  - Pickable Axe + GUI:
  - Lives/Strikes system:
  - Pickable Strike:
  - Strikes GUI:
  - Moving enemy:
  - Static enemy:
  - Key + Door:
  - Final level + camera follow:
  - Bonus spawner:
