using System;
using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    [SerializeField] private RunManager runManager;

    public long CurrentScore { get; private set; }
    public bool HasCollectedCrown { get; private set; }

    public int EscapeMultiplier => HasCollectedCrown ? 2 : 1;

    public long EscapeScore => CurrentScore * EscapeMultiplier;

    // Kept compatible with the existing ScoreHUD event.
    public event Action<long, int, int, long> TreasureCollected;

    public bool TryCollectTreasure(int value)
    {
        if (runManager == null ||
            !runManager.CanCollectTreasure ||
            value <= 0)
        {
            return false;
        }

        // Leave enough room for the crown's escape bonus.
        if (CurrentScore > long.MaxValue / 2 - value)
        {
            Debug.LogWarning("Score limit reached.", this);
            return false;
        }

        long previousScore = CurrentScore;

        CurrentScore += value;

        TreasureCollected?.Invoke(
            previousScore,
            1,
            value,
            CurrentScore
        );

        return true;
    }

    public bool TryCollectCrown()
    {
        if (runManager == null ||
            !runManager.CanCollectTreasure ||
            HasCollectedCrown)
        {
            return false;
        }

        // The bonus is awarded when escaping.
        HasCollectedCrown = true;

        return true;
    }

    private void LateUpdate()
    {
        if (runManager != null && runManager.IsDead)
            CurrentScore = 0;
    }
}