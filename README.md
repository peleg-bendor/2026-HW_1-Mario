# 2026-HW_1-Mario

Homework Exercise 1 for the "Methods in Game Development" Unity course — a 2D Mario-style
platformer built on top of the instructor's Lesson 4 example project.

## Starting point

The `Assets` folder started as a direct copy of the instructor's `Lesson 4.zip`: player
movement/jump, coin pickup with a GUI counter, a weapon system (Fireball/Axe, both built on
`IWeapon`-style interfaces), and a Fire Flower power-up that unlocks the fireball. That project
already demonstrates all five SOLID principles taught so far (SRP, OCP, LSP, ISP, DIP) —
see `HW-1_PLAN.md`'s Decisions Log for what got cleaned up before building on it, and why.

## What this exercise adds

Per the instructor's requirements: a coin-count GUI, a pickable axe (with its own GUI counter),
a lives/"strikes" system, a pickable strike (extra life), a moving enemy, a static enemy that
fires at intervals, a key + door level-end sequence, and a final assembled level with camera
follow. Bonus: an enemy spawner.

Full requirements are in `Course/Exercises/Exercise 01.md` (outside this repo, in the shared
course folder). Stage-by-stage progress, decisions, and what's left is tracked in
[`HW-1_PLAN.md`](HW-1_PLAN.md).

## Running it

Open the project in Unity, then open `Assets/Scenes/Scene_Physics.unity` — that's the real
playable scene (`SampleScene.unity` was removed; it was an unused empty template).

## Submission

Graded from a video showing every implemented requirement, both in code and running live —
per the instructor, anything not shown in the video counts as not done.
