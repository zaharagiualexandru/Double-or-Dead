using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Unity.Services.Leaderboards;

public class LeaderboardBoardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform entriesParent;
    [SerializeField] private LeaderboardRowUI rowPrefab;
    [SerializeField] private TMP_Text statusText;

    [Header("Settings")]
    [SerializeField, Range(1, 100)] private int entriesToShow = 100;
    [SerializeField] private string developerPlayerId;

    [Header("Temporary Editor Preview")]
    [SerializeField] private bool previewMode;

    private bool refreshing;
    private int viewVersion;
    private bool showingPreview;

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

        // Ignore responses belonging to an earlier opening of this panel.
        viewVersion++;
        refreshing = false;
    }

    private IEnumerator AutoRefresh()
    {
        // Allow scene objects to finish their Awake methods first.
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
                    Limit = entriesToShow,
                    IncludeMetadata = true
                });

            if (!IsCurrentRequest(requestVersion))
                return;

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
                    string.IsNullOrEmpty(data.playerName))
                {
                    continue;
                }

                LeaderboardRowUI row =
                    Instantiate(rowPrefab, entriesParent);

                row.gameObject.SetActive(true);

                bool isDeveloper =
                    !string.IsNullOrWhiteSpace(developerPlayerId) &&
                    entry.PlayerId == developerPlayerId.Trim();

                row.SetEntry(
                    entry.Rank + 1,
                    data.playerName,
                    data.loot,
                    data.escapeTimeMilliseconds,
                    isDeveloper);

                displayed++;
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

        StopAllCoroutines();

        // Stop online refreshes without disabling this component.
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
        SetStatus("TEST PREVIEW — not online scores");
    }
#endif
}