using TMPro;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class RunHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunManager runManager;
    [SerializeField] private FirstPersonController player;

    [Header("Timer")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject startPrompt;
    [SerializeField] private GameObject deathText;

    [Header("Stamina")]
    [SerializeField] private RectTransform staminaFill;
    [SerializeField] private RectTransform staminaTrail;

    [Header("Trail Feel")]
    [SerializeField] private float trailDelay = 0.12f;
    [SerializeField] private float trailFollowSpeed = 0.65f;

    private float displayedTrail = 1f;
    private float delayRemaining;
    private bool wasDraining;
    private float previousStamina = 1f;

    private void Start()
    {
        startPrompt.SetActive(true);
        deathText.SetActive(false);

        previousStamina = player.StaminaNormalized;
        displayedTrail = previousStamina;

        SetBarWidth(staminaFill, previousStamina);
        SetBarWidth(staminaTrail, displayedTrail);
    }

    private void LateUpdate()
    {
        UpdateTimer();
        UpdateStamina();
    }

    private void UpdateTimer()
    {
        // Round upwards so the display only reaches zero at death.
        int totalMilliseconds = Mathf.CeilToInt(
            runManager.TimeRemaining * 1000f
        );

        int seconds = totalMilliseconds / 1000;
        int milliseconds = totalMilliseconds % 1000;

        // Fixed character spacing prevents the digits from wobbling.
        timerText.text =
            $"<mspace=0.65em>{seconds:00}</mspace>" +
            $"<size=50%><mspace=0.65em>.{milliseconds:000}" +
            "</mspace></size>";

        startPrompt.SetActive(!runManager.HasStarted);
        deathText.SetActive(runManager.IsDead);
    }

    private void UpdateStamina()
    {
        float currentStamina = player.StaminaNormalized;
        bool isDraining = currentStamina < previousStamina;

        // Start the short delay when a new sprint burst begins.
        if (isDraining && !wasDraining)
            delayRemaining = trailDelay;

        SetBarWidth(staminaFill, currentStamina);

        if (delayRemaining > 0f)
        {
            delayRemaining = Mathf.Max(
                0f,
                delayRemaining - Time.deltaTime
            );
        }
        else
        {
            displayedTrail = Mathf.MoveTowards(
                displayedTrail,
                currentStamina,
                trailFollowSpeed * Time.deltaTime
            );
        }

        displayedTrail = Mathf.Max(displayedTrail, currentStamina);
        SetBarWidth(staminaTrail, displayedTrail);

        previousStamina = currentStamina;
        wasDraining = isDraining;
    }

    private void SetBarWidth(RectTransform bar, float amount)
    {
        bar.localScale = new Vector3(
            Mathf.Clamp01(amount),
            1f,
            1f
        );
    }
}