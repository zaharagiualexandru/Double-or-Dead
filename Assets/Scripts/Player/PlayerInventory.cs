using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private RunManager runManager;

    public bool HasKey { get; private set; }

    public bool TryCollectKey()
    {
        if (runManager == null ||
            !runManager.CanCollectTreasure ||
            HasKey)
        {
            return false;
        }

        HasKey = true;
        return true;
    }

    public bool TryUseKey()
    {
        if (runManager == null ||
            !runManager.CanCollectTreasure ||
            !HasKey)
        {
            return false;
        }

        HasKey = false;
        return true;
    }
}