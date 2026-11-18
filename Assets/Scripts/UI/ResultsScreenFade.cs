using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(200)]
public class ResultsScreenFade : MonoBehaviour
{
    [SerializeField] private RunManager runManager;
    [SerializeField] private CanvasGroup blackBackground;
    [SerializeField] private CanvasGroup resultContent;

    [SerializeField, Min(0.01f)]
    private float backgroundFadeDuration = 0.5f;

    [SerializeField, Min(0.01f)]
    private float textFadeDuration = 0.2f;

    private bool showingResults;

    private void Awake()
    {
        blackBackground.alpha = 0f;
        blackBackground.blocksRaycasts = false;
        blackBackground.interactable = false;

        resultContent.alpha = 0f;
        resultContent.blocksRaycasts = false;
        resultContent.interactable = false;
    }

    private void LateUpdate()
    {
        if (showingResults || !runManager.HasEnded)
            return;

        showingResults = true;

        if (runManager.IsDead)
        {
            blackBackground.alpha = 0f;
            blackBackground.blocksRaycasts = false;
            resultContent.alpha = 1f;
            return;
        }

        StartCoroutine(ShowResults());
    }

    private IEnumerator ShowResults()
    {
        blackBackground.blocksRaycasts = true;

        yield return FadeIn(
            blackBackground,
            backgroundFadeDuration
        );

        yield return FadeIn(
            resultContent,
            textFadeDuration
        );
    }

    private IEnumerator FadeIn(CanvasGroup group, float duration)
    {
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.SmoothStep(0f, 1f, progress);

            yield return null;
        }

        group.alpha = 1f;
    }
}