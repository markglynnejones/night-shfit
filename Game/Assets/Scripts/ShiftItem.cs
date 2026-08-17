using UnityEngine;

public sealed class ShiftItem : MonoBehaviour
{
    public bool IsRequiredForCompletion { get; private set; }
    public bool IsCorrectlyShelved { get; private set; }

    public void Configure(bool requiredForCompletion, bool startsCorrectlyShelved)
    {
        IsRequiredForCompletion = requiredForCompletion;
        IsCorrectlyShelved = startsCorrectlyShelved;
    }

    public bool MarkCorrectlyShelved()
    {
        if (IsCorrectlyShelved)
        {
            return false;
        }

        IsCorrectlyShelved = true;
        return true;
    }

    public bool MarkUnshelved()
    {
        if (!IsCorrectlyShelved)
        {
            return false;
        }

        IsCorrectlyShelved = false;
        return true;
    }
}
