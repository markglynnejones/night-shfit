# Night Shift: seven-row browsing pass QA

This is a code/content candidate awaiting a Unity 6000.5.8f1 playtest.

## Scene and performance

- [ ] Play enters with no C# errors or missing scripts.
- [ ] Night Shift > Validate Running Vertical Slice reports 248 unique album definitions, 1,388 physical copies and eleven unique rack IDs with capacity 252 each.
- [ ] Record scene startup time and average/lowest frame rate in the Game view.
- [ ] Move along all 11 racks and inspect crowded tiers for movement stutter.
- [ ] Leave running at least two minutes and verify autosave does not visibly freeze the game.

## Seven-row racks / four-deep piles

- [ ] Seven visible sloping CD browsing rows per rack, each wide enough for nine front covers.
- [ ] Up to four independent CDs can occupy each facing, with distinct IDs.
- [ ] When not carrying stock, aim at a stack and use the mouse wheel to bring the next copy to the front. The change persists after P/O.
- [ ] E can pick the front copy; deeper copies remain accessible.
- [ ] Empty slots remain reachable, and E/Shift+E insert only the selected carried CD.
- [ ] Wrong-genre and wrong-alphabetical-order cases remain placeable; no magical auto-sort.
- [ ] There is no clipping through rear panels, divider tabs or neighbouring artwork.
- [ ] The trigger volume does not physically block the player or a picked-up case.

## Carried stack and visual readability

- [ ] Collect ten cases. All ten appear at once in a two-column top-right list.
- [ ] The tenth selected item and the bottom-right selected album information remain entirely visible.
- [ ] Long titles ellipsise cleanly rather than spilling off the panel.
- [ ] Scrolling with a carried stack changes the selected CD, not the rack contents.
- [ ] The album sleeves show distinct original artwork, with readable high-contrast artist/title bands; no stacked-on-top 3D letters.
- [ ] Rock and Indie signs fit inside their burgundy/teal header panels.
- [ ] Separate progress and control panels have no collisions with the carry/album panels in normal and maximised Game views.

## Shop fixtures and atmosphere

- [ ] Central display table has four visible legs and lower storage rather than floating.
- [ ] Low loose-stock bins are supported by solid floor-reaching bases.
- [ ] Lighting is warmer and avoids deep, unreadable shadows.
- [ ] Floor skirting, small sale tags and ceiling fixtures look like a retail interior, not distracting floating polygons.

## Save migration and the real organising loop

- [ ] Back up the old save files BEFORE first testing this major content expansion.
- [ ] Opening a former 140-CD version-2 save retains all 140 original IDs and the player's existing organised/misfiled placements.
- [ ] 1,248 new expanded-* copies appear without resetting the old world.
- [ ] P then O reloads exactly 1,388 physical IDs with no duplicates or missing shelf references.
- [ ] The ten-CD hand stack, selected index and four-deep flip order persist between sessions.
- [ ] A fresh shop has 228 reference copies, 892 wrongly shelved cases, and 268 loose cases.
- [ ] Picking up from a deep row, grouping duplicates and dropping stock still feels like the good original physics.

## Acceptance

Play for 15-30 minutes and report frame-rate issues, interaction frustrations, whether the shop feels stocked without overwhelming the player, legibility, and any clipping. Fix critical bugs before adding DVDs or games. Static GitHub checks do not replace this player test.

## Follow-up: procedural cover orientation

- [ ] Hold a CD and read the artist/title on its *front face*. Both must be upright, not rotated 180 degrees.
- [ ] Verify a similar cover on a browsing rack has upright lettering.
- [ ] Check the album art still aligns with its cover border and the high-contrast title band.
- [ ] Confirm this fix does not rotate the physical case, change interaction, shelving order, carry stack or saved positions.

## Follow-up usability: identifying and browsing CDs

- [ ] Aim at a loose CD with empty hands. Its actual cover, artist, album and genre appear on a readable bottom-right card.
- [ ] Hold **Q** on that CD. A large correctly oriented album cover and artist/title appear in a temporary inspection view; releasing Q closes it without changing the item.
- [ ] Hold Q when carrying a stack: inspecting another case should not change which CD is selected to shelve. The normal focus card should continue to identify the selected carried case.
- [ ] Pick up 10 CDs and wheel between them. The bottom-right cover preview and highlighted inventory entry both follow the selected case; no album text is clipped.
- [ ] Try aiming at small cases in a crowded rack and near a shallow retaining lip. The narrow targeting assist should help collect cases without accidentally targeting behind walls.
- [ ] With hands empty and a populated four-deep rack facing, the inspection card shows the count of physical copies behind the face. Mouse-wheel flipping changes the visible case; inspect it again.
- [ ] Confirm artist/title lettering reads upright on real CD case fronts **and** on the screen-space previews.
- [ ] Check all panels in a short Unity Game view (~1100 × 380) and a maximised Game view. The status, carried stock and album card should not overlap.
- [ ] P/O save/reload preserves carried selection, misfiled cases and flipped rack ordering. The Q inspection panel must not cause autosaves or change any world state.

Unity playtest is required: the source checks do not verify physical aiming, rendered labels or in-game legibility.
