using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;

public class OnlineLeaderboard : MonoBehaviour
{
    public static OnlineLeaderboard Instance { get; private set; }

    public const string LeaderboardId = "top_thieves";

    // One extra point of loot always beats any escape-time difference.
    public const long RankingFactor = 20001;
    public const int MaximumTimeMilliseconds = 20000;

    // Keeps the combined ranking value within exact double precision.
    private const long MaximumExactInteger = 9007199254740991L;

    private Task initializationTask;

    [Serializable]
    public class EntryData
    {
        public int version = 1;
        public string playerName;
        public long loot;
        public int escapeTimeMilliseconds;
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
        if (!PlayerNameSetup.HasValidName)
        {
            throw new InvalidOperationException(
                "Save a player name in the Leaderboard menu first.");
        }

        if (float.IsNaN(escapeSeconds) ||
            float.IsInfinity(escapeSeconds) ||
            escapeSeconds < 0f ||
            escapeSeconds >= 20f)
        {
            throw new ArgumentOutOfRangeException(nameof(escapeSeconds));
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

        var data = new EntryData
        {
            playerName = PlayerNameSetup.SavedName,
            loot = loot,
            escapeTimeMilliseconds = milliseconds
        };

        long rankingValue =
            loot * RankingFactor +
            (MaximumTimeMilliseconds - milliseconds);

        await EnsureReadyAsync();

        await LeaderboardsService.Instance.AddPlayerScoreAsync(
            LeaderboardId,
            rankingValue,
            new AddPlayerScoreOptions
            {
                Metadata = data
            });
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}