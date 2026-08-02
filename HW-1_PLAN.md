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

0. `[ ]` **Stage 0: clean up & steady base.** Before writing new features, go through the inherited Lesson 4 code together and decide what (if anything) to fix:
   - `WeaponsHandler.index` is hardcoded to `0`, so the Axe is currently unreachable via input (only the Fireball fires). Decide: fix now, or defer to the Pickable Axe stage (it naturally belongs there too).
   - `SC_Death.cs` has a dead commented-out reference to a removed `SC_Player` class.
   - Stray unrelated `Assets/InvoicePersistence` file (leftover from a different lecture example, not part of the game).
   - Inconsistent naming: some scripts use the legacy `SC_` prefix, newer ones don't. Decide on one convention going forward and note the decision in the log below.
   - Beyond this known list, scan the rest of the inherited scripts for the same categories of issue: organization, useful/informative comments, logging, null checks, magic numbers. Note anything found here before deciding what to change.
   - Confirm the base is steady: project opens cleanly, `Scene_Physics.unity` runs with no Console errors/warnings, controls all work as expected.
   - `SampleScene.unity` deleted. **Still open:** Build Settings still points at the now-deleted `SampleScene.unity` and does not list `Scene_Physics.unity` at all. Needs fixing in the Unity Editor (File -> Build Settings -> Add Open Scenes with `Scene_Physics` open, remove the stale entry) before any actual Build.
1. `[ ]` Coin-count GUI (already exists via `SC_CoinsManager` + `Txt_Coins`); confirm it still works after Stage 0 changes.
2. `[ ]` Pickable Axe: collectible axes, GUI shows count, GUI updates when Mario throws one.
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

## Notes for the Video

_(build this up per stage, so recording at the end is just following a checklist rather than a scramble to remember what to show)_

- Submission is graded only on what the video shows. A section not shown in the video counts as not done, even if it's implemented. So the video should visibly cover every requirement below, each clearly identifiable (e.g. call out on-screen or narrate which exercise item you're demonstrating).
- For each requirement, plan to show: (a) the relevant code briefly, (b) it actually running/working in Play mode.
- Per-stage capture notes:
  - Stage 0: (nothing to show; cleanup only, not a graded item)
  - Coin GUI:
  - Pickable Axe + GUI:
  - Lives/Strikes system:
  - Pickable Strike:
  - Strikes GUI:
  - Moving enemy:
  - Static enemy:
  - Key + Door:
  - Final level + camera follow:
  - Bonus spawner:
