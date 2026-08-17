using UnityEngine;

public sealed class ClockOutPoint : MonoBehaviour
{
    public void Interact()
    {
        if (ShiftManager.Instance == null)
        {
            Debug.Log("Clock-out point is not connected to the shift manager.");
            return;
        }

        ShiftManager.Instance.TryClockOut();
    }
}
