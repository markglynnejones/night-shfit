# Night Shift - Implementation Plan

## Purpose

This document defines the order in which Night Shift should be built.

The goal is to avoid feature creep, avoid prematurely scaling content, and make each development step answer a useful design or technical question.

## Working Model

### Design work - us

Game direction, scope, sorting rules, content tone, player experience, playtest interpretation, and milestone decisions should be made deliberately before implementation.

### Implementation - Codex

Codex should implement bounded tasks against the design documents, preserve working systems, verify compilation, save Unity scenes/assets, and summarise architectural changes.

Codex should not independently broaden the game's scope.

### Source of truth - Git

Working milestones should be committed and pushed before significant new work begins.

## Current State

The project has already proved several important foundations:

- Unity 6.5 / URP project
- First-person movement
- Mouse look
- Physical CD interaction
- Pickup and drop
- Readable prototype covers
- Shelf validation
- Shared album definitions
- Multiple physical copies of one album
- Dense shelving experiments

The next work should build from these foundations rather than replacing them unnecessarily.

---

# Phase 0 - Rebaseline the Project

## Goal

Make the repository and code reflect the actual game direction: one enormous persistent tidy-up rather than repeating shifts.

## Work

- Update `AGENTS.md`.
- Update `PROTOTYPE_SCOPE.md`.
- Add `GAME_VISION.md`.
- Add this implementation plan.
- Add `DECISIONS.md`.
- Inspect code for any accidental shift, clock-out, or short-session architecture.
- Remove or disable those concepts without disturbing working interaction/sorting systems.

## Gate

The current CD interaction, duplicate stock, and shelf validation still work after cleanup.

---

# Phase 1 - Define and Prove Sorting Rules

## Design question

What exactly counts as correctly organised stock?

This needs to be decided before scaling the catalogue.

Likely hierarchy for music:

`Department -> Genre -> Artist -> Album -> Duplicate copies`

The exact handling of albums by the same artist, compilations, charts, new releases, and promotional displays should be designed deliberately rather than guessed in code.

## Implementation work

- Replace simplistic one-letter test logic where necessary with explicit section/sorting rules.
- Allow several artists/titles to coexist within one section.
- Validate correct relative organisation where appropriate.
- Keep duplicates grouped naturally.
- Preserve player freedom to remove and replace stock.

## Gate

A shelf containing several artists, albums, and duplicates can correctly determine whether its stock is organised.

---

# Phase 2 - Persistent World and Save/Load

## Goal

Prove that the store can function as one task spanning many play sessions.

## Required persistence

At minimum, save and restore:

- Stable physical item ID
- Catalogue definition reference
- World position/rotation when loose
- Shelf/section placement when shelved
- Correct/incorrect organisation state where needed
- Player position
- Relevant persistent store state

Avoid saving data that can be derived reliably from other saved state.

## Work

- Design a versioned save format.
- Implement manual save/load for testing.
- Add safe autosave checkpoints later if useful.
- Ensure duplicate physical copies remain distinguishable.
- Test interrupted organisation and recovery.

## Gate

Take a 30-50 CD mess, organise approximately half, save/quit, reopen the project/game, and continue with the store exactly as it was left.

---

# Phase 3 - First Proper Vertical Slice

## Goal

Build one convincing Rock/Indie area large enough to judge the actual game rather than the technology.

## Target content

Approximately:

- 30-50 unique fictional albums
- 100-150 physical CD copies
- Several proper shelving bays
- Multiple duplicates of popular titles
- A minority of correctly placed reference stock
- Catastrophic but believable disorder

## Starting mess

Include different forms of disorder:

- Wrong shelf
- Wrong alphabetical position
- Mixed piles
- Floor items
- Display-table items
- Checkout-area items
- Baskets
- Scattered duplicates

Do not simply randomise every object's transform.

## First-pass presentation

- More believable shelving proportions
- First real pass at cover art
- Stronger signage hierarchy
- Basic environmental lighting
- Early atmosphere/audio where useful

## Gate

A player can spend 30-60 minutes organising this area and still find the loop satisfying.

If the answer is no, improve the interaction/design before scaling content.

---

# Phase 4 - Content Pipeline

## Goal

Make adding hundreds of fictional titles and thousands of physical copies practical.

## Requirement

Do not hand-create hundreds of independent Unity GameObjects or duplicate metadata assets manually.

## Work

Create an editor/content workflow capable of representing catalogue data such as:

- Stable catalogue ID
- Artist/title
- Media type
- Genre/category
- Sort metadata
- Stock quantity
- Artwork reference

The source may be ScriptableObjects, CSV, JSON, an editor tool, or a combination. Choose based on simplicity, editability, version control, and scale.

A catalogue entry with a stock quantity should be capable of producing many physical copies that all reference shared catalogue data/artwork.

## Gate

Adding a new album and several physical copies should take seconds/minutes, not repetitive manual Unity setup.

---

# Phase 5 - Scale and Performance Tests

## Goal

Prove the architecture progressively before targeting thousands of objects.

## Test stages

Roughly:

1. 250 physical items
2. 500 physical items
3. 1,000 physical items

Do not optimise based purely on assumptions. Profile actual bottlenecks.

## Likely considerations

- Shelved objects should not require active physics simulation.
- Settled objects should avoid unnecessary per-frame work.
- Colliders should remain simple.
- Duplicate artwork/materials should share resources.
- Save data should stay compact.
- Interaction raycasts should remain reliable in dense shelves.

## Gate

A densely populated music area remains responsive and playable on the target development machine without obviously wasteful architecture.

---

# Phase 6 - Establish Store Identity

## Goal

Turn the proven greybox into something recognisably Night Shift.

## Work

- Finalise broad store layout.
- Build believable mid-2000s UK retail fixtures.
- Define colour/material language.
- Improve lighting and carpet/walls.
- Add tills, shutters, listening stations, posters, displays, security cases, signage, and stock furniture.
- Establish the fictional retailer's brand.
- Establish visual language for fictional albums/media.
- Add restrained environmental sound and original era-inspired music.

## Gate

A screenshot should communicate "British physical-media megastore circa 2006" without relying on real retailer branding.

---

# Phase 7 - Complete the Music Department

## Goal

Make music alone feel like a substantial version of the game.

## Possible scale

- Hundreds of unique music catalogue entries
- Approximately 1,000+ physical CDs
- Multiple genres
- Duplicates based on plausible popularity/stocking
- Full catastrophic starting-state layout

## Work

- Expand catalogue and art pipeline.
- Finalise music sorting rules.
- Hand-author/generate believable disorder using controlled rules.
- Improve organisation feedback.
- Continue save/performance validation at full music scale.

## Gate

The music department alone delivers the intended long-form organisation experience.

---

# Phase 8 - Add Film/TV and Games

Only begin this phase once music works at scale.

## Film/TV

Add DVDs/box sets with their own physical proportions and sorting rules.

## Games

Add fictional era-appropriate game platforms, case formats, and platform/title organisation.

## Architecture rule

Generalise existing catalogue/media abstractions only when the real requirements of the second/third media type are known.

Do not prematurely redesign the current system around imagined future needs.

## Gate

Different media types feel visually and organisationally distinct while sharing sensible common systems.

---

# Phase 9 - Full Store, Quality of Life, and Polish

Potential work includes:

- Full-store content population
- Organisation/progress feedback
- Accessibility
- Controller support
- Settings
- Robust save recovery
- Audio polish
- Achievements if appropriate
- Steam integration
- Performance passes
- Onboarding
- Quality-of-life mechanics only where playtesting demonstrates genuine need

Do not add progression abilities merely because comparable games have them. Add convenience mechanics only if large-scale playtesting shows that the core interaction becomes tedious without them.

---

# Milestone Workflow

For every meaningful milestone:

1. Decide the design goal before coding.
2. Update design docs if the decision is durable.
3. Start from a clean committed working state.
4. Give Codex one bounded implementation task.
5. Do not append unrelated features during the task.
6. Codex saves all Unity scenes/assets and verifies compilation.
7. Manually playtest in Unity.
8. Discuss what feels good/bad before deciding the next task.
9. Commit and push a working checkpoint.

## Codex task footer

Tasks that modify Unity scenes should end with:

> Before finishing:
> - Save all modified Unity scenes and assets.
> - Verify the project compiles without errors.
> - Confirm scene changes are saved to disk.
> - Do not add unrelated features.
> - Summarise files changed and any architectural decisions made.

## Immediate Next Engineering Task

After this documentation rebaseline, Codex should inspect the existing project for shift/clock-out/completion concepts left over from the earlier prototype direction.

Remove or disable only those concepts while preserving:

- Player movement
- Pickup/drop
- Media definitions
- Duplicate physical stock
- Shelf validation
- Dense shelf behaviour

After that cleanup, stop and design the exact music shelving/sorting rules before adding more content.