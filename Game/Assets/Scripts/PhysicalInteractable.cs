using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class PhysicalInteractable : MonoBehaviour
{
    [SerializeField] private Vector3 heldLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 heldLocalEulerAngles = Vector3.zero;

    private Rigidbody body;
    private Transform originalParent;

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

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.useGravity = false;
        body.isKinematic = true;
        body.detectCollisions = false;

        transform.SetParent(holdPoint, false);
        transform.localPosition = heldLocalPosition;
        transform.localRotation = Quaternion.Euler(heldLocalEulerAngles);
    }

    public void Drop()
    {
        if (!IsHeld)
        {
            return;
        }

        Transform currentParent = originalParent != null ? originalParent : null;
        transform.SetParent(currentParent, true);

        body.isKinematic = false;
        body.useGravity = true;
        body.detectCollisions = true;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        IsHeld = false;
    }
}
