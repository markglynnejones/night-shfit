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
    private MediaItem inspectedMedia;
    private PhysicalInteractable aimedItem;
    private ShelfSlot aimedShelf;
    private int hoveredColumnCount;
    private GUIStyle inspectHeaderStyle;
    private GUIStyle inspectArtistStyle;
    private GUIStyle inspectAlbumStyle;

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

        // With CDs in hand, wheel cycles the ten-item carrying stack.
        // Empty-handed, it flips through the four-deep column at the cursor.
        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            float wheel = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(wheel) > 0.01f)
            {
                if (carriedItems.Count > 1)
                {
                    SelectRelative(wheel > 0f ? -1 : 1);
                }
                else if (carriedItems.Count == 0
                    && TryFindLookedAtShelfSlot(out ShelfSlot rack,
                        out Vector3 wheelRackHitPoint))
                {
                    rack.FlipCoverAt(wheelRackHitPoint, wheel < 0f);
                }
            }
        }

        if (carriedItems.Count > 0)
        {
            RefreshPlacementHints();
        }

        PhysicalInteractable lookedAt = FindLookedAtInteractable();
        aimedItem = lookedAt;
        aimedShelf = TryFindLookedAtShelfSlot(out ShelfSlot visibleRack,
            out _) ? visibleRack : null;
        PhysicalInteractable selected = SelectedItem;

        // Stable focus while carrying: the card describes the SELECTED CD,
        // not whichever shelf case happens to be under the crosshair.
        PhysicalInteractable shown = selected != null ? selected : lookedAt;
        focusedMedia = shown != null ? shown.GetComponent<MediaItem>() : null;
        // Holding Q deliberately inspects the aimed-at case if there is one.
        inspectedMedia = lookedAt != null ? lookedAt.GetComponent<MediaItem>() : focusedMedia;
        hoveredColumnCount = 0;
        if (selected == null
            && TryFindLookedAtShelfSlot(out ShelfSlot facing, out Vector3 point)
            && facing.TryGetFrontCaseAt(point, out _, out int count))
        {
            hoveredColumnCount = count;
        }

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

    private Ray CentreScreenRay()
    {
        // Always use the camera's true optical centre, matching the dot drawn
        // at Screen.width / 2 and Screen.height / 2. This also handles camera
        // lens shift and aspect settings correctly.
        return playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    }

    private PhysicalInteractable FindLookedAtInteractable()
    {
        if (playerCamera == null)
        {
            return null;
        }

        Ray ray = CentreScreenRay();
        int hitCount = Physics.RaycastNonAlloc(ray, hits, interactionRange,
            interactionLayers, QueryTriggerInteraction.Ignore);

        // The first *solid* hit controls interaction. Do not look through
        // furniture or another case to select an album further away.
        PhysicalInteractable direct = null;
        float firstSolidDistance = interactionRange;
        for (int i = 0; i < hitCount; i++)
        {
            if (hits[i].distance >= firstSolidDistance)
            {
                continue;
            }

            firstSolidDistance = hits[i].distance;
            PhysicalInteractable candidate =
                hits[i].collider.GetComponentInParent<PhysicalInteractable>();
            direct = candidate != null && !candidate.IsHeld ? candidate : null;
        }

        if (direct != null)
        {
            return direct;
        }

        // A tiny, 2 cm-wide assist compensates for small realistic jewel cases.
        // Never go through the nearest blocking solid collider.
        const float assistanceRadius = 0.02f;
        int nearbyHits = Physics.SphereCastNonAlloc(ray, assistanceRadius,
            hits, firstSolidDistance, interactionLayers,
            QueryTriggerInteraction.Ignore);

        PhysicalInteractable nearCase = null;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < nearbyHits; i++)
        {
            RaycastHit hit = hits[i];
            PhysicalInteractable candidate =
                hit.collider.GetComponentInParent<PhysicalInteractable>();
            if (candidate == null || candidate.IsHeld
                || hit.distance >= nearestDistance)
            {
                continue;
            }

            nearCase = candidate;
            nearestDistance = hit.distance;
        }

        if (nearCase != null)
        {
            return nearCase;
        }

        // A shallow browse-bin lip can hide a case collider. Only resolve
        // the front case if the crosshair is genuinely near a visible facing,
        // never by guessing from the large trigger's entry-point height.
        if (carriedItems.Count == 0
            && TryFindLookedAtShelfSlot(out ShelfSlot shelf, out Vector3 facePoint)
            && shelf.TryGetFrontCaseAt(facePoint,
                out PhysicalInteractable front, out _)
            && front != null && !front.IsHeld)
        {
            return front;
        }

        return null;
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

        if (shelfSlots.Length == 0)
        {
            shelfSlots = Object.FindObjectsByType<ShelfSlot>(FindObjectsInactive.Exclude);
        }

        Ray ray = CentreScreenRay();
        float bestMissSquared = float.MaxValue;
        float bestDistance = float.MaxValue;

        // A thick shelf trigger is hit at a different height from the CD
        // behind it. Some top/bottom rows never intersected it at all.
        // Instead, test the real cover facings in every section against the
        // precise centre-screen ray, regardless of the trigger's shape.
        foreach (ShelfSlot shelf in shelfSlots)
        {
            if (shelf == null || !shelf.TryGetAimedFacePoint(
                ray, interactionRange, out Vector3 facingPoint))
            {
                continue;
            }

            Vector3 offset = facingPoint - ray.origin;
            float distance = Vector3.Dot(offset, ray.direction);
            if (distance <= 0f || distance > interactionRange)
            {
                continue;
            }

            float missSquared = Vector3.Cross(ray.direction, offset).sqrMagnitude;
            if (missSquared > bestMissSquared + 0.0001f
                || (Mathf.Abs(missSquared - bestMissSquared) <= 0.0001f
                    && distance >= bestDistance))
            {
                continue;
            }

            closestShelfSlot = shelf;
            closestHitPoint = facingPoint;
            bestMissSquared = missSquared;
            bestDistance = distance;
        }

        if (closestShelfSlot == null)
        {
            return false;
        }

        // Keep real occlusion. CDs and the frame belonging to the aimed rack
        // are allowed, but a different rack, loose CD, table or wall in front
        // of it blocks picking and shelving through that object.
        int hitCount = Physics.RaycastNonAlloc(ray, hits, bestDistance,
            interactionLayers, QueryTriggerInteraction.Ignore);
        Transform targetRoot = closestShelfSlot.transform.parent;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider == null || hit.distance + 0.015f >= bestDistance)
            {
                continue;
            }

            PhysicalInteractable cd = hit.collider.GetComponentInParent<PhysicalInteractable>();
            if (cd != null && cd.CurrentShelfSlot == closestShelfSlot)
            {
                continue;
            }

            if (targetRoot != null
                && hit.collider.transform.IsChildOf(targetRoot))
            {
                continue;
            }

            closestShelfSlot = null;
            closestHitPoint = Vector3.zero;
            return false;
        }

        return true;
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
        GUI.color = aimedItem != null
            ? new Color(0.64f, 0.94f, 0.76f, 1f)
            : aimedShelf != null
                ? new Color(0.60f, 0.85f, 0.96f, 1f)
                : new Color(0.92f, 0.91f, 0.86f, 0.75f);
        GUI.DrawTexture(new Rect(Screen.width * 0.5f - 2f,
            Screen.height * 0.5f - 2f, 4f, 4f), Texture2D.whiteTexture);
        GUI.color = previous;
        DrawAimPrompt();

        if (carriedItems.Count > 0)
        {
            DrawStackPanel();
        }

        if (focusedMedia != null && focusedMedia.AlbumDefinition != null)
        {
            DrawAlbumCard(focusedMedia);
        }

        // Hold Q to view the original cover at a readable size. The player
        // can still navigate the shop with a quick glance at the smaller card.
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.qKey.isPressed
            && inspectedMedia != null && inspectedMedia.AlbumDefinition != null)
        {
            DrawLargeInspection(inspectedMedia);
        }
    }

    private void DrawAimPrompt()
    {
        string message = null;
        if (aimedItem != null)
        {
            MediaItem media = aimedItem.GetComponent<MediaItem>();
            AlbumDefinition album = media != null ? media.AlbumDefinition : null;
            string artist = album != null ? album.ArtistName : "CD";
            if (carriedItems.Count < MaxCarryItems)
            {
                message = "E  PICK UP  " + artist;
                if (SelectedItem != null && aimedShelf != null)
                {
                    message += "     SHIFT+E  SHELVE";
                }
            }
            else if (aimedShelf != null)
            {
                message = "E  SHELVE SELECTED CD";
            }
        }
        else if (aimedShelf != null && SelectedItem != null)
        {
            message = "E  SHELVE SELECTED CD";
        }

        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        const float maxWidth = 320f;
        float width = Mathf.Min(maxWidth, Screen.width - 24f);
        float x = Mathf.Clamp(Screen.width * 0.5f + 13f,
            8f, Screen.width - width - 8f);
        float y = Mathf.Clamp(Screen.height * 0.5f + 11f,
            8f, Screen.height - 34f);
        DrawPanel(new Rect(x, y, width, 27f),
            new Color(0.06f, 0.12f, 0.14f, 0.91f));
        GUI.Label(new Rect(x + 8f, y + 3f, width - 16f, 21f),
            FitText(message, albumDetailStyle, width - 16f), albumDetailStyle);
    }

    private void DrawAlbumCard(MediaItem media)
    {
        AlbumDefinition album = media.AlbumDefinition;
        bool compact = Screen.height < 510f;
        float height = compact ? 162f : 214f;
        float width = Mathf.Min(Screen.width - 22f,
            Mathf.Min(510f, Mathf.Max(340f, Screen.width * 0.47f)));
        float x = Screen.width - width - 12f;
        float y = Mathf.Max(8f, Screen.height - height - 10f);

        DrawPanel(new Rect(x, y, width, height),
            new Color(0.065f, 0.12f, 0.15f, 0.97f));
        float coverSize = height - 24f;
        DrawCoverPreview(media, new Rect(x + 12f, y + 12f,
            coverSize, coverSize));

        float textX = x + coverSize + 24f;
        float textWidth = Mathf.Max(90f, width - coverSize - 36f);
        bool selected = SelectedItem != null
            && media == SelectedItem.GetComponent<MediaItem>();

        string category = album.Genre.ToUpperInvariant()
            + (selected ? "  /  SELECTED CD" : "  /  BROWSING");
        GUI.Label(new Rect(textX, y + 10f, textWidth, 25f),
            category, albumDetailStyle);

        GUI.Label(new Rect(textX, y + 37f, textWidth,
            compact ? 40f : 57f), album.ArtistName, albumArtistStyle);
        GUI.Label(new Rect(textX, y + (compact ? 85f : 99f),
            textWidth, compact ? 37f : 57f),
            album.AlbumTitle, albumTitleStyle);

        if (!selected && hoveredColumnCount > 1)
        {
            GUI.Label(new Rect(textX, y + height - 51f, textWidth, 22f),
                hoveredColumnCount + " CASES IN THIS BIN",
                albumDetailStyle);
        }

        string controls = selected
            ? "WHEEL choose  •  E shelve  •  G drop"
            : "E collect  •  WHEEL flip rack";
        GUI.Label(new Rect(textX, y + height - 29f, textWidth, 21f),
            FitText(controls, albumDetailStyle, textWidth),
            albumDetailStyle);

        DrawPanel(new Rect(x + 12f, y + height - 34f,
            coverSize, 22f), new Color(0.06f, 0.12f, 0.15f, 0.94f));
        GUI.Label(new Rect(x + 16f, y + height - 33f,
            coverSize - 8f, 20f), "HOLD Q  INSPECT", stackSelectedStyle);
    }

    private void DrawLargeInspection(MediaItem media)
    {
        AlbumDefinition album = media.AlbumDefinition;

        // Temporary enlarged information card. This never picks up an item,
        // pauses the world or modifies a shelf/save.
        DrawPanel(new Rect(0f, 0f, Screen.width, Screen.height),
            new Color(0.02f, 0.055f, 0.07f, 0.86f));

        float width = Mathf.Min(Screen.width - 30f, 740f);
        float height = Mathf.Min(Screen.height - 24f, 455f);
        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - height) * 0.5f;
        DrawPanel(new Rect(x, y, width, height),
            new Color(0.08f, 0.15f, 0.17f, 0.98f));

        float artSize = Mathf.Min(height - 36f, width * 0.56f);
        DrawCoverPreview(media,
            new Rect(x + 18f, y + 18f, artSize, artSize));

        float textX = x + artSize + 32f;
        float textWidth = Mathf.Max(75f, width - artSize - 48f);
        GUI.Label(new Rect(textX, y + 20f, textWidth, 28f),
            album.Genre.ToUpperInvariant() + "  /  CD",
            inspectHeaderStyle);
        GUI.Label(new Rect(textX, y + 60f, textWidth,
            Mathf.Max(55f, height * 0.31f)), album.ArtistName,
            inspectArtistStyle);
        GUI.Label(new Rect(textX, y + height * 0.47f, textWidth,
            Mathf.Max(55f, height * 0.30f)), album.AlbumTitle,
            inspectAlbumStyle);
        GUI.Label(new Rect(textX, y + height - 45f, textWidth, 27f),
            "RELEASE Q TO RETURN", inspectHeaderStyle);
    }

    private static void DrawCoverPreview(MediaItem media, Rect bounds)
    {
        DrawPanel(bounds, new Color(0.24f, 0.27f, 0.29f, 1f));
        AlbumCoverArt cover = media.GetComponent<AlbumCoverArt>();
        Texture2D texture = cover != null ? cover.CoverTexture : null;
        if (texture == null)
        {
            return;
        }

        // The cube front-face UV is 180 degrees from GUI texture coordinates.
        // The artwork was rotated for the physical case in the previous fix;
        // counter-rotate the shared texture when drawing it in screen-space.
        Matrix4x4 previous = GUI.matrix;
        GUIUtility.RotateAroundPivot(180f, bounds.center);
        GUI.DrawTexture(bounds, texture, ScaleMode.ScaleToFit);
        GUI.matrix = previous;
    }

    private void DrawStackPanel()
    {
        // Two columns of five fit a short Unity Game viewport. One long list
        // used to cut off the selected tenth item and long album titles.
        int columns = Screen.width >= 540 ? 2 : 1;
        float width = Mathf.Min(490f, Mathf.Max(280f, Screen.width * 0.49f));
        float x = Mathf.Max(6f, Screen.width - width - 12f);
        int rows = Mathf.CeilToInt(carriedItems.Count / (float)columns);
        bool compact = Screen.height < 360f;
        float rowHeight = compact ? 23f : columns == 2 ? 39f : 28f;
        float height = 43f + rows * rowHeight;
        float y = 12f;

        // Don't obscure the bottom-right album inspection card.
        height = Mathf.Min(height, Mathf.Max(70f, Screen.height - 151f));
        DrawPanel(new Rect(x, y, width, height),
            new Color(0.07f, 0.13f, 0.16f, 0.97f));

        GUI.Label(new Rect(x + 12f, y + 7f, width - 24f, 26f),
            "CARRYING  " + carriedItems.Count + " / " + MaxCarryItems
            + "   •   WHEEL TO CHOOSE", albumDetailStyle);

        float margin = 10f;
        float columnWidth = (width - margin * 2f - (columns - 1) * 6f)
            / columns;

        for (int i = 0; i < carriedItems.Count; i++)
        {
            int rowIndex = i / columns;
            int columnIndex = i % columns;
            float rowY = y + 36f + rowIndex * rowHeight;
            float colX = x + margin + columnIndex * (columnWidth + 6f);

            if (rowY + rowHeight > y + height + 0.5f)
            {
                // On very small Game windows, entries remain accessible by
                // wheel and are always readable in the full album card below.
                break;
            }

            MediaItem media = carriedItems[i] != null
                ? carriedItems[i].GetComponent<MediaItem>() : null;
            AlbumDefinition album = media != null ? media.AlbumDefinition : null;
            if (album == null)
            {
                continue;
            }

            if (i == selectedIndex)
            {
                DrawPanel(new Rect(colX, rowY, columnWidth,
                    rowHeight - 2f), new Color(0.25f, 0.43f, 0.43f, 0.95f));
            }

            float textWidth = columnWidth - 14f;
            GUIStyle artistStyle = i == selectedIndex
                ? stackSelectedStyle : stackEntryStyle;
            string artist = (i + 1) + ". " + album.ArtistName;

            GUI.Label(new Rect(colX + 7f, rowY + 1f, textWidth, 19f),
                FitText(artist, artistStyle, textWidth), artistStyle);

            if (columns == 2 && !compact)
            {
                GUI.Label(new Rect(colX + 7f, rowY + 20f, textWidth, 18f),
                    FitText(album.AlbumTitle, stackEntryStyle, textWidth),
                    stackEntryStyle);
            }
        }
    }

    private static string FitText(string content, GUIStyle style, float availableWidth)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        if (style.CalcSize(new GUIContent(content)).x <= availableWidth)
        {
            return content;
        }

        const string ellipsis = "…";
        int lower = 0;
        int upper = content.Length;
        while (lower < upper)
        {
            int middle = (lower + upper + 1) / 2;
            string candidate = content.Substring(0, middle) + ellipsis;
            if (style.CalcSize(new GUIContent(candidate)).x <= availableWidth)
            {
                lower = middle;
            }
            else
            {
                upper = middle - 1;
            }
        }

        return content.Substring(0, lower) + ellipsis;
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
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            clipping = TextClipping.Clip,
            normal = { textColor = new Color(0.82f, 0.88f, 0.86f) }
        };
        stackEntryStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            clipping = TextClipping.Clip,
            normal = { textColor = new Color(0.80f, 0.84f, 0.84f) }
        };
        stackSelectedStyle = new GUIStyle(stackEntryStyle)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        albumArtistStyle.wordWrap = true;
        albumTitleStyle.wordWrap = true;

        inspectHeaderStyle = new GUIStyle(albumDetailStyle)
        {
            fontSize = 15,
            wordWrap = true
        };
        inspectArtistStyle = new GUIStyle(albumArtistStyle)
        {
            fontSize = 25,
            wordWrap = true
        };
        inspectAlbumStyle = new GUIStyle(albumTitleStyle)
        {
            fontSize = 20,
            wordWrap = true
        };
    }
}
