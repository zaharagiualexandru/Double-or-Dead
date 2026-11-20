using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.CloudCode;
using Unity.Services.Leaderboards;

public class LeaderboardBoardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform entriesParent;
    [SerializeField] private LeaderboardRowUI rowPrefab;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Settings")]
    [SerializeField, Range(1, 100)] private int entriesToShow = 100;
    [SerializeField] private string developerPlayerId;

    [Header("Temporary Editor Preview")]
    [SerializeField] private bool previewMode;

    private bool refreshing;
    private int viewVersion;
    private bool showingPreview;

    [Serializable]
    public class RegisteredNamesResponse
    {
        public RegisteredPlayer[] players;
    }

    [Serializable]
    public class RegisteredPlayer
    {
        public string playerId;
        public string playerName;
    }

    private void Awake()
    {
        if (scrollRect == null && entriesParent != null)
            scrollRect = entriesParent.GetComponentInParent<ScrollRect>();
    }

    private void OnEnable()
    {
        showingPreview = false;

#if UNITY_EDITOR
        if (previewMode)
        {
            showingPreview = true;
            StartCoroutine(ShowPreviewOnStart());
            return;
        }
#endif

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
        yield return null;

        while (true)
        {
            Refresh();
            yield return new WaitForSecondsRealtime(30f);
        }
    }

    public async void Refresh()
    {
        if (showingPreview || refreshing || !isActiveAndEnabled)
            return;

        refreshing = true;
        int requestVersion = viewVersion;

        SetStatus("Loading leaderboard...");

        try
        {
            if (entriesParent == null || rowPrefab == null)
                throw new InvalidOperationException(
                    "Assign Entries Parent and Row Prefab.");

            OnlineLeaderboard manager = OnlineLeaderboard.Instance;

            if (manager == null)
                throw new InvalidOperationException(
                    "OnlineLeaderboard is missing from the scene.");

            await manager.EnsureReadyAsync();

            if (!IsCurrentRequest(requestVersion))
                return;

            var response = await LeaderboardsService.Instance.GetScoresAsync(
                OnlineLeaderboard.LeaderboardId,
                new GetScoresOptions
                {
                    Limit = Mathf.Clamp(entriesToShow, 1, 100),
                    IncludeMetadata = true
                });

            if (!IsCurrentRequest(requestVersion))
                return;

            var playerIds = new List<string>();

            foreach (var entry in response.Results)
            {
                if (!string.IsNullOrEmpty(entry.PlayerId) &&
                    !playerIds.Contains(entry.PlayerId))
                {
                    playerIds.Add(entry.PlayerId);
                }
            }

            var names = new Dictionary<string, string>();

            if (playerIds.Count > 0)
            {
                var arguments = new Dictionary<string, object>
                {
                    {
                        "playerIdsJson",
                        JsonConvert.SerializeObject(playerIds)
                    }
                };

                var registered =
                    await CloudCodeService.Instance
                        .CallEndpointAsync<RegisteredNamesResponse>(
                            "GetRegisteredNames",
                            arguments);

                if (!IsCurrentRequest(requestVersion))
                    return;

                if (registered == null || registered.players == null)
                    throw new InvalidOperationException(
                        "The registered-name response was invalid.");

                foreach (var player in registered.players)
                {
                    if (player != null &&
                        !string.IsNullOrEmpty(player.playerId) &&
                        PlayerNameSetup.IsValidName(player.playerName))
                    {
                        names[player.playerId] = player.playerName;
                    }
                }
            }

            // Capture the latest position after network requests finish.
            float scrollPosition = scrollRect != null
                ? scrollRect.verticalNormalizedPosition
                : 1f;

            ClearRows();

            int displayed = 0;

            foreach (var entry in response.Results)
            {
                OnlineLeaderboard.EntryData data;

                try
                {
                    if (string.IsNullOrEmpty(entry.Metadata))
                        continue;

                    data = JsonUtility.FromJson<
                        OnlineLeaderboard.EntryData>(entry.Metadata);
                }
                catch (Exception)
                {
                    continue;
                }

                if (data == null ||
                    data.version != 1 ||
                    data.loot < 0 ||
                    data.escapeTimeMilliseconds < 0 ||
                    data.escapeTimeMilliseconds >
                        OnlineLeaderboard.MaximumTimeMilliseconds)
                {
                    continue;
                }

                // Never reuse an old name that another player may now own.
                string displayName =
                    names.TryGetValue(entry.PlayerId, out string currentName)
                        ? currentName
                        : "Unnamed";

                LeaderboardRowUI row =
                    Instantiate(rowPrefab, entriesParent);

                row.gameObject.SetActive(true);

                bool isDeveloper =
                    !string.IsNullOrWhiteSpace(developerPlayerId) &&
                    entry.PlayerId == developerPlayerId.Trim();

                row.SetEntry(
                    entry.Rank + 1,
                    displayName,
                    data.loot,
                    data.escapeTimeMilliseconds,
                    isDeveloper);

                displayed++;
            }

            Canvas.ForceUpdateCanvases();

            if (scrollRect != null)
            {
                scrollRect.StopMovement();
                scrollRect.verticalNormalizedPosition =
                    Mathf.Clamp01(scrollPosition);
            }

            SetStatus(displayed == 0
                ? "No scores yet. Escape to set the first!"
                : "");
        }
        catch (Exception exception)
        {
            if (!IsCurrentRequest(requestVersion))
                return;

            SetStatus("Couldn't refresh. Check your connection.");

            Debug.LogWarning(
                $"Leaderboard refresh failed: {exception.Message}");
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
               !showingPreview &&
               requestVersion == viewVersion;
    }

    private void ClearRows()
    {
        foreach (Transform child in entriesParent)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

#if UNITY_EDITOR
    private IEnumerator ShowPreviewOnStart()
    {
        yield return null;
        PreviewTestRows();
    }

    [ContextMenu("Preview 100 Test Rows")]
    private void PreviewTestRows()
    {
        if (!Application.isPlaying || !isActiveAndEnabled)
        {
            Debug.LogWarning(
                "Enter Play Mode with this component enabled first.");
            return;
        }

        if (entriesParent == null || rowPrefab == null)
        {
            Debug.LogWarning("Assign Entries Parent and Row Prefab.");
            return;
        }

        StopAllCoroutines();

        showingPreview = true;
        viewVersion++;
        refreshing = false;

        ClearRows();

        for (int i = 0; i < 100; i++)
        {
            LeaderboardRowUI row = Instantiate(rowPrefab, entriesParent);
            row.gameObject.SetActive(true);

            row.SetEntry(
                i + 1,
                $"Test{i + 1:000}",
                1500000L - i * 10000L,
                15000 + i * 40,
                false);
        }

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;
        }

        SetStatus("TEST PREVIEW — not online scores");
    }
#endif
}