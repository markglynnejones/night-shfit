using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint = null;
    [SerializeField] private float interactionRange = 3.5f;
    [SerializeField] private LayerMask interactionLayers = Physics.DefaultRaycastLayers;

    private PhysicalInteractable heldItem;
    private readonly RaycastHit[] hits = new RaycastHit[64];
    private ShelfSlot[] shelfSlots = new ShelfSlot[0];
    private MediaItem focusedMedia;
    private GUIStyle albumArtistStyle;
    private GUIStyle albumTitleStyle;
    private GUIStyle albumDetailStyle;

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
            GameObject holdPointObject = new GameObject("Interaction Hold Point");
            holdPoint = holdPointObject.transform;
            holdPoint.SetParent(playerCamera.transform, false);
            holdPoint.localPosition = new Vector3(0f, -0.12f, 0.85f);
            holdPoint.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        if (heldItem != null)
        {
            RefreshPlacementHints();
        }

        // Physical sleeves are now close to actual jewel-case size. Read the
        // catalogue from a subtle inspect card instead of giant floating labels.
        PhysicalInteractable focused = heldItem != null ? heldItem : FindLookedAtInteractable();
        focusedMedia = focused != null ? focused.GetComponent<MediaItem>() : null;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.eKey.wasPressedThisFrame)
        {
            return;
        }

        if (heldItem != null)
        {
            if (TryPlaceHeldItem())
            {
                return;
            }

            DropHeldItem();
            return;
        }

        TryPickUpLookedAtItem();
    }

    private void OnGUI()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        // Gentle fixed crosshair: see which small physical case you are aiming at.
        Color previousColor = GUI.color;
        GUI.color = focusedMedia != null
            ? new Color(0.87f, 0.91f, 0.70f, 0.9f)
            : new Color(0.91f, 0.90f, 0.83f, 0.5f);
        GUI.DrawTexture(new Rect(Screen.width * 0.5f - 2f, Screen.height * 0.5f - 2f,
            4f, 4f), Texture2D.whiteTexture);
        GUI.color = previousColor;

        AlbumDefinition album = focusedMedia != null ? focusedMedia.AlbumDefinition : null;
        if (album == null)
        {
            return;
        }

        if (albumArtistStyle == null)
        {
            albumArtistStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.97f, 0.94f, 0.87f) }
            };
            albumTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                normal = { textColor = Color.white }
            };
            albumDetailStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.79f, 0.84f, 0.81f) }
            };
        }

        float x = Mathf.Max(12f, Screen.width - 366f);
        float y = Mathf.Max(12f, Screen.height - 136f);
        GUI.color = new Color(0.10f, 0.16f, 0.18f, 0.91f);
        GUI.DrawTexture(new Rect(x, y, 350f, 119f), Texture2D.whiteTexture);
        GUI.color = previousColor;

        GUI.Label(new Rect(x + 14f, y + 9f, 322f, 22f), album.Genre.ToUpperInvariant(),
            albumDetailStyle);
        GUI.Label(new Rect(x + 14f, y + 31f, 322f, 29f), album.ArtistName,
            albumArtistStyle);
        GUI.Label(new Rect(x + 14f, y + 61f, 322f, 25f), album.AlbumTitle,
            albumTitleStyle);
        GUI.Label(new Rect(x + 14f, y + 93f, 322f, 19f),
            heldItem != null ? "E  PLACE ON SHELF / DROP" : "E  PICK UP CD",
            albumDetailStyle);
    }

    private void TryPickUpLookedAtItem()
    {
        if (playerCamera == null || holdPoint == null)
        {
            return;
        }

        PhysicalInteractable interactable = FindLookedAtInteractable();
        if (interactable == null || interactable.IsHeld)
        {
            return;
        }

        heldItem = interactable;
        heldItem.PickUp(holdPoint);
        focusedMedia = heldItem.GetComponent<MediaItem>();
        shelfSlots = Object.FindObjectsByType<ShelfSlot>(FindObjectsInactive.Exclude);
        RefreshPlacementHints();
    }

    private PhysicalInteractable FindLookedAtInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        int hitCount = Physics.RaycastNonAlloc(ray, hits, interactionRange, interactionLayers, QueryTriggerInteraction.Ignore);

        PhysicalInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hits[i];
            PhysicalInteractable interactable = hit.collider.GetComponentInParent<PhysicalInteractable>();
            if (interactable == null || hit.distance >= closestDistance)
            {
                continue;
            }

            closestInteractable = interactable;
            closestDistance = hit.distance;
        }

        return closestInteractable;
    }

    private bool TryPlaceHeldItem()
    {
        if (playerCamera == null)
        {
            return false;
        }

        if (!TryFindLookedAtShelfSlot(out ShelfSlot shelfSlot, out Vector3 placementPoint))
        {
            return false;
        }

        if (shelfSlot.TryPlace(heldItem, placementPoint))
        {
            heldItem = null;
            focusedMedia = null;
            HidePlacementHints();
        }

        return true;
    }

    private bool TryFindLookedAtShelfSlot(out ShelfSlot closestShelfSlot, out Vector3 closestHitPoint)
    {
        closestShelfSlot = null;
        closestHitPoint = Vector3.zero;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        int hitCount = Physics.RaycastNonAlloc(ray, hits, interactionRange, interactionLayers, QueryTriggerInteraction.Ignore);

        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hits[i];
            ShelfSlot shelfSlot = hit.collider.GetComponentInParent<ShelfSlot>();
            if (shelfSlot == null || hit.distance >= closestDistance)
            {
                continue;
            }

            closestShelfSlot = shelfSlot;
            closestHitPoint = hit.point;
            closestDistance = hit.distance;
        }

        return closestShelfSlot != null;
    }

    private void DropHeldItem()
    {
        heldItem.Drop();
        heldItem = null;
        focusedMedia = null;
        HidePlacementHints();
    }

    private void RefreshPlacementHints()
    {
        HidePlacementHints();

        if (TryFindLookedAtShelfSlot(out ShelfSlot shelfSlot, out Vector3 placementPoint))
        {
            shelfSlot.ShowPlacementHintFor(heldItem, placementPoint);
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

    public void ClearHeldItemForPersistence()
    {
        heldItem = null;
        focusedMedia = null;
        HidePlacementHints();
    }
}
