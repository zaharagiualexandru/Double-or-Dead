using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FirstPersonController player;
    [SerializeField] private PlayerScore playerScore;

    [Header("Run Settings")]
    [SerializeField] private float runDuration = 20f;

    private double startTime;
    private double endTime;
    private bool restarting;

    public float TimeRemaining { get; private set; }
    public float CompletionTime { get; private set; }
    public long SecuredScore { get; private set; }

    public bool HasStarted { get; private set; }
    public bool IsDead { get; private set; }
    public bool HasEscaped { get; private set; }

    public bool HasEnded => IsDead || HasEscaped;

    public bool CanCollectTreasure =>
        HasStarted &&
        !HasEnded &&
        !restarting &&
        Time.timeAsDouble < endTime;

    private void Awake()
    {
        TimeRemaining = runDuration;
    }

    private void Update()
    {
        if (!restarting &&
            Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            restarting = true;
            Time.timeScale = 1f;

            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }
    }

    private void LateUpdate()
    {
        if (restarting || HasEnded || player == null)
            return;

        if (!HasStarted)
        {
            if (!player.HasStartedMoving)
                return;

            HasStarted = true;
            startTime = Time.timeAsDouble;
            endTime = startTime + runDuration;
        }

        UpdateRemainingTime();

        if (TimeRemaining <= 0f)
            Die();
    }

    public bool TryEscape(PlayerScore escapingPlayer)
    {
        if (escapingPlayer == null ||
            escapingPlayer != playerScore ||
            !HasStarted ||
            HasEnded ||
            restarting)
        {
            return false;
        }

        // Check the actual deadline, not the previous frame's display.
        UpdateRemainingTime();

        if (TimeRemaining <= 0f)
        {
            Die();
            return false;
        }

        SecuredScore = playerScore.CurrentScore;
        CompletionTime = (float)(Time.timeAsDouble - startTime);

        HasEscaped = true;
        player.enabled = false;

        return true;
    }

    private void UpdateRemainingTime()
    {
        TimeRemaining = Mathf.Max(
            0f,
            (float)(endTime - Time.timeAsDouble)
        );
    }

    private void Die()
    {
        if (HasEnded)
            return;

        TimeRemaining = 0f;
        SecuredScore = 0;
        IsDead = true;

        player.enabled = false;
    }
}