using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// A ten-case hand stack. Every case remains a separate physical item with its own ID.
// Scrolling changes only the selected case, never the underlying shelf order.
public sealed class PlayerInteraction : MonoBehaviour
{
    public const int MaxCarryItems = 10;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint = null;
    [SerializeField] private float interactionRange = 3.5f;
    [SerializeField] private LayerMask interactionLayers = Physics.DefaultRaycastLayers;

    private readonly List<PhysicalInteractable> carriedItems = new();
    private int selectedIndex = -1;
    private readonly RaycastHit[] hits = new RaycastHit[64];
    private ShelfSlot[] shelfSlots = new ShelfSlot[0];
    private MediaItem focusedMedia;

    private GUIStyle albumArtistStyle;
    private GUIStyle albumTitleStyle;
    private GUIStyle albumDetailStyle;
    private GUIStyle stackEntryStyle;
    private GUIStyle stackSelectedStyle;

    public int CarriedCount => carriedItems.Count;
    public int SelectedCarriedIndex => selectedIndex;

    private PhysicalInteractable SelectedItem =>
        selectedIndex >= 0 && selectedIndex < carriedItems.Count
            ? carriedItems[selectedIndex] : null;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (holdPoint == null && playerCamera != null)
        {
            GameObject holdPointObject = new("Interaction Hold Point");
            holdPoint = holdPointObject.transform;
            holdPoint.SetParent(playerCamera.transform, false);
            holdPoint.localPosition = new Vector3(0f, -0.12f, 0.48f);
            holdPoint.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (carriedItems.Count > 1 && mouse != null
            && Cursor.lockState == CursorLockMode.Locked)
        {
            float wheel = mouse.scroll.ReadValue().y;
            if (wheel > 0.01f)
            {
                SelectRelative(-1);
            }
            else if (wheel < -0.01f)
            {
                SelectRelative(1);
            }
        }

        if (carriedItems.Count > 0)
        {
            RefreshPlacementHints();
        }

        PhysicalInteractable lookedAt = FindLookedAtInteractable();
        PhysicalInteractable selected = SelectedItem;
        PhysicalInteractable shown = lookedAt != null && carriedItems.Count < MaxCarryItems
            ? lookedAt : selected != null ? selected : lookedAt;
        focusedMedia = shown != null ? shown.GetComponent<MediaItem>() : null;

        if (keyboard == null || Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        // G always drops just the selected CD. Dropped stock uses the original
        // Rigidbody physics, as in the first playable prototype.
        if (keyboard.gKey.wasPressedThisFrame && selected != null)
        {
            DropSelected();
            return;
        }

        if (!keyboard.eKey.wasPressedThisFrame)
        {
            return;
        }

        // Shift+E explicitly shelves the selected case even when the crosshair
        // also intersects a CD already in that rack.
        bool forceShelf = keyboard.leftShiftKey.isPressed
            || keyboard.rightShiftKey.isPressed;
        if (forceShelf && selected != null)
        {
            TryPlaceSelected();
            return;
        }

        if (lookedAt != null && carriedItems.Count < MaxCarryItems)
        {
            AddToStack(lookedAt);
            return;
        }

        if (selected != null)
        {
            TryPlaceSelected();
        }
    }

    private void SelectRelative(int change)
    {
        selectedIndex = (selectedIndex + change + carriedItems.Count) % carriedItems.Count;
        ReflowStack();
        PrototypeSaveLoadController.NotifyPersistentStateChanged();
    }

    private void AddToStack(PhysicalInteractable item)
    {
        if (item == null || item.IsHeld || holdPoint == null
            || carriedItems.Contains(item) || carriedItems.Count >= MaxCarryItems)
        {
            return;
        }

        item.PickUp(holdPoint);
        carriedItems.Add(item);
        selectedIndex = carriedItems.Count - 1;
        if (shelfSlots.Length == 0)
        {
            shelfSlots = Object.FindObjectsByType<ShelfSlot>(FindObjectsInactive.Exclude);
        }

        ReflowStack();
        PrototypeSaveLoadController.NotifyPersistentStateChanged();
    }

    private void ReflowStack()
    {
        // Cases are physically fanned in the player's hand. The selected front
        // cover sits in front; the remaining cases recede below and behind it.
        for (int i = 0; i < carriedItems.Count; i++)
        {
            PhysicalInteractable item = carriedItems[i];
            if (item == null)
            {
                continue;
            }

            int depth = (i - selectedIndex + carriedItems.Count) % carriedItems.Count;
            Vector3 localPosition = depth == 0
                ? new Vector3(0.14f, -0.09f, -0.05f)
                : new Vector3(0.14f + depth * 0.009f,
                    -0.115f - depth * 0.011f, 0.02f + depth * 0.012f);
            item.SetHeldPresentation(localPosition, Vector3.zero);
        }
    }

    private bool TryPlaceSelected()
    {
        PhysicalInteractable selected = SelectedItem;
        if (selected == null
            || !TryFindLookedAtShelfSlot(out ShelfSlot shelf, out Vector3 worldPoint))
        {
            return false;
        }

        // A full shelf still refuses placement. Do not remove the CD from the
        // hand stack until the shelf confirms a successful insertion.
        if (!shelf.TryPlace(selected, worldPoint))
        {
            return false;
        }

        RemoveSelectedFromStack();
        return true;
    }

    private void DropSelected()
    {
        PhysicalInteractable selected = SelectedItem;
        if (selected == null)
        {
            return;
        }

        selected.Drop();
        RemoveSelectedFromStack();
    }

    private void RemoveSelectedFromStack()
    {
        if (selectedIndex < 0 || selectedIndex >= carriedItems.Count)
        {
            return;
        }

        carriedItems.RemoveAt(selectedIndex);
        selectedIndex = carriedItems.Count == 0
            ? -1 : Mathf.Min(selectedIndex, carriedItems.Count - 1);
        ReflowStack();
        focusedMedia = null;
        if (carriedItems.Count == 0)
        {
            HidePlacementHints();
        }

        PrototypeSaveLoadController.NotifyPersistentStateChanged();
    }

    private PhysicalInteractable FindLookedAtInteractable()
    {
        if (playerCamera == null)
        {
            return null;
        }

        Ray ray = new(playerCamera.transform.position, playerCamera.transform.forward);
        int hitCount = Physics.RaycastNonAlloc(ray, hits, interactionRange,
            interactionLayers, QueryTriggerInteraction.Ignore);

        PhysicalInteractable closest = null;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hits[i];
            PhysicalInteractable candidate =
                hit.collider.GetComponentInParent<PhysicalInteractable>();
            if (candidate == null || candidate.IsHeld || hit.distance >= closestDistance)
            {
                continue;
            }

            closest = candidate;
            closestDistance = hit.distance;
        }

        return closest;
    }

    private bool TryFindLookedAtShelfSlot(out ShelfSlot closestShelfSlot,
        out Vector3 closestHitPoint)
    {
        closestShelfSlot = null;
        closestHitPoint = Vector3.zero;
        if (playerCamera == null)
        {
            return false;
        }

        Ray ray = new(playerCamera.transform.position, playerCamera.transform.forward);
        int hitCount = Physics.RaycastNonAlloc(ray, hits, interactionRange,
            interactionLayers, QueryTriggerInteraction.Collide);

        float closestDistance = float.MaxValue;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hits[i];
            ShelfSlot candidate = hit.collider.GetComponentInParent<ShelfSlot>();
            if (candidate == null || hit.distance >= closestDistance)
            {
                continue;
            }

            closestShelfSlot = candidate;
            closestHitPoint = hit.point;
            closestDistance = hit.distance;
        }

        return closestShelfSlot != null;
    }

    private void RefreshPlacementHints()
    {
        HidePlacementHints();
        PhysicalInteractable selected = SelectedItem;
        if (selected != null
            && TryFindLookedAtShelfSlot(out ShelfSlot shelf, out Vector3 worldPoint))
        {
            shelf.ShowPlacementHintFor(selected, worldPoint);
        }
    }

    private void HidePlacementHints()
    {
        for (int i = 0; i < shelfSlots.Length; i++)
        {
            if (shelfSlots[i] != null)
            {
                shelfSlots[i].HidePlacementHint();
            }
        }
    }

    // Carried order and selection are stored separately from the physical cases'
    // saved ground poses. This keeps old version-2 saves compatible.
    public List<string> GetCarriedPhysicalIds()
    {
        List<string> ids = new();
        foreach (PhysicalInteractable item in carriedItems)
        {
            MediaItem media = item != null ? item.GetComponent<MediaItem>() : null;
            if (media != null && !string.IsNullOrWhiteSpace(media.PhysicalItemId))
            {
                ids.Add(media.PhysicalItemId);
            }
        }

        return ids;
    }

    public void RestoreCarriedStock(List<PhysicalInteractable> items, int selection)
    {
        carriedItems.Clear();
        selectedIndex = -1;
        if (holdPoint == null || items == null)
        {
            return;
        }

        foreach (PhysicalInteractable item in items)
        {
            if (item == null || item.CurrentShelfSlot != null || item.IsHeld
                || carriedItems.Count >= MaxCarryItems || carriedItems.Contains(item))
            {
                continue;
            }

            item.PickUp(holdPoint);
            carriedItems.Add(item);
        }

        selectedIndex = carriedItems.Count == 0
            ? -1 : Mathf.Clamp(selection, 0, carriedItems.Count - 1);
        shelfSlots = Object.FindObjectsByType<ShelfSlot>(FindObjectsInactive.Exclude);
        ReflowStack();
    }

    public void ClearHeldItemForPersistence()
    {
        carriedItems.Clear();
        selectedIndex = -1;
        focusedMedia = null;
        HidePlacementHints();
        // LoadGame follows with ResetSceneMediaToLoose, which releases every
        // previously carried PhysicalInteractable before restoring the save.
    }

    private void OnGUI()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        EnsureStyles();

        Color previous = GUI.color;
        GUI.color = focusedMedia != null
            ? new Color(0.90f, 0.94f, 0.78f, 0.95f)
            : new Color(0.87f, 0.90f, 0.84f, 0.65f);
        GUI.DrawTexture(new Rect(Screen.width * 0.5f - 2f,
            Screen.height * 0.5f - 2f, 4f, 4f), Texture2D.whiteTexture);
        GUI.color = previous;

        if (carriedItems.Count > 0)
        {
            DrawStackPanel();
        }

        AlbumDefinition album =
            focusedMedia != null ? focusedMedia.AlbumDefinition : null;
        if (album == null)
        {
            return;
        }

        float x = Mathf.Max(8f, Screen.width - 390f);
        float y = Mathf.Max(8f, Screen.height - 142f);
        DrawPanel(new Rect(x, y, 378f, 128f),
            new Color(0.07f, 0.13f, 0.16f, 0.94f));

        GUI.Label(new Rect(x + 14f, y + 9f, 350f, 22f),
            album.Genre.ToUpperInvariant(), albumDetailStyle);
        GUI.Label(new Rect(x + 14f, y + 32f, 350f, 28f),
            album.ArtistName, albumArtistStyle);
        GUI.Label(new Rect(x + 14f, y + 61f, 350f, 25f),
            album.AlbumTitle, albumTitleStyle);

        bool showingPickupTarget = !carriedItems.Contains(
            focusedMedia.GetComponent<PhysicalInteractable>());
        string hint = showingPickupTarget
            ? "E  ADD TO STACK (" + carriedItems.Count + "/10)"
            : "E  SHELVE   •   G  DROP   •   WHEEL  SELECT";
        GUI.Label(new Rect(x + 14f, y + 99f, 351f, 20f),
            hint, albumDetailStyle);
    }

    private void DrawStackPanel()
    {
        float width = 378f;
        float x = Mathf.Max(8f, Screen.width - width - 12f);
        float y = 14f;
        float height = 44f + carriedItems.Count * 16f;
        DrawPanel(new Rect(x, y, width, height),
            new Color(0.08f, 0.13f, 0.15f, 0.95f));

        GUI.Label(new Rect(x + 12f, y + 8f, width - 24f, 24f),
            "CARRYING " + carriedItems.Count + " / " + MaxCarryItems
            + "   •   SCROLL TO SELECT", albumDetailStyle);

        for (int i = 0; i < carriedItems.Count; i++)
        {
            MediaItem media = carriedItems[i] != null
                ? carriedItems[i].GetComponent<MediaItem>() : null;
            AlbumDefinition album = media != null ? media.AlbumDefinition : null;
            if (album == null)
            {
                continue;
            }

            float rowY = y + 32f + i * 16f;
            if (i == selectedIndex)
            {
                DrawPanel(new Rect(x + 7f, rowY, width - 14f, 16f),
                    new Color(0.25f, 0.40f, 0.41f, 0.9f));
            }

            string title = album.ArtistName + "  •  " + album.AlbumTitle;
            GUI.Label(new Rect(x + 15f, rowY, width - 30f, 16f),
                (i == selectedIndex ? "▶ " : "   ") + (i + 1) + ". " + title,
                i == selectedIndex ? stackSelectedStyle : stackEntryStyle);
        }
    }

    private static void DrawPanel(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private void EnsureStyles()
    {
        if (albumArtistStyle != null)
        {
            return;
        }

        albumArtistStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            clipping = TextClipping.Clip,
            normal = { textColor = new Color(0.98f, 0.95f, 0.87f) }
        };
        albumTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            clipping = TextClipping.Clip,
            normal = { textColor = Color.white }
        };
        albumDetailStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            clipping = TextClipping.Clip,
            normal = { textColor = new Color(0.82f, 0.88f, 0.86f) }
        };
        stackEntryStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            clipping = TextClipping.Clip,
            normal = { textColor = new Color(0.80f, 0.84f, 0.84f) }
        };
        stackSelectedStyle = new GUIStyle(stackEntryStyle)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
    }
}
