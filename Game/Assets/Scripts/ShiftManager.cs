using UnityEngine;

[DefaultExecutionOrder(-100)]
public sealed class ShiftManager : MonoBehaviour
{
    public static ShiftManager Instance { get; private set; }

    private int requiredItemCount;
    private int correctlyShelvedRequiredItemCount;

    public bool IsShiftComplete { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterItem(ShiftItem item)
    {
        if (item == null || !item.IsRequiredForCompletion)
        {
            return;
        }

        requiredItemCount++;
        if (item.IsCorrectlyShelved)
        {
            correctlyShelvedRequiredItemCount++;
        }
    }

    public void NotifyItemPlaced(PhysicalInteractable item)
    {
        ShiftItem shiftItem = item.GetComponent<ShiftItem>();
        if (shiftItem == null || !shiftItem.IsRequiredForCompletion || !shiftItem.MarkCorrectlyShelved())
        {
            return;
        }

        correctlyShelvedRequiredItemCount++;
        if (!IsShiftComplete && correctlyShelvedRequiredItemCount >= requiredItemCount)
        {
            IsShiftComplete = true;
            Debug.Log("Shift Complete - Clock Out");
        }
    }

    public void NotifyItemPickedUp(PhysicalInteractable item)
    {
        if (IsShiftComplete)
        {
            return;
        }

        ShiftItem shiftItem = item.GetComponent<ShiftItem>();
        if (shiftItem == null || !shiftItem.IsRequiredForCompletion || !shiftItem.MarkUnshelved())
        {
            return;
        }

        correctlyShelvedRequiredItemCount = Mathf.Max(0, correctlyShelvedRequiredItemCount - 1);
    }

    public void TryClockOut()
    {
        if (!IsShiftComplete)
        {
            Debug.Log("Shift is not complete yet.");
            return;
        }

        Debug.Log("Shift ended. Nice work.");
    }
}
