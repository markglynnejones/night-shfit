# Night Shift - Decisions Log

This file records durable game-design and technical decisions so future implementation work does not drift away from the intended game.

## 2026-08-17 - One enormous persistent task

Night Shift is not structured around repeating shifts, nights, missions, or daily resets.

The whole store is one persistent organisation task that may take many play sessions to complete.

Consequences:

- No clock-out gameplay loop
- No shift-complete reset
- No daily timer
- Save/load must preserve the physical state of the store
- Progress is represented by the improving state of the same shop

## 2026-08-17 - Night-time is framing, not session structure

The player is alone after closing because this removes customers and creates atmosphere.

"Night Shift" describes the setting, not a repeating shift mechanic.

## 2026-08-17 - Catastrophic post-sale starting state

The game begins after an exceptionally busy sale day, similar in spirit to the aftermath of Black Friday.

Approximately 80-90% of stock should require organisation.

The chaos must be severe but believable rather than random.

Examples include wrong shelves, mixed piles, empty shelf gaps, baskets, checkout clutter, scattered duplicates, floor stock, and disrupted displays.

A minority of correctly organised stock remains as visual reference.

## 2026-08-17 - Player chooses their own workflow

The game should validate where products ultimately belong without telling the player exactly how to tidy the store.

Players may create temporary piles, group duplicates, clear one genre at a time, clear floors first, or work however they prefer.

Avoid mechanics that unnecessarily force a single optimisation strategy.

## 2026-08-17 - Catalogue definitions and physical copies are separate

A media title is shared catalogue data.

A physical item is one individual stock copy that references the shared definition.

Example:

`Blue Day - International Clever Person` may exist once as catalogue data and six times as physical CD cases.

Consequences:

- Duplicate copies share metadata/artwork
- Physical copies have independent position/state
- Stock quantity can scale without duplicating catalogue content
- Gameplay systems should not hard-code individual album names

## 2026-08-17 - Thousands of physical items are an intended part of the fantasy

The finished store should feel enormous and intimidating at the beginning.

A rough long-term ambition is 2,000-3,000+ physical products across music, film/TV, games, and possible miscellaneous media.

This is a design target, not permission to instantiate thousands of prototype objects before scale/performance testing.

## 2026-08-17 - Music is the first complete media type

Build and prove the music department before implementing DVDs and games at scale.

This lets us establish the interaction, sorting, content pipeline, persistence, atmosphere, and performance model with one media type first.

Only generalise architecture for additional media types once their real requirements are known.

## 2026-08-17 - Music shelf order

Music is organised using the following rule:

**Genre -> Artist A-Z -> Album A-Z -> duplicate copies grouped together**

Consequences:

- Genre determines the broad store area or shelf run.
- Artists are ordered alphabetically within that genre.
- Albums by the same artist are ordered alphabetically by album title.
- Multiple physical copies of the same album sit together as one contiguous group.
- A CD is not considered correctly organised merely because it is inside the correct first-letter section; its relative position amongst neighbouring artists/albums also matters.
- Shelf validation should care about logical ordering rather than requiring one exact hard-coded physical slot for every copy.
- Empty space on a shelf is acceptable as long as the relative ordering of stock is correct.
- Correctly placed items should still snap into clean, consistent physical positions.

Leading articles such as `The` should be ignored for artist alphabetisation unless later playtesting shows that this feels unintuitive.

Example:

```text
ROCK

Black Afternoon
  Broken Television x3

Blue Day
  British Genius x2
  International Clever Person x5

Grey Parade
  The Grey Parade x4
```

## 2026-08-17 - Mid-2000s fictional UK media-retail setting

The store should evoke a British high-street physical-media retailer around 2004-2008 without copying a real retailer directly.

The fictional media catalogue should combine:

- Affectionate parody
- Indirect references
- Completely original fictional media

The goal is a recognisable alternate-2000s pop-culture universe rather than a list of simple opposite-name jokes.

## 2026-08-17 - Player knowledge is progression

The player should naturally become more efficient by learning artists, covers, genres, shelf locations, duplicates, and the store layout.

Traditional XP/levels/skill trees are not required for the core experience.

Convenience abilities should only be considered later if large-scale playtesting demonstrates that the base interaction becomes tedious.

## 2026-08-17 - Dense spines do not need to be perfectly readable at normal distance

It is acceptable for densely stocked CD spines to be difficult to read from across an aisle.

The intended experience can involve getting closer, pulling a case out, inspecting the cover, and gradually recognising products visually through familiarity.

Do not solve this automatically with intrusive floating UI unless playtesting proves it necessary.

## 2026-08-17 - Current technology

- Unity 6.5
- C#
- Universal Render Pipeline
- Git

The Unity project lives in `/Game`.

## 2026-08-17 - Scene-saving discipline

Unity scene changes must be explicitly saved before a Codex task is considered complete.

For tasks touching scenes/assets, Codex should:

- Save all modified scenes/assets
- Verify compilation
- Confirm the scene was written to disk
- Summarise changes

Working milestones should be committed before starting substantial new work.

## 2026-11-22 - Free-form misfiling instead of auto-correct

Wrong-genre and wrongly alphabetised CDs must be physically placeable on any shelf with capacity. Validation controls progress feedback, not permission to place the item. Never snap misfiled cases to their correct location automatically. Save/load must restore the exact physical order the player left, including mistakes.

## 2026-11-22 - First Rock/Indie vertical-slice candidate

The first extended playtest uses 40 fictional albums, 140 physical cases and 11 shelf bays. A minority starts correctly sorted, with others misfiled or scattered in intentional piles. Original procedural sleeve art is sufficient for a first playtest. The implementation is a candidate only; Unity compilation, recovery and a 30–60 minute player test remain acceptance gates.

## 2026-11-22 - Five-minute scale and readability feedback

First in-Editor playtest: the player found the CD physics enjoyable but felt the CDs, shop props and general presentation were oversized and the greybox difficult to read. The next checkpoint scales physical CDs to 0.22 of their prototype root scale (approximately 17.2 by 13.6 cm), retunes shelf and pile geometry, keeps existing Rigidbody pickup/drop behaviour, starts the player on the floor, and improves shop materials, department signs and contextual album readability. The scale pass should not add timed shifts or change the freely-ordered persistence model. Acceptance requires another five-to-ten-minute Unity playtest.


## 2026-11-22 - Shelving reference: music-shop flip-through bins

The first in-game screenshot showed oversized empty upright shelves, even after correcting CD size. The user's visual reference is a UK high-street CD shop with sloped, densely packed face-out browsing trays, front lips and section-divider tabs. Prefer this recognisable retail geometry to bookcase shelves.

Replace each existing bay's visual geometry but **preserve stable shelf IDs and authored order**. Use 3 tiers × 9 cover-facing positions, 27 copies per bay; allow incorrect shelving; keep the interaction hit volume broad and non-colliding. Improve only the fresh starting layout to 20 correct, 60 misfiled and 60 loose cases so bins look less barren. Do not forcibly replace an existing player's save.


## 2026-11-22 - Carry up to ten CDs; scroll to choose

Player asked to hold up to 10 physical CD copies and confirmed that the **mouse wheel should scroll through the carried pile**. The implementation keeps individual copy IDs, with a selected item index; E collects another case until the stack is full, E shelves the selected case when aimed at a rack, Shift+E forces shelving when another case is in front, and G drops the selected case. Add an on-screen ordered stack with selected artist/title, rather than making players infer titles from very small 3D artwork. Save and load the carried IDs and selection in backward-compatible v2 data, retaining the existing free-form shelving and case physics.


## 2026-11-22 — Comprehensive seven-row store pass

The user asked for all outstanding visual/gameplay improvements in one integrated iteration: seven physical browsing rows per section, four CDs stacked behind every facing, enough stock to fill the shop, an unclipped ten-CD carry interface, more legible cover art, smaller readable category signs, physical furniture supports and a warmer retail atmosphere.

Each rack now has nine facings across each of seven rows, with four real item positions deep (252 capacity). The carrying wheel still changes the highlighted carried album; empty-handed scrolling flips the copies behind a facing, persisting their actual physical order. The expanded fixture contains 248 albums / 1,388 CDs, with 228 reference, 892 misfiled and 268 loose cases at fresh start. Original 140 stock IDs and all eleven shelf IDs survive. New expanded stock IDs are appended to existing version-2 saves without resetting progress.

The ten-CD panel now uses two columns of five and truncates long labels with ellipses, while full album detail remains in the focus panel. Covers have original high-contrast bitmap lettering instead of the previous overlapping per-item TextMesh objects. The store has supported furniture, more restrained signage and warmer lighting.

This is an implementation candidate. Unity Editor compilation, frame rate, physical interactions, rendered cover readability and migration are still acceptance requirements; do not claim a finished milestone based on source tests alone.

## 2026-11-22 — Readable CD browsing and inspection

After the seven-row/four-deep and full shop pass, the player's feedback confirmed the racks and frame rate are good. The next priority is to **identify and browse CDs more easily without enlarging the physical cases**. Use the same cached album cover texture for a clear bottom-right card, a held-Q larger inspection screen, a stable selected-CD card while carrying, narrow aim assist for small cases, and a read-only count of cases behind a browsed rack facing. Keep E, Shift+E, G, wheel carrying, player-authored shelf order and version-2 save persistence unchanged. Q is a temporary view rather than a separate inventory or pause mechanic.

## 2026-11-22 — Crosshair aiming alignment

After the latest local Unity screenshot, the player said cursor and aiming seemed offset. The previous shelf logic used the entry point on the broad invisible trigger as the intended case position, even though perspective makes that a different row from the visible cover. The pickup algorithm also used a wider-than-necessary spherecast. Use an exact centre-camera ray, prefer the nearest unoccluded CD, reduce fallback aim assist to 2 cm, resolve the closest physical rack facing and indicate the actual targeted action beside the dot. Keep carrying selection independent of what the cursor points at, and retain the existing shelves and save format.

## 2026-11-22 — Preserve the CD position when picking cases

The player reported two issues after the precise-crosshair pass: **removing a CD seems to shuffle other cases/rows**, and aiming worked on some seven-row shelf tiers but not others. Their intended interaction is physical CD browsing: four individual cases behind each facing; when the front case is removed, only the following cases in that facing move forward.

The old `ShelfSlot` model used a single list index for both logical ordering and physical location, reflowing every subsequent item after a pickup. Replace this with persistent per-item physical slots (face number 0–62 plus 63 times the depth 0–3). Removal compacts only one facing; insertions push back only that facing; wheel flipping rotates only that facing. Maintain a separately ordered list for progress/save ordering. Add an optional fixed-slot field to version-2 save data with a layout marker; legacy files use their existing `shelfOrder` as the original physical slot to migrate without a reset.

Aiming must compare visible CD faces across all racks instead of requiring a ray collision against the old oversized trigger. Preserve occlusion and leave geometry, catalogue, carry stack, physics and save version unchanged. Full Unity interaction testing remains required.

## 2026-11-23 — Fix row 4 and partial row 3 targeting

The player reports row 4 fails across every CD rack and some of row 3 does not work after the independent four-deep-stack change. This is shared targeting geometry, not an individual faulty copy. The previous code picked the closest 3D cover **centre** inside a 16 cm tolerance, despite rows being packed much closer, so it could choose the wrong row. Use the centre-camera ray against actual sloping rectangular cover planes, choose the first front-facing intersection and share that hit with pickup, placement and wheel flipping. A small aiming overlay reports row, column and fullness, to separate failed aiming from a physically full four-deep position. Keep the player's established layout, persistent IDs and physics unchanged.
