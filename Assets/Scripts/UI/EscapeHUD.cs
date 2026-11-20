using TMPro;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class EscapeHUD : MonoBehaviour
{
    [SerializeField] private RunManager runManager;
    [SerializeField] private PlayerScore playerScore;
    [SerializeField] private TMP_Text escapeText;
    [SerializeField] private TMP_Text uploadStatusText;

    [Header("Upload Message Colours")]
    [SerializeField]
    private Color submittingColour =
        new Color32(255, 208, 90, 255);

    [SerializeField]
    private Color successColour =
        new Color32(130, 230, 140, 255);

    [SerializeField]
    private Color failedColour =
        new Color32(255, 100, 100, 255);

    private bool resultsShown;
    private string lastStatus;
    private RunManager.UploadState lastState;

    private void Awake()
    {
        if (escapeText != null)
            escapeText.gameObject.SetActive(false);

        if (uploadStatusText != null)
        {
            uploadStatusText.text = "";
            uploadStatusText.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (runManager == null || !runManager.HasEscaped)
            return;

        if (!resultsShown)
        {
            resultsShown = true;

            string breakdown = "";

            if (playerScore != null)
            {
                string crownResult = playerScore.HasCollectedCrown
                    ? "Crown collected: x2"
                    : "Crown not collected: x1";

                breakdown =
                    $"Collected loot: {playerScore.CurrentScore:N0}\n" +
                    crownResult + "\n";
            }

            if (escapeText != null)
            {
                escapeText.text =
                    "ESCAPED\n" +
                    "<size=50%>" +
                    breakdown +
                    $"Secured loot: {runManager.SecuredScore:N0}\n" +
                    $"Escape time: {runManager.CompletionTime:F3}s" +
                    "</size>\n\n" +
                    "<size=40%>R — Retry\nM — Main Menu</size>";

                escapeText.gameObject.SetActive(true);
            }

            if (uploadStatusText != null)
                uploadStatusText.gameObject.SetActive(true);
        }

        UpdateUploadMessage();
    }

    private void UpdateUploadMessage()
    {
        if (uploadStatusText == null)
            return;

        string status = runManager.LeaderboardStatus;
        RunManager.UploadState state =
            runManager.LeaderboardUploadState;

        if (status == lastStatus && state == lastState)
            return;

        lastStatus = status;
        lastState = state;

        uploadStatusText.text = status;

        switch (state)
        {
            case RunManager.UploadState.Succeeded:
                uploadStatusText.color = successColour;
                break;

            case RunManager.UploadState.Failed:
                uploadStatusText.color = failedColour;
                break;

            default:
                uploadStatusText.color = submittingColour;
                break;
        }
    }
}