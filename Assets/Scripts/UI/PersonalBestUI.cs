using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;

public class PersonalBestUI : MonoBehaviour
{
    [SerializeField] private TMP_Text personalBestText;

    private bool refreshing;
    private int viewVersion;

    private void OnEnable()
    {
        StartCoroutine(AutoRefresh());
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        viewVersion++;
        refreshing = false;
    }

    private IEnumerator AutoRefresh()
    {
        // Let scene managers finish Awake first.
        yield return null;

        while (true)
        {
            Refresh();
            yield return new WaitForSecondsRealtime(30f);
        }
    }

    public async void Refresh()
    {
        if (refreshing || !isActiveAndEnabled)
            return;

        refreshing = true;
        int requestVersion = viewVersion;

        SetText("YOUR BEST RUN\nLoading...");

        try
        {
            OnlineLeaderboard manager = OnlineLeaderboard.Instance;

            if (manager == null)
            {
                throw new InvalidOperationException(
                    "OnlineLeaderboard is missing from the scene.");
            }

            await manager.EnsureReadyAsync();

            if (!IsCurrentRequest(requestVersion))
                return;

            string playerId = AuthenticationService.Instance.PlayerId;

            var entry = await LeaderboardsService.Instance
                .GetPlayerScoreAsync(
                    OnlineLeaderboard.LeaderboardId,
                    new GetPlayerScoreOptions
                    {
                        IncludeMetadata = true
                    });

            if (!IsCurrentRequest(requestVersion))
                return;

            if (!AuthenticationService.Instance.IsSignedIn ||
                AuthenticationService.Instance.PlayerId != playerId)
            {
                throw new InvalidOperationException(
                    "The signed-in player changed. Please reopen this panel.");
            }

            if (entry == null || string.IsNullOrEmpty(entry.Metadata))
            {
                throw new InvalidOperationException(
                    "The saved score has no run details.");
            }

            var data = JsonUtility.FromJson<OnlineLeaderboard.EntryData>(
                entry.Metadata);

            if (data == null ||
                data.version != 1 ||
                data.loot < 0 ||
                data.escapeTimeMilliseconds < 0 ||
                data.escapeTimeMilliseconds >
                    OnlineLeaderboard.MaximumTimeMilliseconds)
            {
                throw new InvalidOperationException(
                    "The saved run details are invalid.");
            }

            string loot = data.loot.ToString(
                "N0", CultureInfo.InvariantCulture);

            string time = (data.escapeTimeMilliseconds / 1000d).ToString(
                "F3", CultureInfo.InvariantCulture);

            SetText(
                "YOUR BEST RUN\n" +
                $"Rank: #{entry.Rank + 1}\n" +
                $"Loot: {loot}\n" +
                $"Time: {time}s");
        }
        catch (LeaderboardsException exception)
            when (exception.Reason ==
                  LeaderboardsExceptionReason.EntryNotFound)
        {
            if (!IsCurrentRequest(requestVersion))
                return;

            SetText(
                "YOUR BEST RUN\n" +
                "Rank: —\n" +
                "No score yet\n" +
                "Escape to set one!");
        }
        catch (Exception exception)
        {
            if (!IsCurrentRequest(requestVersion))
                return;

            SetText(
                "YOUR BEST RUN\n" +
                "Couldn't load stats.\n" +
                "Check your connection.\n" +
                "Retrying automatically...");

            Debug.LogWarning(
                $"Personal best refresh failed: {exception.Message}");
        }
        finally
        {
            if (this != null && requestVersion == viewVersion)
                refreshing = false;
        }
    }

    private bool IsCurrentRequest(int requestVersion)
    {
        return this != null &&
               isActiveAndEnabled &&
               requestVersion == viewVersion;
    }

    private void SetText(string message)
    {
        if (personalBestText != null)
            personalBestText.text = message;
    }
}