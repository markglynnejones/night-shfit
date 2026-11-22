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
- [ ] Press **P** and **O**; correct, wrong and loose states return exactly as saved.
- [ ] In WebGL, press P to save, then refresh the webpage and verify persistence; **F5 refreshes browsers**.
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

## Scale and readability follow-up (first five-minute feedback)

- [ ] A physical CD should be approximately **17.2 cm by 13.6 cm** rather than the original 78 cm by 62 cm.
- [ ] Player eye starts at **about 1.65 m** above floor; walk into the room and confirm no floating start or excessively tall viewpoint.
- [ ] The table, browse bins and loose floor stacks sit on their surfaces without visible floating or intersections; cases in a pile are about 2 cm apart vertically.
- [ ] Shelf rows hold small CDs closely, with a narrow, neutral placement marker rather than an oversized highlighted slot.
- [ ] Rock shelving uses burgundy signs and Indie shelving uses muted petrol/teal signs, with legible cream lettering.
- [ ] Room walls and floor have a warmer, calmer material palette, with less harsh lighting.
- [ ] Aim at a small CD: a bottom-right card identifies its genre, artist and album title, and shows **E to pick up**. Pick it up and check **E to place/drop**.
- [ ] Verify an incorrectly filed CD can still be placed and saved; check your existing progress still loads.
- [ ] With a saved v2 shop from before the scale pass, confirm loose cases are automatically resettled on supports, while shelf IDs/order remain intact. Keep a backup before testing.
- [ ] Play for 5–10 minutes and note any cases now too fiddly to pick up, shelf gaps, or unreadable signs.

If the new dimensions are too small to target reliably, adjust the interaction affordance or spacing rather than reverting to giant physical CDs.


## Flip-through browsing rack visual acceptance

Photo reference: real UK high-street music store CD flip bins (roughly mid-2000s): inclined tiers of front covers, narrow retaining lips, section dividers and large genre signs. These are NOT oversized upright bookcases.

- [ ] Each of the 11 stock bays looks like a shallow waist-height browsing bin with three visible rows, not a tall empty display cabinet
- [ ] A CD is nearly real jewel-case size (17.2 × 13.6 cm) and displays its FRONT COVER tilted towards the customer, not an edge-on spine
- [ ] Nine cases fit across a row and three staggered rows are available: 27 physical copies per section
- [ ] Rock uses a burgundy sign, Indie uses petrol/teal, and alphabet ranges remain visible beside each sign
- [ ] Front lips and divider tabs imply real record-store browsing racks, without blocking art or obscuring the interaction target
- [ ] The player can pick up one case in a visually crowded bin (aim at its face and press E), move it, and put it between two others
- [ ] An intentionally wrong genre/artist placement succeeds, remains in player-authored order and survives P/O save and reload
- [ ] The shelving target is a non-colliding trigger, so the player does not bump into invisible interaction boxes
- [ ] When starting with a fresh save, there are 20 correctly filed cases, 60 intentionally misfiled cases, and 60 loose cases. Existing saves are intentionally NOT reset.
- [ ] Check a 5–10 minute playtest for cases that clip through trays, rows that overlap the main sign, too-small album covers or interaction difficulty

**Known limitation:** this is a first geometry/layout pass with procedurally coloured sleeves. The reference photo is a visual target for shelving construction, not a licensed texture asset to reproduce.
