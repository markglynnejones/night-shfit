using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class PhysicalInteractable : MonoBehaviour
{
    [SerializeField] private Vector3 heldLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 heldLocalEulerAngles = Vector3.zero;

    private Rigidbody body;
    private Transform originalParent;
    private ShelfSlot currentShelfSlot;

    public ShelfSlot CurrentShelfSlot => currentShelfSlot;
    public bool IsHeld { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        originalParent = transform.parent;
    }

    private void LateUpdate()
    {
        if (!IsHeld)
        {
            return;
        }

        transform.localPosition = heldLocalPosition;
        transform.localRotation = Quaternion.Euler(heldLocalEulerAngles);
    }

    public void PickUp(Transform holdPoint)
    {
        if (holdPoint == null)
        {
            return;
        }

        IsHeld = true;
        SetShelfVisualMode(false);

        bool removedFromShelf = currentShelfSlot != null;
        currentShelfSlot?.ClearIfHolding(this);
        currentShelfSlot = null;

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.useGravity = false;
        body.isKinematic = true;
        body.detectCollisions = false;

        transform.SetParent(holdPoint, false);
        transform.localPosition = heldLocalPosition;
        transform.localRotation = Quaternion.Euler(heldLocalEulerAngles);

        if (removedFromShelf)
        {
            PrototypeSaveLoadController.NotifyPersistentStateChanged();
        }
    }

    public void Drop()
    {
        if (!IsHeld)
        {
            return;
        }

        SetShelfVisualMode(false);

        Transform currentParent = originalParent != null ? originalParent : null;
        transform.SetParent(currentParent, true);

        body.isKinematic = false;
        body.useGravity = true;
        body.detectCollisions = true;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        IsHeld = false;
        PrototypeSaveLoadController.NotifyPersistentStateChanged();
    }

    public void PlaceOnShelf(Transform snapPoint, ShelfSlot shelfSlot)
    {
        PlaceOnShelf(snapPoint, shelfSlot, Vector3.zero);
    }

    public void PlaceOnShelf(Transform snapPoint, ShelfSlot shelfSlot, Vector3 localOffset)
    {
        PlaceOnShelf(snapPoint, shelfSlot, localOffset, Vector3.zero);
    }

    public void PlaceOnShelf(Transform snapPoint, ShelfSlot shelfSlot, Vector3 localOffset, Vector3 localEulerAngles)
    {
        if (snapPoint == null)
        {
            return;
        }

        IsHeld = false;
        currentShelfSlot = shelfSlot;
        SetShelfVisualMode(true);

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.useGravity = false;
        body.isKinematic = true;
        body.detectCollisions = true;

        transform.SetParent(snapPoint, false);
        transform.localPosition = localOffset;
        transform.localRotation = Quaternion.Euler(localEulerAngles);
    }

    public void MoveWithinShelf(Vector3 localOffset)
    {
        if (currentShelfSlot == null)
        {
            return;
        }

        transform.localPosition = localOffset;
    }

    public void RestoreLooseState(Vector3 worldPosition, Quaternion worldRotation)
    {
        currentShelfSlot?.ClearIfHolding(this);
        currentShelfSlot = null;
        IsHeld = false;
        SetShelfVisualMode(false);

        Transform currentParent = originalParent != null ? originalParent : null;
        transform.SetParent(currentParent, true);
        transform.SetPositionAndRotation(worldPosition, worldRotation);

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.useGravity = true;
        body.isKinematic = false;
        body.detectCollisions = true;
    }

    private void SetShelfVisualMode(bool enabled)
    {
        AlbumCaseLabel label = GetComponent<AlbumCaseLabel>();
        if (label != null)
        {
            label.SetShelfMode(enabled);
        }
    }
}
