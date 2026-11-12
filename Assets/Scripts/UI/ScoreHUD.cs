using TMPro;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class ScoreHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerScore playerScore;
    [SerializeField] private RunManager runManager;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text breakdownText;

    [Header("Score Punch")]
    [SerializeField, Range(1f, 1.5f)]
    private float punchScale = 1.15f;

    [SerializeField, Min(0.01f)]
    private float punchDuration = 0.2f;

    [Header("Breakdown")]
    [SerializeField, Min(0f)]
    private float holdDuration = 1f;

    [SerializeField, Min(0.01f)]
    private float fadeDuration = 0.35f;

    private Vector3 normalScoreScale;
    private Color normalBreakdownColor;

    private float punchRemaining;
    private float breakdownRemaining;
    private long lastDisplayedScore = -1;

    private void Awake()
    {
        normalScoreScale = scoreText.rectTransform.localScale;
        normalBreakdownColor = breakdownText.color;
    }

    private void OnEnable()
    {
        playerScore.TreasureCollected += OnTreasureCollected;

        lastDisplayedScore = -1;
        ClearFeedback();
    }

    private void OnDisable()
    {
        if (playerScore != null)
            playerScore.TreasureCollected -= OnTreasureCollected;

        ClearFeedback();
    }

    private void LateUpdate()
    {
        long score = playerScore.CurrentScore;

        if (score != lastDisplayedScore)
        {
            scoreText.text = $"LOOT: {score:N0}";
            lastDisplayedScore = score;
        }

        if (runManager.IsDead || runManager.HasEscaped)
        {
            ClearFeedback();
            return;
        }

        AnimateScore();
        AnimateBreakdown();
    }

    private void OnTreasureCollected(
        long previousScore,
        int multiplier,
        int addedValue,
        long resultingScore)
    {
        punchRemaining = punchDuration;
        breakdownRemaining = holdDuration + fadeDuration;

        if (addedValue > 0)
        {
            breakdownText.text =
                $"{previousScore:N0} × {multiplier} + " +
                $"{addedValue:N0} = {resultingScore:N0}";
        }
        else
        {
            breakdownText.text =
                $"{previousScore:N0} × {multiplier} = " +
                $"{resultingScore:N0}";
        }

        breakdownText.color = normalBreakdownColor;
    }

    private void AnimateScore()
    {
        float remaining = Mathf.Clamp01(
            punchRemaining / Mathf.Max(0.01f, punchDuration)
        );

        float eased = Mathf.SmoothStep(0f, 1f, remaining);

        scoreText.rectTransform.localScale =
            normalScoreScale * Mathf.Lerp(1f, punchScale, eased);

        punchRemaining = Mathf.Max(
            0f,
            punchRemaining - Time.deltaTime
        );
    }

    private void AnimateBreakdown()
    {
        float alpha = Mathf.Clamp01(
            breakdownRemaining / Mathf.Max(0.01f, fadeDuration)
        );

        Color color = normalBreakdownColor;
        color.a *= alpha;
        breakdownText.color = color;

        breakdownRemaining = Mathf.Max(
            0f,
            breakdownRemaining - Time.deltaTime
        );
    }

    private void ClearFeedback()
    {
        punchRemaining = 0f;
        breakdownRemaining = 0f;

        scoreText.rectTransform.localScale = normalScoreScale;
        breakdownText.text = "";

        Color color = normalBreakdownColor;
        color.a = 0f;
        breakdownText.color = color;
    }
}