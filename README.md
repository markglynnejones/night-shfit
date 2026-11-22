# Night Shift

A first-person, cosy **physical-media sorting game** set in a fictional British record shop around 2006. One tremendous post-sale mess, one persistent shop, no timed shifts.

## Run the Rock / Indie vertical slice

1. Open the Game folder as a Unity project using **Unity 6000.5.8f1** (Unity 6.5, URP).
2. Open Assets/Scenes/Prototype.unity.
3. Enter **Play** mode. The prototype builds stock, shelving and scenery from original scene templates.
4. Use **WASD + mouse** to move, **E** to pick up/place/drop, **Esc** to free the cursor, **P** to save and **O** to reload (also F5/F9 in the desktop Editor).
5. During Play mode choose **Night Shift > Validate Running Vertical Slice**. The Console should report 40 catalogue entries, 140 unique physical copies and 11 shelving bays.
6. Follow [the extended test](VERTICAL_SLICE_QA.md).

## What is in this build

- 40 fictional Rock/Indie albums: eight saved ScriptableObject prototypes plus 32 deterministic runtime entries.
- 140 physical CDs, with varying quantities per title, sharing album metadata and procedural cover art.
- Eleven capacity-limited shelf sections across back and side walls, signed by genre and artist ranges.
- Approximately 14% reference stock in correct alphabetical order, 11% deliberately misfiled, and the remainder in piles around the shop.
- **Free-form shelving:** wrong genres and wrongly alphabetised stock can be placed.
- Ordered stock is counted for gentle progress feedback, without forcing a specific workflow.
- Save/load/autosave retains each physical copy, its position or shelf and its precise shelf order. Saves use **format v2**, with backup.
- Original procedural sleeve art, sale signage, display bins, checkout, closed shutters and a progress overlay.
- **Scale/readability pass:** jewel cases now measure roughly 17.2 by 13.6 cm, shelf spacing and pile heights match their new size, the player starts at floor level (eye approximately 1.65 m), and the room has calmer warm colours with red Rock / teal Indie signs.
- Looking at or holding a case shows a small artist/album/genre panel at the bottom-right so you can read real-sized stock without oversized 3D labels.

## Persistence / old saves

Version 2 saves intentionally replace the old 21-item demo save schema. **If previous prototype progress matters, back up** the existing prototype-save.json in Unity's Application.persistentDataPath before running this build. Version 1 files are not migrated. Without a valid v2 file the new authored starting state is used.

To restart the slice from its initial disorder, **close Unity Play mode** and move prototype-save.json and prototype-save.json.bak elsewhere before playing again. Keep backups if you wish to restore them.

The scale/readability pass keeps the version 2 save schema. Saved CDs on shelves keep their persistent IDs and order, and loose CDs from the old large-case build are repositioned onto the nearest static surface the first time that save is loaded. Your local save is not reset. If you prefer a fresh start, move your original save files aside while Unity is closed.

## Status and limitations

This branch is a feature-complete **candidate** for the first 30–60-minute playtest. Source changes have been committed, **but local Unity Editor compilation, scene play, save recovery and a timed playtest remain unverified**. The milestone is not accepted until the QA gates pass.

Long-term ambitions (not in this prototype): more genres, DVDs, games, hundreds of distinct titles and eventually 2,000–3,000+ physical items. Do not scale before evaluating this loop.
