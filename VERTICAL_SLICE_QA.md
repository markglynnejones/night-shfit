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

## Precise aiming acceptance

- [ ] Point the centre dot at different CDs in a densely filled rack. The **green** dot and nearby artist/action prompt should correspond to the precise CD under the dot.
- [ ] With a CD selected in hand, point at an empty rack space. The dot should turn **blue** and E should place in the row being aimed at, not one above or below.
- [ ] When empty-handed, wheel-flip a four-deep row and check only the intended stack changes.
- [ ] Try near the edge of a small CD and near retaining lips. The narrow 2 cm targeting assist should help without selecting a neighbouring CD.
- [ ] Look at a rack partly hidden behind a table, a wall and another object. There should be no pickup or placement through that obstruction.
- [ ] With several carried CDs, the bottom-right card still describes the selected carried CD while the crosshair prompt describes what pressing E will collect or shelve.
- [ ] Test a normal Game window and maximised Game view, and check P/O save/reload does not change any shelving or inventory state.

These are real Unity interaction tests; passing GitHub source checks alone does not verify the feel.

## Follow-up: independent four-deep positions and every-row aim

- [ ] Before beginning, back up `prototype-save.json` and its `.bak` file in the Unity persistent-data folder.
- [ ] Stand at each of the **seven rows**, left, middle and right. Check the crosshair target consistently identifies the intended CD/slot. It must not depend on the invisible old trigger's entrance position.
- [ ] With hands empty, pick up the visible front CD in a four-deep position. **Only that facing's next case comes forward**. Adjacent facings, shelves and rows must remain unmoved.
- [ ] Pick up a case from a deeper facing after flipping through the stack. Remaining cases behind it advance in the same facing, without leaving gaps.
- [ ] Aim at another facing, put down a CD while carrying one, and check that **only the chosen facing** pushes back its existing CDs. A position with four CDs must refuse a fifth and leave the held case in the hand stack.
- [ ] Empty-handed scroll through a four-deep position. The stack should rotate without any cases from other positions moving.
- [ ] Repeat these checks across Rock and Indie sections, including the highest and lowest rows.
- [ ] Save with **P**, reload with **O**, and check the exact same CDs are in the same front-to-back position for each facing. The carrying stack must also survive.
- [ ] Load an existing pre-change version-2 save and check that all previously arranged CDs remain and new stock is retained. The first load upgrades shelf positions without resetting progress.
- [ ] While playing, select **Night Shift → Validate Running Vertical Slice**. It should find no duplicate occupied slots or holes in any four-deep facing.
- [ ] Make sure a CD in another shelf or behind solid furniture cannot be collected or shelved through.
- [ ] The colour/action prompt should match the CD, shelf or empty space directly under the centre dot.

GitHub tests verify structural rules, but only these Unity tests can confirm the row-by-row feel and saved game behaviour.

## Targeting regression: row 4 and parts of row 3

- [ ] Back up your `prototype-save.json` and `.bak` before testing this revision.
- [ ] In Play mode, aim at the **third and fourth rows** from the bottom on three different racks. The near-centre prompt must show `ROW 3` or `ROW 4`, never the neighbouring row.
- [ ] On each row, test columns 1, 5 and 9. Check the displayed row/column and `n/4 CDS` count.
- [ ] Pick up a front CD using **E** with empty hands. Only the deeper copies in that exact position should move forward; other rows/columns stay fixed.
- [ ] With a CD already carried, aim at another stocked position. **E collects another CD** while your stack has space; **Shift+E shelves the selected CD**. Both target the row/column shown by the prompt.
- [ ] Check a facing reading `4/4 CDS`: shelving must be refused with a visible full message. Pick a case up first, then shelve.
- [ ] Empty-handed wheel flipping moves only the targeted four-deep position.
- [ ] In Unity, choose **Night Shift > Validate Running Vertical Slice**. It now tests 7 rows × 3 columns × 3 vertical aim positions on each of the 11 racks, besides checking stock identities and physical-slot gaps.
- [ ] Repeat from both normal standing distance and close to the rack, then press P and O to confirm exact placement persists.

This is a sightline and input repair, not a change to case sizes, stock counts, or saved shelf positions. Confirm the visual behaviour locally; GitHub source tests are not a Unity compilation run.
