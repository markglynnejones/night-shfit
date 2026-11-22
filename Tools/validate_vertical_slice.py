#!/usr/bin/env python3
"""Fast, Unity-free validation of Night Shift vertical-slice source data.

These checks validate data and wiring, not C# compilation, collision physics
or the feel of the game. Those still require a Unity runner/browser build.
"""
from __future__ import annotations

import re
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCRIPTS = ROOT / "Game" / "Assets" / "Scripts"
ALBUMS = ROOT / "Game" / "Assets" / "Data" / "Resources" / "Albums"
SCENE = ROOT / "Game" / "Assets" / "Scenes" / "Prototype.unity"

def require(condition: bool, message: str) -> None:
    if not condition:
        raise AssertionError(message)

def main() -> None:
    setup = (SCRIPTS / "PrototypeSortingSetup.cs").read_text(encoding="utf-8")
    shelf = (SCRIPTS / "ShelfSlot.cs").read_text(encoding="utf-8")
    save = (SCRIPTS / "PrototypeSaveLoadController.cs").read_text(encoding="utf-8")
    scene = SCENE.read_text(encoding="utf-8")
    layout = (SCRIPTS / "PrototypeStoreLayout.cs").read_text(encoding="utf-8")
    player = (SCRIPTS / "PlayerInteraction.cs").read_text(encoding="utf-8")

    # Pull only literal seed constructors out of the two purpose-built blocks.
    album_block = setup.split("AdditionalAlbums =", 1)[1].split("private static readonly ShelfSeed", 1)[0]
    albums = re.findall(
        r'new\("([^"]+)", "([^"]+)", "([^"]+)", "(Rock|Indie)"\)', album_block
    )
    require(len(albums) == 32, f"Expected 32 new albums; found {len(albums)}")

    existing = []
    for path in sorted(ALBUMS.glob("*.asset")):
        text = path.read_text(encoding="utf-8")
        keys = {}
        for key in ("catalogueId", "artistName", "albumTitle", "genre"):
            match = re.search(r"^\s*" + key + r":\s*(.*?)\s*$", text, re.M)
            require(match is not None, f"{path.name} is missing {key}")
            keys[key] = match.group(1)
        existing.append((keys["catalogueId"], keys["artistName"], keys["albumTitle"], keys["genre"]))
    require(len(existing) == 8, f"Expected 8 existing album definitions; found {len(existing)}")
    entries = sorted(albums + existing, key=lambda row: row[0])
    require(len(entries) == 40, f"Expected 40 albums, got {len(entries)}")
    require(len({row[0] for row in entries}) == 40, "Duplicate catalogue IDs")

    shelf_block = setup.split("private static readonly ShelfSeed[] Shelves", 1)[1]
    shelf_block = shelf_block.split("private static readonly PileSpot[] Piles", 1)[0]
    shelves = re.findall(
        r'new\("([^"]+)", "(Rock|Indie)", "([A-Z])", "([A-Z])", '
        r'(-?[\d.]+)f, (-?[\d.]+)f, (-?[\d.]+)f\)', shelf_block
    )
    require(len(shelves) == 11, f"Expected 11 shelf sections; found {len(shelves)}")
    require(len({row[0] for row in shelves}) == 11, "Duplicate shelf IDs")
    for sid, genre, start, end, x, z, yaw in shelves:
        require(start <= end, f"Invalid alphabetical range on {sid}")
        require(-6.85 < float(x) < 6.85 and -4.85 < float(z) < 4.85,
                f"Shelf centre out of room bounds: {sid}")
        require(float(yaw) in (-90.0, 0.0, 90.0), f"Unexpected yaw: {sid}")

    def accepts(shelf_row: tuple[str, ...], album: tuple[str, ...]) -> bool:
        _, genre, start, end, *_ = shelf_row
        artist = album[1]
        if artist.lower().startswith("the "):
            artist = artist[4:]
        return album[3] == genre and start <= artist[0].upper() <= end

    occupancy = Counter()
    total = 0
    reference = misplaced = loose = 0
    for index, album in enumerate(entries):
        copies = 2 + ((index * 7) % 4)
        matching = [s for s in shelves if accepts(s, album)]
        require(matching, f"No home shelf for album {album[0]}")
        for _ in range(copies):
            total += 1
            if total % 7 == 0:
                home = matching[0]
                occupancy[home[0]] += 1
                reference += 1
            elif total % 8 == 0:
                misplaced += 1
                # Mirrors FindIncorrectShelf's modulo/first-capacity strategy.
                destination = next((
                    shelves[( (misplaced - 1) * 3 + offset) % len(shelves)]
                    for offset in range(len(shelves))
                    if not accepts(shelves[((misplaced - 1) * 3 + offset) % len(shelves)], album)
                    and occupancy[shelves[((misplaced - 1) * 3 + offset) % len(shelves)][0]] < 18
                ), None)
                require(destination is not None, f"No misfile shelf space for {album[0]}")
                occupancy[destination[0]] += 1
            else:
                loose += 1
    require(total == 140, f"Expected 140 physical cases, got {total}")
    require(reference == 20 and misplaced == 60 and loose == 60,
            f"Unexpected start split: {reference}/{misplaced}/{loose}")
    require(max(occupancy.values()) <= 18, "Starting shelf is over capacity")

    # Visual proportions are coupled: changing case scale without retuning piles
    # or shelf spacing would leave the room full of floating, oversized gaps.
    require("JewelCaseScale = 0.22f" in setup, "Physical case scale drifted")
    require("cd.transform.localScale = Vector3.one * JewelCaseScale" in setup,
            "New case scale is not applied to each physical instance")
    require("PileLayerSpacing = 0.020f" in setup and "layer * PileLayerSpacing" in setup,
            "Loose stock stacks were not rescaled")
    require("slot.ConfigureJewelCaseSpacing();" in setup,
            "Shelf spacing was not updated")
    require("PrototypeStoreLayout.StyleShelf(shelf, seed.Genre);" in setup,
            "Shelves are not restyled")
    require("public static void StyleShelf" in layout
            and "slotVisual.enabled = false;" in layout,
            "Oversized visible shelf slot is still in the scene")
    require("focusedMedia" in player and "album.ArtistName" in player,
            "Readable catalogue interaction prompt is missing")
    require("m_LocalPosition: {x: 0, y: 0, z: -2}" in scene,
            "Player starts too high for a 1.65-metre eye height")
    require("CurrentVisualScaleSignature" in save
            and "UpgradeLegacyLooseCaseHeights" in save,
            "Old saves may not reposition their oversized piles")

    require("private const int SaveFormatVersion = 2;" in save, "Incorrect save version")
    require("shelfOrder = shelfOrder" in save, "Shelved order is not persisted")
    require("RestoreShelvedItems(" in save and "RestoreLooseItems(" in save,
            "Save load lacks stock restoration")
    require("TryGetKnownAlbum" in shelf, "Shelf placement must allow the wrong genre")
    require("CoversPerTier = 6" in shelf and "TierCount = 3" in shelf,
            "CD racks no longer use six-by-three browsing tiers")
    require("new(28f, 0f, 0f)" in shelf, "CD faces no longer tilt towards shoppers")
    require('CreateRackPart(shelf, "Browsing Deck "' in layout,
            "CD browsing deck geometry was not built")
    require("placementVolume.isTrigger = true" in layout,
            "Browse tray placement volume must not collide with players")
    require("QueryTriggerInteraction.Collide" in player,
            "Shelving raycast must be able to see tray triggers")
    require("slot.transform.localScale = new Vector3(2.02f, 0.57f, 0.75f)" in layout,
            "The bin placement area is missing")
    require("new Vector3(-0.35f, 1.38f, 0.268f)" in
            (SCRIPTS / "ShelfSectionLabel.cs").read_text(encoding="utf-8"),
            "Flip-bin genre sign is no longer on the upright header")
    require("WouldRemainLogicallyOrdered" not in shelf,
            "Incorrectly ordered stock is still being blocked")
    require("CountCorrectlyOrganisedItems()" in shelf, "Progress feedback missing")

    for basename in ("PrototypeSortingSetup", "PrototypeStoreLayout"):
        meta = (SCRIPTS / f"{basename}.cs.meta").read_text(encoding="utf-8")
        guid = re.search(r"^guid:\s*(\w+)", meta, re.M).group(1)
        require(guid in scene, f"Prototype scene does not reference {basename}")
    build_settings = (ROOT / "Game" / "ProjectSettings" / "EditorBuildSettings.asset").read_text(encoding="utf-8")
    require("Assets/Scenes/Prototype.unity" in build_settings and "enabled: 1" in build_settings,
            "Prototype not included in build scenes")

    print(f"PASS: {len(entries)} catalogue titles, {total} stock copies, "
          f"{len(shelves)} valid bays; {reference} reference, "
          f"{misplaced} misfiled, {loose} loose.")
    print("PASS: Save/load contracts, scene wiring and permissive shelving signatures.")
    print("PASS: CD proportions, pile/shelf geometry, room contrast, readable inspect prompt and save upgrade contracts.")
    print("PASS: Three-tier face-out racks, marker/trigger setup, dividers and 80 initially shelved cases.")
    print("Note: source/data checks only. Unity compilation and gameplay remain unverified.")

if __name__ == "__main__":
    main()
