using TMPro;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class ScoreHUD : MonoBehaviour
{
    [SerializeField] private PlayerScore playerScore;
    [SerializeField] private TMP_Text scoreText;

    private long lastDisplayedScore = -1;

    private void LateUpdate()
    {
        long score = playerScore.CurrentScore;

        if (score == lastDisplayedScore)
            return;

        scoreText.text = $"LOOT: {score:N0}";
        lastDisplayedScore = score;
    }
}