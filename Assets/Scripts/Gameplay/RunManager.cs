using System;
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

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private double startTime;
    private double endTime;
    private bool restarting;
    private bool submissionStarted;

    public float TimeRemaining { get; private set; }
    public float CompletionTime { get; private set; }
    public long SecuredScore { get; private set; }

    public bool HasStarted { get; private set; }
    public bool IsDead { get; private set; }
    public bool HasEscaped { get; private set; }

    public string LeaderboardStatus { get; private set; } = "";

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
        if (restarting)
            return;

        if (ScreenFade.Instance != null &&
            ScreenFade.Instance.IsBusy)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.rKey.wasPressedThisFrame)
        {
            ChangeScene(SceneManager.GetActiveScene().name);
        }
        else if (HasEnded && keyboard.mKey.wasPressedThisFrame)
        {
            ChangeScene(mainMenuSceneName);
        }
    }

    private void ChangeScene(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"Scene '{sceneName}' is not available. " +
                "Check its name and the build scene list.",
                this
            );

            return;
        }

        ScreenFade fade = ScreenFade.Instance;

        if (fade != null)
        {
            if (!fade.LoadSceneWithFade(sceneName))
                return;
        }

        restarting = true;
        Time.timeScale = 1f;

        if (player != null)
            player.enabled = false;

        if (fade == null)
            SceneManager.LoadSceneAsync(sceneName);
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

        // Check the actual deadline, not last frame's display.
        UpdateRemainingTime();

        if (TimeRemaining <= 0f)
        {
            Die();
            return false;
        }

        SecuredScore = playerScore.EscapeScore;
        CompletionTime = (float)(Time.timeAsDouble - startTime);

        HasEscaped = true;

        if (player != null)
            player.enabled = false;

        SubmitLeaderboardResult();

        return true;
    }

    private async void SubmitLeaderboardResult()
    {
        if (submissionStarted)
            return;

        submissionStarted = true;

        OnlineLeaderboard leaderboard = OnlineLeaderboard.Instance;

        if (leaderboard == null)
        {
            LeaderboardStatus = "Leaderboard unavailable.";

            Debug.LogWarning(
                "Score not submitted: OnlineLeaderboard is missing. " +
                "Start testing from the MainMenu scene.");

            return;
        }

        if (!PlayerNameSetup.HasValidName)
        {
            LeaderboardStatus =
                "Save a name in the Leaderboard menu before your next run.";

            Debug.LogWarning(
                "Score not submitted: no valid player name is saved.");

            return;
        }

        // Capture this run's result before starting the upload.
        long score = SecuredScore;
        float escapeTime = CompletionTime;

        LeaderboardStatus = "Submitting score...";

        try
        {
            await leaderboard.SubmitEscapeAsync(score, escapeTime);

            // This scene may have been unloaded while awaiting the upload.
            if (this != null)
                LeaderboardStatus = "Leaderboard updated.";

            Debug.Log(
                "Leaderboard submission completed. " +
                "Your best result has been retained.");
        }
        catch (Exception exception)
        {
            if (this != null)
                LeaderboardStatus = "Upload failed. Check your connection.";

            Debug.LogWarning(
                $"Score upload failed: {exception.Message}");
        }
    }

    private void UpdateRemainingTime()
    {
        TimeRemaining = Mathf.Max(
            0f,
            (float)(endTime - Time.timeAsDouble)
        );
    }

    public void Die()
    {
        if (HasEnded || restarting)
            return;

        TimeRemaining = 0f;
        SecuredScore = 0;
        IsDead = true;

        if (player != null)
            player.enabled = false;
    }
}