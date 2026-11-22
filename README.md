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
