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

        if (TryUseClockOutPoint())
        {
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

        ShelfSlot shelfSlot = FindLookedAtShelfSlot();
        if (shelfSlot == null)
        {
            return false;
        }

        if (shelfSlot.TryPlace(heldItem))
        {
            heldItem = null;
        }

        return true;
    }

    private ShelfSlot FindLookedAtShelfSlot()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        int hitCount = Physics.RaycastNonAlloc(ray, hits, interactionRange, interactionLayers, QueryTriggerInteraction.Ignore);

        ShelfSlot closestShelfSlot = null;
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
            closestDistance = hit.distance;
        }

        return closestShelfSlot;
    }

    private bool TryUseClockOutPoint()
    {
        ClockOutPoint clockOutPoint = FindLookedAtClockOutPoint();
        if (clockOutPoint == null)
        {
            return false;
        }

        clockOutPoint.Interact();
        return true;
    }

    private ClockOutPoint FindLookedAtClockOutPoint()
    {
        if (playerCamera == null)
        {
            return null;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        int hitCount = Physics.RaycastNonAlloc(ray, hits, interactionRange, interactionLayers, QueryTriggerInteraction.Ignore);

        ClockOutPoint closestClockOutPoint = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hits[i];
            ClockOutPoint clockOutPoint = hit.collider.GetComponentInParent<ClockOutPoint>();
            if (clockOutPoint == null || hit.distance >= closestDistance)
            {
                continue;
            }

            closestClockOutPoint = clockOutPoint;
            closestDistance = hit.distance;
        }

        return closestClockOutPoint;
    }

    private void DropHeldItem()
    {
        heldItem.Drop();
        heldItem = null;
    }
}
