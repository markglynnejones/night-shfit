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

    private void DropHeldItem()
    {
        heldItem.Drop();
        heldItem = null;
    }
}
