using TMPro;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class EscapeHUD : MonoBehaviour
{
    [SerializeField] private RunManager runManager;
    [SerializeField] private TMP_Text escapeText;

    private bool resultsShown;

    private void Awake()
    {
        escapeText.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (resultsShown || !runManager.HasEscaped)
            return;

        resultsShown = true;

        escapeText.text =
            "ESCAPED\n" +
            $"<size=50%>Secured loot: {runManager.SecuredScore:N0}\n" +
            $"Escape time: {runManager.CompletionTime:F3}s</size>\n\n" +
            "<size=40%>R — Retry\nM — Main Menu</size>";

        escapeText.gameObject.SetActive(true);
    }
}