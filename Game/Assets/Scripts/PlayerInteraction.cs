using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint = null;
    [SerializeField] private float interactionRange = 3.5f;
    [SerializeField] private LayerMask interactionLayers = Physics.DefaultRaycastLayers;

    private PhysicalInteractable heldItem;
    private readonly RaycastHit[] hits = new RaycastHit[8];
    private ShelfSlot[] shelfSlots = new ShelfSlot[0];

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
        HidePlacementHints();
    }
}
