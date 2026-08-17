# Night Shift

## Read First

Before changing gameplay or architecture, read:

- `GAME_VISION.md`
- `PROTOTYPE_SCOPE.md`
- `IMPLEMENTATION_PLAN.md`
- `DECISIONS.md`

These files are the source of truth for product direction.

## Project

Night Shift is a first-person cosy organisation game set in a fictional British physical-media retailer in the mid-2000s.

The shop is closed and empty after an exceptionally destructive sale day. The player has one enormous persistent task: restore the entire shop from believable retail chaos to an organised state.

There are no repeating shifts, daily resets, timers, customers, or shop-management loops.

The core gameplay loop is:

1. Find misplaced physical media.
2. Pick it up and inspect it.
3. Identify what it is.
4. Determine where it belongs.
5. Carry it to the correct section.
6. Place it correctly.
7. Gradually restore the same persistent shop over many play sessions.

The finished game is intended to contain thousands of physical media items, including multiple physical copies of the same catalogue title.

## Current Prototype Goal

The prototype exists to prove that physically sorting a dense collection of media is satisfying before we scale content or polish.

Everything should support this question:

> Is identifying, carrying, grouping, and shelving a large amount of physical media enjoyable for an extended period?

## Technology

- Unity 6.5
- C#
- Universal Render Pipeline
- Git

The Unity project lives in `/Game`.

## Current Prototype Systems

The current implementation includes or is expected to preserve:

- First-person movement and mouse look
- Physical CD cases
- Pickup and drop interaction
- Readable prototype media covers
- Shelf placement and validation
- Shared album/catalogue definitions
- Multiple physical copies referencing the same definition
- Dense shelving tests

## Data Model

Keep catalogue data separate from physical stock.

### Catalogue definition

A catalogue entry such as an album contains shared data, for example:

- Stable catalogue ID
- Artist
- Title
- Genre/category
- Sort key
- Artwork reference

### Physical item

A physical `MediaItem` represents one copy in the shop and references a shared catalogue definition.

Multiple physical items may reference the same definition. Do not duplicate album metadata per copy.

## Design Principles

### Tactile

Picking up, carrying, inspecting, grouping, and placing media should feel physical and satisfying.

### Organisational

The game is about solving one enormous mess. The player's reward is seeing chaos become orderly.

### Player-directed

Validate the final organisation, but avoid prescribing the exact workflow. Players should be free to make piles, group duplicates, work one department at a time, or invent their own process.

### Learnable

Player knowledge is part of progression. Repeated exposure should allow players to recognise fictional artists, covers, genres, and locations faster over time.

### Nostalgic

The environment should evoke a fictional British media retailer circa 2004-2008.

Use original fictional media and affectionate parody rather than direct copies of real-world copyrighted artwork.

### Believable chaos

The starting store should look catastrophically untidy after a huge sale day, but not randomly exploded. Disorder should come from plausible retail behaviour: abandoned products, mixed piles, wrong shelves, baskets, display tables, stock behind counters, scattered duplicates, and partially emptied bays.

### Scope discipline

Prefer the smallest implementation that proves the current milestone.

Do not build speculative systems for hypothetical future requirements unless the implementation plan calls for them.

## Explicitly Out of Scope

Do NOT add any of the following unless the design documents are deliberately changed first:

- Repeating shifts or days
- Clock-out gameplay loops
- Daily resets
- Customers
- NPC schedules
- Dialogue systems
- Combat
- Multiplayer
- Shop economy or business management
- Hunger, stamina, or survival systems
- Crafting
- Character stats
- Procedural stores
- Competitive scoring
- Timers
- Elaborate story systems
- Multiple shops

Do not expand project scope proactively.

## Code Guidelines

Prefer focused components with clear responsibilities.

Avoid large all-purpose manager classes.

Keep data separate from physical behaviour where practical.

Current conceptual responsibilities include:

- `AlbumDefinition` / catalogue definition - shared title metadata
- `MediaItem` - one physical copy and its physical behaviour
- `PlayerInteraction` - player pickup/placement interaction
- `ShelfSection` - category, sorting, capacity, and placement rules
- Persistence/save components - physical item state and player progress

Do not introduce a `ShiftManager` or similar shift-completion architecture.

Avoid hard-coding individual artist or album names into gameplay logic.

## Working With Codex

Before implementing a feature:

1. Read the design documents listed at the top of this file.
2. Inspect the existing implementation.
3. Make the smallest change necessary for the current milestone.
4. Do not introduce unrelated systems.
5. Preserve existing working behaviour unless the task explicitly changes it.
6. Verify the project still compiles without errors.
7. Save all modified Unity scenes and assets.
8. Confirm scene changes are written to disk before finishing.
9. Summarise files changed and significant architectural decisions.

When a request is ambiguous, favour prototype simplicity rather than adding features.