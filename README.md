# Night Shift

A first-person cosy organisation game in a fictional British record shop around 2006, after a catastrophic sale. One persistent tidy-up task, not repeating shifts.

## Current extended Rock / Indie playtest candidate

This iteration addresses the real Unity playtest feedback: empty-looking racks, tiny hard-to-read CD sleeves, a cropped ten-CD carrying list, floating furniture and enormous genre lettering.

| Category | Quantity |
| --- | --- |
| Fictional albums | 248 |
| Independent physical CDs | 1,388 |
| Browsing rack sections | 11 |
| Sloping rows per section | 7 |
| Cover positions per row | 9 |
| Physical CD depth per position | 4 |
| Maximum cases per rack | 252 |

Every CD has its own persistent ID and shares procedural album artwork with copies of the same record. The original 40 albums, 140 physical cases and all existing IDs are preserved, then 208 albums and 1,248 new stock copies are appended.

Other improvements include original 256-pixel cover illustrations with high-contrast artist/title lettering, no overlapping 3D labels, restrained Rock/Indie signage, properly supported tables and bins, subtle shop details and softer lighting. Correctness remains feedback, not a restriction on where you can put a CD.

## Run and validate

Open the Game folder in Unity Editor 6000.5.8f1. Open Assets/Scenes/Prototype.unity, enter Play mode, then use Night Shift > Validate Running Vertical Slice. The Console should confirm 248 albums, 1,388 unique physical IDs, 11 unique shelves and 252 capacity per shelf. Complete VERTICAL_SLICE_QA.md before accepting this milestone.

## Controls

| Input | Behaviour |
| --- | --- |
| WASD and mouse | Walk and look |
| E on a CD | Collect it into your ten-CD hand stack |
| Mouse wheel while carrying | Change the highlighted CD in the two-column carry list |
| E on a rack | Place only the selected carried case |
| Shift+E | Force rack placement when another case is under the crosshair |
| G | Drop only the selected CD with the existing physical handling |
| Mouse wheel while empty-handed at a rack | Flip through the physical copies behind the front CD |
| P and O | Save and reload (F5 and F9 also available in Unity Editor) |
| Escape | Free the cursor |

The carrying panel uses two columns so all ten physical copies can be listed without the bottom items disappearing in a typical Unity Game window. Long names are ellipsised only where necessary; the larger focus panel displays the current album.

## Save preservation

Existing version-2 save files remain compatible. The original physical IDs slice-0001 to slice-0140 and all 11 stable shelf IDs retain their saved positions and order. On first load after this expansion, previously unknown expanded-* copies are added around the saved stock, without a shuffle of your previous work, and written out by autosave. A save from an earlier prototype with version 1 is still unsupported.

Before this major content expansion, BACK UP prototype-save.json and prototype-save.json.bak in Application.persistentDataPath. On Windows this is usually in the user's AppData/LocalLow/DefaultCompany/Game folder.

To deliberately begin with fresh shop chaos, quit Play mode and move both saved JSON files somewhere safe before restarting the game.

## Verification status

The repository contains Unity-free source/data tests, and an in-Editor runtime validator. Source tests cannot establish actual Unity compilation, performance or rendered appearance. This 1,388-case candidate requires a local 15-30 minute player and frame-rate test before the next category is added.

No timed shifts, XP economy or shop-management systems are part of this prototype.

## Improved CD browsing and inspection (follow-up)

Physical CD cases remain their realistic size. The bottom-right focus card now shows a **large version of the actual album cover** alongside the artist, album and genre. While carrying one or more CDs, this card stays on the **selected case**, rather than changing unexpectedly when the crosshair passes over a shelf. Hold **Q** to inspect the album in a much larger temporary view. When pointing at an unheld CD, Q inspects the target without picking it up.

A narrow aim-assist target helps with tiny jewel cases; if the front case is obscured by a browse-bin lip, the player can still select it through the rack's existing non-colliding aim region. A populated rack can now report how many physical cases occupy the browsed column. Empty-handed mouse-wheel flipping, ten-CD carrying, wrong-genre shelving and version-2 saves are unchanged.

The cover in the on-screen UI is taken from the existing **shared 256px texture** per album, corrected for the case mesh's 180-degree UV orientation. The preview does not create new textures for each individual stock copy.

## Crosshair alignment and rack aiming

The player now uses the camera's **exact viewport-centre ray** for CD pickup, rack placement and browsing. A direct pickup respects the closest solid collider, with only 2 cm of fallback aim forgiveness. Rack interactions map the visible case nearest the centre ray, not the misleading entry point of the large invisible placement trigger. Obstacles must be visible rather than allowing interaction with stock hidden behind walls or unrelated furniture.

The crosshair is green when aimed at a CD, pale blue for a rack and white when there is no available interaction. A compact near-centre label names the pickup action so the bottom-right **selected carried album** information is no longer mistaken for the target currently under the crosshair. No changes to case scale, shelves, physical behaviour, or saves.

## Independent four-deep CD positions and seven-row aim (follow-up)

Each rack still has seven rows and nine album-facing positions per row, with up to four **separate physical cases behind each position**. Picking up a CD moves only the cases behind that specific facing forward. The rest of the row and the other six rows stay exactly where they were. Putting a CD into a position places it at the front, pushing only the other cases in that facing backwards (up to four). Scrolling empty-handed cycles just that facing.

This replaces the old flattened arrangement where removing one case reshuffled everything after it. Each physical case now has a fixed rack slot number, stored alongside its existing shelf section and order in backward-compatible version-2 saves. Previously saved version-2 shops are interpreted using their original shelf order, preserving your existing CDs and progress. The in-Editor **Night Shift → Validate Running Vertical Slice** command also checks that no rack has duplicate physical positions or gaps behind a removed case.

Shelf targeting is no longer dependent on the oversized invisible placement trigger. The camera-centre ray compares the actual CD facing positions in all eleven racks, making all seven rows available even if the old trigger doesn't intersect the crosshair. Walls and unrelated furniture remain solid obstructions. No physical case dimensions, carry capacity or scene geometry were changed.
