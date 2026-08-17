# Night Shift

## Project

Night Shift is a first-person cosy tidying game set in a fictional
British physical-media retailer in the mid-2000s.

The player is the night-shift employee after the store has closed.

There are no customers.

The core gameplay loop is:

1. Find misplaced physical media.
2. Pick it up and inspect it.
3. Determine where it belongs.
4. Carry it to the correct shelf.
5. Place it correctly.
6. Gradually restore the shop to an organised state.
7. Clock out when the shift is complete.

## Prototype Goal

The initial prototype exists to answer one question:

"Is physically sorting CDs into the correct shelves satisfying?"

Everything should serve that question.

## Technology

- Unity 6.5
- C#
- Universal Render Pipeline
- Git

The Unity project lives in `/Game`.

## Prototype Scope

The first prototype contains:

- First-person movement
- One small greybox media shop
- One music department
- Shelving
- Physical CD cases
- Approximately 20 fictional artists
- Approximately 50-60 CDs
- Four genres:
  - Rock
  - Pop
  - Metal
  - Indie
- Picking up CDs
- Dropping CDs
- Inspecting CDs
- Shelf placement
- Correct/incorrect placement validation
- Basic completion tracking
- Clocking out after all required items are organised

## Explicitly Out of Scope

Do NOT add any of the following unless specifically requested:

- Customers
- NPCs
- Dialogue
- Combat
- Multiplayer
- Skill trees
- Shop management
- Economy simulation
- Procedural generation
- Character creation
- Achievements
- Online functionality
- Elaborate story systems
- Multiple shops
- Multiple time periods

Do not expand project scope proactively.

## Design Principles

### Tactile

Picking up, carrying and placing media should feel physical and satisfying.

### Readable

Players should be able to determine where an item belongs by inspecting
the physical object and reading shop signage.

### Nostalgic

The environment should evoke a fictional British media retailer circa
2004-2008.

References to real-world media should be parody or original fictional
material rather than direct copies.

### Small

Prefer the simplest implementation that proves the gameplay concept.

Do not build systems for hypothetical future requirements unless they
are needed by the current prototype.

## Code Guidelines

Prefer focused components with clear responsibilities.

Avoid large all-purpose manager classes.

Separate data from behaviour where practical.

For example:

- MediaItemData - album metadata
- MediaItem - physical object behaviour
- PlayerInteraction - player interaction
- ShelfSlot - placement location
- ShelfSection - shelf/category rules
- ShiftManager - overall prototype completion

Keep systems understandable and easy to change while the game design is
still being prototyped.

## Working With Codex

Before implementing a feature:

1. Inspect the existing implementation.
2. Make the smallest change necessary.
3. Do not introduce unrelated systems.
4. Verify the project still compiles.
5. Summarise files changed and significant design decisions.

When a request is ambiguous, favour prototype simplicity rather than
adding features.