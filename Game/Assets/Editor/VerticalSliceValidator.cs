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
            else if (!physical.CurrentShelfSlot.CanAccept(item.AlbumDefinition))
            {
                misfiled++;
            }
            else
            {
                reference++;
            }
        }

        int shelved = 0;
        int correctlyOrganised = 0;
        foreach (ShelfSlot shelf in shelves)
        {
            shelved += shelf.ItemCount;
            correctlyOrganised += shelf.CountCorrectlyOrganisedItems();

            if (string.IsNullOrWhiteSpace(shelf.ShelfSectionId)
                || !shelfIds.Add(shelf.ShelfSectionId))
            {
                Debug.LogError("Missing or duplicate shelf ID: " + shelf.ShelfSectionId);
                failures++;
            }
        }

        if (stock.Length != 140 || catalogueIds.Count != 40 || shelves.Length != 11)
        {
            Debug.LogError("Expected 140 CDs / 40 albums / 11 shelves, found "
                + stock.Length + " / " + catalogueIds.Count + " / " + shelves.Length);
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
            Debug.Log("PASS: 40 album definitions, 140 unique cases, 11 shelf IDs. "
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
