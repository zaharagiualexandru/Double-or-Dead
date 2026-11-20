using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;

public class OnlineLeaderboard : MonoBehaviour
{
    public static OnlineLeaderboard Instance { get; private set; }

    public const string LeaderboardId = "top_thieves";
    public const long RankingFactor = 20001;
    public const int MaximumTimeMilliseconds = 20000;

    private const long MaximumExactInteger = 9007199254740991L;

    private Task initializationTask;

    // Used by LeaderboardBoardUI to read existing score metadata.
    [Serializable]
    public class EntryData
    {
        public int version = 1;
        public string playerName;
        public long loot;
        public int escapeTimeMilliseconds;
    }

    [Serializable]
    public class SubmitScoreResponse
    {
        public bool success;
        public string message;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        try
        {
            await EnsureReadyAsync();
            Debug.Log("Online leaderboard ready.");
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"Leaderboard unavailable: {exception.Message}");
        }
    }

    public Task EnsureReadyAsync()
    {
        if (initializationTask == null ||
            initializationTask.IsFaulted ||
            initializationTask.IsCanceled ||
            (initializationTask.IsCompleted &&
             !AuthenticationService.Instance.IsSignedIn))
        {
            initializationTask = InitializeAsync();
        }

        return initializationTask;
    }

    private async Task InitializeAsync()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async Task SubmitEscapeAsync(long loot, float escapeSeconds)
    {
        if (float.IsNaN(escapeSeconds) ||
            float.IsInfinity(escapeSeconds) ||
            escapeSeconds < 0f ||
            escapeSeconds >= 20f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(escapeSeconds),
                "Escape time must be below 20 seconds.");
        }

        long maximumLoot =
            (MaximumExactInteger - MaximumTimeMilliseconds) / RankingFactor;

        if (loot < 0 || loot > maximumLoot)
        {
            throw new ArgumentOutOfRangeException(nameof(loot));
        }

        int milliseconds = (int)Math.Round(
            escapeSeconds * 1000d,
            MidpointRounding.AwayFromZero);

        await EnsureReadyAsync();

        if (!PlayerNameSetup.HasValidName)
        {
            throw new InvalidOperationException(
                "Confirm your name online in the Leaderboard menu first.");
        }

        string submittingPlayerId =
            AuthenticationService.Instance.PlayerId;

        var arguments = new Dictionary<string, object>
        {
            { "loot", loot },
            { "escapeTimeMilliseconds", milliseconds }
        };

        SubmitScoreResponse response =
            await CloudCodeService.Instance
                .CallEndpointAsync<SubmitScoreResponse>(
                    "SubmitEscapeScore",
                    arguments);

        if (!AuthenticationService.Instance.IsSignedIn ||
            AuthenticationService.Instance.PlayerId != submittingPlayerId)
        {
            throw new InvalidOperationException(
                "The signed-in player changed during submission.");
        }

        if (response == null || !response.success)
        {
            throw new InvalidOperationException(
                response != null &&
                !string.IsNullOrWhiteSpace(response.message)
                    ? response.message
                    : "The server did not confirm score submission.");
        }

        Debug.Log(
            "Cloud Code accepted the score submission. " +
            "The leaderboard keeps your best score.");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}