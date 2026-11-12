using System;
using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    [SerializeField] private RunManager runManager;

    public long CurrentScore { get; private set; }
    public bool HasCollectedCrown { get; private set; }

    // Previous score, multiplier, added value, resulting score.
    public event Action<long, int, int, long> TreasureCollected;

    public bool TryCollectTreasure(int value)
    {
        if (runManager == null ||
            !runManager.CanCollectTreasure ||
            value <= 0)
        {
            return false;
        }

        // Prevent extremely long doubling chains from overflowing.
        if (CurrentScore > (long.MaxValue - value) / 2)
        {
            Debug.LogWarning("Score limit reached.", this);
            return false;
        }

        long previousScore = CurrentScore;
        CurrentScore = CurrentScore * 2 + value;

        TreasureCollected?.Invoke(
            previousScore,
            2,
            value,
            CurrentScore
        );

        return true;
    }

    private void LateUpdate()
    {
        // Dying loses the treasure carried during this run.
        if (runManager != null && runManager.IsDead)
            CurrentScore = 0;
    }

    public bool TryCollectCrown()
    {
        if (runManager == null ||
            !runManager.CanCollectTreasure ||
            HasCollectedCrown)
        {
            return false;
        }

        if (CurrentScore > long.MaxValue / 3)
        {
            Debug.LogWarning("Score limit reached.", this);
            return false;
        }

        long previousScore = CurrentScore;

        CurrentScore *= 3;
        HasCollectedCrown = true;

        TreasureCollected?.Invoke(
            previousScore,
            3,
            0,
            CurrentScore
        );

        return true;
    }
}