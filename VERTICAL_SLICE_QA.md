# Rock / Indie vertical-slice acceptance

**Status:** candidate ready for Unity playtesting; acceptance pending.

Run in Unity **6000.5.8f1**, scene **Assets/Scenes/Prototype.unity**.

## Gate A — Editor/scene and catalogue

- [ ] Unity imports with **zero C# compiler errors**.
- [ ] No missing-script warnings in the scene.
- [ ] Choose **Night Shift > Validate Running Vertical Slice** in Play mode. It should pass 40 albums, 140 CDs and 11 unique shelves.
- [ ] Confirm no duplicate physical IDs, missing definitions or errors during scene startup.
- [ ] All shelving is reachable; no colliders trap the player or leave CDs inaccessible.
- [ ] Cover artwork and spine labels are visibly distinct/readable close up.

## Gate B — Actual free-form sorting

- [ ] Look at a CD and press **E** to pick it up; look away from a shelf and press **E** to drop it.
- [ ] Place a Rock CD in an Indie shelf and confirm placement succeeds.
- [ ] Place a correct-genre CD ahead of an alphabetically earlier artist; confirm placement succeeds.
- [ ] Remove and replace a CD between two others; visible order remains as the player chose.
- [ ] Put duplicate copies together, then confirm the progress feedback can improve.
- [ ] A full shelf refuses on capacity grounds, not because of its category or alphabet.
- [ ] Dense shelves can still be aimed at; individual CDs can be removed.

## Gate C — Persistence and recovery

- [ ] Organise 15–30 items; intentionally leave several in wrong genres and wrong alphabetical positions.
- [ ] Press **F5** and **F9**; correct, wrong and loose states return exactly as saved.
- [ ] Quit Play mode and restart; the same shop resumes, not regenerated disorder.
- [ ] Pick up a shelved case, move and drop it, allow autosave, then reload.
- [ ] Leave the game idle two minutes; periodic autosave must not disrupt it.
- [ ] Test a corrupt main save with valid .bak (using temporary copies) for recovery.
- [ ] Preserve valued older v1 saves separately (see README).

## Gate D — 30–60-minute real player test

Record at least one timed **30–60-minute** session.

| Metric | Observation |
| --- | --- |
| Time before first satisfying sorting action | |
| CDs successfully organised | |
| Accidental drops/misfiles due to controls | |
| Trouble selecting piles or shelf insertion | |
| Moments of learning/recognising albums | |
| Progress feedback felt motivating? | |
| Visible transformation was meaningful? | |
| Frame rate / hitches | |
| Would play another 30 minutes? | |

**Acceptance:** the organising loop remains enjoyable, transforms the shop and persists between sessions. Counts alone do not prove the milestone.

## Known limitations to assess

- Authored loose piles are kinematic until picked up, avoiding a huge initial physics collision.
- Covers are original procedural pattern art, not final illustrated album jackets.
- Progress is calculated using an ordered subsequence of correctly filed cases, not a hard-coded exact slot per copy.
- The shop remains a prototype-scale layout. Controller support and advanced grab controls are later scope.
- Old v1 save files are not migrated.
- There is no Unity CI runner configured; compile, scene and playtest gates must run locally.

Fix critical failures and repeat the test **before** scaling to more music or other media.
