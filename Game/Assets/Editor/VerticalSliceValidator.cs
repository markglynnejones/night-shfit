#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Use Night Shift > Validate Running Vertical Slice during Play mode.
// This validates the assembled runtime scene without changing any save data.
public static class VerticalSliceValidator
{
    [MenuItem("Night Shift/Validate Running Vertical Slice")]
    private static void Validate()
    {
        if (!EditorApplication.isPlaying || !PrototypeSortingSetup.HasCompletedSetup)
        {
            Debug.LogWarning("Open Assets/Scenes/Prototype and enter Play mode first.");
            return;
        }

        MediaItem[] stock = Object.FindObjectsByType<MediaItem>(FindObjectsInactive.Exclude);
        ShelfSlot[] shelves = Object.FindObjectsByType<ShelfSlot>(FindObjectsInactive.Exclude);
        HashSet<string> physicalIds = new();
        HashSet<string> catalogueIds = new();
        HashSet<string> shelfIds = new();
        int failures = 0;
        int loose = 0;
        int misfiled = 0;
        int reference = 0;
        int expandedPhysicalIds = 0;
        Dictionary<ShelfSlot, HashSet<int>> occupiedPhysicalSlots = new();
        Dictionary<ShelfSlot, Dictionary<int, int>> depthsByFacing = new();

        foreach (MediaItem item in stock)
        {
            if (item == null || item.AlbumDefinition == null
                || string.IsNullOrWhiteSpace(item.PhysicalItemId)
                || !physicalIds.Add(item.PhysicalItemId))
            {
                Debug.LogError("Bad or duplicate physical stock identity at " + (item == null ? "<null>" : item.name));
                failures++;
                continue;
            }

            catalogueIds.Add(item.AlbumDefinition.CatalogueId);
            if (item.PhysicalItemId.StartsWith("expanded-",
                System.StringComparison.Ordinal))
            {
                expandedPhysicalIds++;
            }
            PhysicalInteractable physical = item.GetComponent<PhysicalInteractable>();
            if (physical == null)
            {
                Debug.LogError("PhysicalInteractable missing on " + item.name);
                failures++;
            }
            else if (physical.CurrentShelfSlot == null)
            {
                loose++;
            }
            else
            {
                ShelfSlot rack = physical.CurrentShelfSlot;
                int position = rack.PhysicalSlotOf(physical);
                if (position < 0 || position >= rack.Capacity)
                {
                    Debug.LogError("Invalid fixed physical slot for "
                        + item.PhysicalItemId + " in " + rack.ShelfSectionId);
                    failures++;
                }
                else
                {
                    if (!occupiedPhysicalSlots.TryGetValue(rack, out HashSet<int> positions))
                    {
                        positions = new HashSet<int>();
                        occupiedPhysicalSlots.Add(rack, positions);
                        depthsByFacing.Add(rack, new Dictionary<int, int>());
                    }

                    if (!positions.Add(position))
                    {
                        Debug.LogError("Two CDs occupy physical slot " + position
                            + " in " + rack.ShelfSectionId);
                        failures++;
                    }

                    int face = position % ShelfSlot.FacesPerLayer;
                    Dictionary<int, int> depths = depthsByFacing[rack];
                    depths.TryGetValue(face, out int occupiedCount);
                    depths[face] = occupiedCount + 1;
                }

                if (!rack.CanAccept(item.AlbumDefinition))
                {
                    misfiled++;
                }
                else
                {
                    reference++;
                }
            }
        }

        int shelved = 0;
        int correctlyOrganised = 0;
        foreach (ShelfSlot shelf in shelves)
        {
            if (shelf.Capacity != 252)
            {
                Debug.LogError("Expected four-deep capacity of 252 for "
                    + shelf.ShelfSectionId + ", found " + shelf.Capacity);
                failures++;
            }
            shelved += shelf.ItemCount;
            correctlyOrganised += shelf.CountCorrectlyOrganisedItems();

            if (string.IsNullOrWhiteSpace(shelf.ShelfSectionId)
                || !shelfIds.Add(shelf.ShelfSectionId))
            {
                Debug.LogError("Missing or duplicate shelf ID: " + shelf.ShelfSectionId);
                failures++;
            }
        }

        // Every occupied facing must be a contiguous front-to-back stack.
        // Removing a case should never leave gaps or shift another facing.
        foreach (KeyValuePair<ShelfSlot, Dictionary<int, int>> rack in depthsByFacing)
        {
            HashSet<int> positions = occupiedPhysicalSlots[rack.Key];
            foreach (KeyValuePair<int, int> column in rack.Value)
            {
                if (column.Value > ShelfSlot.MaxDepth)
                {
                    Debug.LogError("Too many CDs in facing " + column.Key
                        + " of " + rack.Key.ShelfSectionId);
                    failures++;
                }

                for (int depth = 0; depth < column.Value; depth++)
                {
                    int slot = column.Key + depth * ShelfSlot.FacesPerLayer;
                    if (!positions.Contains(slot))
                    {
                        Debug.LogError("Gap within four-deep facing "
                            + column.Key + " in " + rack.Key.ShelfSectionId);
                        failures++;
                    }
                }
            }
        }

        if (stock.Length != 1388 || catalogueIds.Count != 248
            || shelves.Length != 11 || expandedPhysicalIds != 1248)
        {
            Debug.LogError("Expected 1388 CDs / 248 albums / 11 shelves, found "
                + stock.Length + " / " + catalogueIds.Count + " / "
                + shelves.Length + " (expanded copies " + expandedPhysicalIds + ")");
            failures++;
        }

        if (shelved + loose != stock.Length)
        {
            Debug.LogError("Inventory mismatch: loose + shelved does not equal total.");
            failures++;
        }

        if (correctlyOrganised > stock.Length)
        {
            Debug.LogError("Progress calculation exceeds stock count.");
            failures++;
        }

        if (failures == 0)
        {
            Debug.Log("PASS: 248 album definitions, 1388 unique cases, "
                + "11 seven-row, four-deep shelf IDs. "
                + loose + " loose, " + misfiled + " wrong-genre/range cases, "
                + reference + " in the right category/range, "
                + correctlyOrganised + " correctly ordered.");
        }
        else
        {
            Debug.LogError("Vertical slice validation failed: " + failures + " issue(s).");
        }
    }
}
#endif
