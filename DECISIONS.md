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

Replace each existing bay's visual geometry but **preserve stable shelf IDs and authored order**. Use 3 tiers × 6 cover-facing positions, 18 copies per bay; allow incorrect shelving; keep the interaction hit volume broad and non-colliding. Improve only the fresh starting layout to 20 correct, 60 misfiled and 60 loose cases so bins look less barren. Do not forcibly replace an existing player's save.
