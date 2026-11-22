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
    physical = (SCRIPTS / "PhysicalInteractable.cs").read_text(encoding="utf-8")
    hud = (SCRIPTS / "PrototypeOrganisationHud.cs").read_text(encoding="utf-8")
    case_art = (SCRIPTS / "AlbumCoverArt.cs").read_text(encoding="utf-8")
    case_labels = (SCRIPTS / "AlbumCaseLabel.cs").read_text(encoding="utf-8")
    validator = (ROOT / "Game" / "Assets" / "Editor" / "VerticalSliceValidator.cs").read_text(encoding="utf-8")

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
            elif total % 2 == 0:
                misplaced += 1
                # Mirrors FindIncorrectShelf's modulo/first-capacity strategy.
                destination = next((
                    shelves[( (misplaced - 1) * 3 + offset) % len(shelves)]
                    for offset in range(len(shelves))
                    if not accepts(shelves[((misplaced - 1) * 3 + offset) % len(shelves)], album)
                    and occupancy[shelves[((misplaced - 1) * 3 + offset) % len(shelves)][0]] < 252
                ), None)
                require(destination is not None, f"No misfile shelf space for {album[0]}")
                occupancy[destination[0]] += 1
            else:
                loose += 1
    require(total == 140, f"Expected 140 physical cases, got {total}")
    require(reference == 20 and misplaced == 60 and loose == 60,
            f"Unexpected start split: {reference}/{misplaced}/{loose}")
    require(max(occupancy.values()) <= 252, "Original slice over capacity")

    # The expansion deliberately happens AFTER the original 40/140 are seeded:
    # old physical IDs slice-0001..0140 must still refer to identical copies.
    def word_array(name: str) -> list[str]:
        match = re.search(
            r"Expansion" + name + r"\s*=\s*\{(.*?)\};", setup, re.S
        )
        require(match is not None, "Expansion catalogue missing " + name)
        return re.findall(r'"([^"]+)"', match.group(1))

    prefixes = word_array("ArtistPrefixes")
    endings = word_array("ArtistEndings")
    adjectives = word_array("AlbumAdjectives")
    nouns = word_array("AlbumNouns")
    require(len(prefixes) == 26 and len(endings) == 2,
            "Expanded artist catalogue should contain two artists per letter")
    require(len(adjectives) == 16 and len(nouns) == 16,
            "Expanded album naming vocabulary changed unexpectedly")
    require({prefix[0] for prefix in prefixes} == set("ABCDEFGHIJKLMNOPQRSTUVWXYZ"),
            "Expanded catalogue needs all artist initials A-Z")

    expanded = []
    for p, prefix in enumerate(prefixes):
        for e, ending in enumerate(endings):
            for variant in range(4):
                adj = adjectives[(p * 3 + e * 5 + variant * 7) % len(adjectives)]
                noun = nouns[(p * 5 + e * 11 + variant * 9) % len(nouns)]
                title = adj + " " + noun
                artist = prefix + " " + ending
                genre = "Rock" if (p + e + variant) % 2 == 0 else "Indie"
                identity = f"exp-release-{p:02d}-{e}-{variant}"
                expanded.append((identity, artist, title, genre))
    require(len(expanded) == 208 and len(set(x[0] for x in expanded)) == 208,
            "Expanded catalogue should contain 208 unique albums")
    require(len(entries) + len(expanded) == 248,
            "Expected exactly 248 total fictional albums")

    extra_reference = extra_misfiled = extra_loose = ordinal = 0
    for album in expanded:
        matches = [shelf for shelf in shelves if accepts(shelf, album)]
        require(matches, f"Expanded album missing a home: {album}")
        for _ in range(6):
            ordinal += 1
            if ordinal % 6 == 0:
                extra_loose += 1
            elif ordinal % 5 == 0:
                home = matches[0]
                require(occupancy[home[0]] < 252,
                        f"Full home shelf {home[0]}")
                occupancy[home[0]] += 1
                extra_reference += 1
            else:
                target = next((
                    shelves[(ordinal * 3 + offset) % len(shelves)]
                    for offset in range(len(shelves))
                    if not accepts(shelves[(ordinal * 3 + offset) % len(shelves)], album)
                    and occupancy[shelves[(ordinal * 3 + offset) % len(shelves)][0]] < 252
                ), None)
                require(target is not None, "Expanded misfiled stock has no room")
                occupancy[target[0]] += 1
                extra_misfiled += 1

    require(ordinal == 1248 and
            (extra_reference, extra_misfiled, extra_loose) == (208, 832, 208),
            "Unexpected extended stock distribution")
    require(total + ordinal == 1388, "Expected 1388 physical copies")
    require(max(occupancy.values()) <= 252, "Expanded shelf over capacity")
    require(sum(occupancy.values()) == 1120,
            "Expected 1120 starting shelved copies in a fresh game")
    require("RestoreMissingExpansionStock(" in setup
            and "RestoreMissingExpansionStock(" in save,
            "Old saves must gain new stock without resetting original CDs")
    require("string id = \"expanded-\"" in setup,
            "New stock must have a separate persistent ID range")
    require("expandedStockPendingSave" in save,
            "Newly migrated stock must be saved automatically")


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
            and "visual.enabled = false;" in layout,
            "Oversized visible shelf slot is still in the scene")
    require("focusedMedia" in player and "album.ArtistName" in player,
            "Readable catalogue interaction prompt is missing")
    require("m_LocalPosition: {x: 0, y: 0, z: -2}" in scene,
            "Player starts too high for a 1.65-metre eye height")
    require("CurrentVisualScaleSignature" in save
            and "UpgradeLegacyLooseCaseHeights" in save,
            "Old saves may not reposition their oversized piles")

    # Ten-case carry stack: controls, selection, physical presentation and save
    # identity must stay connected. These are source invariants, not Play tests.
    require("public const int MaxCarryItems = 10;" in player,
            "The carry capacity is not ten CDs")
    require("mouse.scroll.ReadValue().y" in player and "SelectRelative(" in player,
            "The mouse wheel must cycle through carried cases")
    require("AddToStack(" in player and "TryPlaceSelected()" in player
            and "DropSelected()" in player,
            "Collect/place/drop selected CD actions are missing")
    require("keyboard.gKey.wasPressedThisFrame" in player,
            "G must drop the selected physical CD")
    require("keyboard.leftShiftKey.isPressed" in player,
            "Shift+E must allow shelving while aiming at another case")
    require("carriedItems.RemoveAt(selectedIndex)" in player,
            "Placing a CD must remove only that item from the carry list")
    require("item.SetHeldPresentation(" in player and
            "public void SetHeldPresentation(" in physical,
            "The CD stack has no physical fanned presentation")
    require("GetCarriedPhysicalIds()" in player and
            "RestoreCarriedStock(" in player,
            "Carry stack snapshot/restore methods are missing")
    require("saveData.carriedPhysicalIds = carryingPlayers[0].GetCarriedPhysicalIds();" in save
            and "saveData.selectedCarriedIndex = carryingPlayers[0].SelectedCarriedIndex;" in save,
            "Carried ID order/selection are not captured in the saved data")
    require("RestoreCarriedItems(saveData.carriedPhysicalIds," in save
            and "saveData.carriedPhysicalIds ??= new List<string>();" in save,
            "Carried selection is not restored or old saves are incompatible")
    require("Wheel browse" in hud and "Q inspect" in hud
            and "G drop" in hud,
            "Controls help does not describe browsing, inspection and carry actions")
    require("private const int SaveFormatVersion = 2;" in save,
            "Do not break existing v2 saves to add the carry stack")

    require("private const int SaveFormatVersion = 2;" in save, "Incorrect save version")
    require("shelfOrder = shelfOrder" in save, "Shelved order is not persisted")
    require("RestoreShelvedItems(" in save and "RestoreLooseItems(" in save,
            "Save load lacks stock restoration")
    require("TryGetKnownAlbum" in shelf, "Shelf placement must allow the wrong genre")
    require("CoversPerTier = 9" in shelf
            and "TierCount = 7" in shelf and "MaxDepth = 4" in shelf,
            "CD racks must have seven nine-wide rows and four-deep physical stacks")
    require("FacesPerLayer = CoversPerTier * TierCount" in shelf,
            "Physical column/depth slot count is inconsistent")
    require("maxItems = FacesPerLayer * MaxDepth;" in shelf,
            "Browse racks do not use 252 independent physical item positions")
    require("FlipCoverAt(" in shelf and "rack.FlipCoverAt(" in player,
            "The player cannot flip through the four-deep browse stacks")
    require("new(28f, 0f, 0f)" in shelf, "CD faces no longer tilt towards shoppers")
    require('CreateRackPart(shelf, "Browsing Deck "' in layout,
            "CD browsing deck geometry was not built")
    require("volume.isTrigger = true" in layout,
            "Browse tray placement volume must not collide with players")
    require("QueryTriggerInteraction.Collide" in player,
            "Shelving raycast must be able to see tray triggers")
    require("slot.transform.localScale = new Vector3(2.01f, 0.91f, 1.17f)" in layout,
            "The bin placement area is missing")
    require("new Vector3(-0.26f, 1.66f, 0.375f)" in
            (SCRIPTS / "ShelfSectionLabel.cs").read_text(encoding="utf-8"),
            "Flip-bin genre sign is no longer on the upright header")
    require("WouldRemainLogicallyOrdered" not in shelf,
            "Incorrectly ordered stock is still being blocked")
    require("CountCorrectlyOrganisedItems()" in shelf, "Progress feedback missing")
    require("List<AlbumDefinition> tails = new();" in shelf,
            "Progress calculation should remain O(n log n) at store-scale")
    require("FinishDisplayFurniture()" in layout and "CreateShopAccents()" in layout,
            "Store furniture and atmosphere pass is missing")
    require('CreateCube("Retail Shop Ceiling"' in layout
            and "ShadowCastingMode.Off" in layout,
            "The shop ceiling must close the old open-sky greybox without blocking light")
    require('CreateCube("Display Table Lower Shelf"' in layout
            and 'CreateCube("Left Browser Bin Base"' in layout,
            "The original floating furniture still lacks physical supports")
    require("CreateRackPart(shelf, \"Browsing Deck \"" in layout,
            "All seven retail browse decks must be constructed")
    require("TextureFormat.RGBA32" in case_art
            and "private static readonly Dictionary<char, byte[]> Font" in case_art
            and "DrawLine(pixels, artistLines" in case_art,
            "Fictional album covers must include high-contrast legible typography")
    require("RotateForCaseFrontUV(pixels);" in case_art
            and "private static void RotateForCaseFrontUV(Color32[] pixels)" in case_art
            and "pixels[i] = pixels[opposite];" in case_art,
            "Case-front bitmap must be turned 180 degrees to face the player upright")
    require("four tiny TextMesh children" in case_labels,
            "CD cases should not create thousands of overlapping 3D labels")
    require("int columns = Screen.width >= 540 ? 2 : 1;" in player
            and "FitText(" in player,
            "Ten-carried-CD panel must wrap to two short columns and avoid clipping")
    # Inspect/browse usability pass, without changing world inventory or save format.
    require("public Texture2D CoverTexture { get; private set; }" in case_art
            and "CoverTexture = sleeve.Texture;" in case_art,
            "A CD must expose its existing shared cover texture for inspection")
    require("public bool TryGetFrontCaseAt(" in shelf,
            "A rack must expose its front case and count of copies behind it")
    require("Physics.SphereCastNonAlloc(ray, 0.045f, hits," in player,
            "Small real-sized CDs need a narrow forgiving aim fallback")
    require("TryGetFrontCaseAt(worldPoint, out PhysicalInteractable front" in player,
            "Hidden rack lips must not prevent front-case selection")
    require("DrawAlbumCard(focusedMedia);" in player
            and "DrawLargeInspection(inspectedMedia);" in player,
            "The selected CD must have an album card and a full inspection view")
    require("keyboard.qKey.isPressed" in player
            and "GUIUtility.RotateAroundPivot(180f, bounds.center);" in player,
            "Holding Q must show cover art in the correct screen-space orientation")
    require("PhysicalInteractable shown = selected != null ? selected : lookedAt;" in player,
            "Selected album details must remain stable when aiming at other stock")
    require("Q inspect" in hud,
            "The contextual inspection key must be documented in the HUD")

    require("stock.Length != 1388" in validator and "shelf.Capacity != 252" in validator,
            "Unity Editor validator must check the expanded shop")

    for basename in ("PrototypeSortingSetup", "PrototypeStoreLayout"):
        meta = (SCRIPTS / f"{basename}.cs.meta").read_text(encoding="utf-8")
        guid = re.search(r"^guid:\s*(\w+)", meta, re.M).group(1)
        require(guid in scene, f"Prototype scene does not reference {basename}")
    build_settings = (ROOT / "Game" / "ProjectSettings" / "EditorBuildSettings.asset").read_text(encoding="utf-8")
    require("Assets/Scenes/Prototype.unity" in build_settings and "enabled: 1" in build_settings,
            "Prototype not included in build scenes")

    print(f"PASS: {len(entries) + len(expanded)} catalogue titles, "
          f"{total + ordinal} stock copies, {len(shelves)} shelf bays.")
    print(f"PASS: legacy 140 physical IDs retained; "
          f"{reference + extra_reference} reference, "
          f"{misplaced + extra_misfiled} misfiled, "
          f"{loose + extra_loose} loose. Max bay: {max(occupancy.values())}/252.")
    print("PASS: Save/load contracts, scene wiring and permissive shelving signatures.")
    print("PASS: CD proportions, pile/shelf geometry, room contrast, readable inspect prompt and save upgrade contracts.")
    print("PASS: Seven-row, four-deep racks, 1120 shelved cases, nonblocking placement targets.")
    print("PASS: Ten-CD two-column UI, wheel selection and in-rack flipping, legible art, furniture and save migration.")
    print("PASS: Shared cover preview, Q inspect, rack front/copy count and small-CD aim assist.")
    print("Note: source/data checks only. Unity compilation and gameplay remain unverified.")

if __name__ == "__main__":
    main()
