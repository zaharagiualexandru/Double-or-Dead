using System;
using Newtonsoft.Json;
using UnityEngine;
using Unity.Services.Leaderboards;

public class LeaderboardConnectionTest : MonoBehaviour
{
#if UNITY_EDITOR
    private bool testing;

    [ContextMenu("Test Direct Score Write")]
    private async void TestDirectScoreWrite()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Enter Play Mode from MainMenu first.");
            return;
        }

        if (testing)
            return;

        testing = true;

        try
        {
            OnlineLeaderboard manager = OnlineLeaderboard.Instance;

            if (manager == null)
                throw new InvalidOperationException(
                    "OnlineLeaderboard is missing. Start from MainMenu.");

            await manager.EnsureReadyAsync();

            var existing = await LeaderboardsService.Instance
                .GetPlayerScoreAsync(
                    OnlineLeaderboard.LeaderboardId,
                    new GetPlayerScoreOptions
                    {
                        IncludeMetadata = true
                    });

            if (existing == null ||
                string.IsNullOrEmpty(existing.Metadata))
            {
                Debug.LogWarning(
                    "Test cancelled: this player needs an existing " +
                    "score with metadata.");
                return;
            }

            object metadata =
                JsonConvert.DeserializeObject<object>(existing.Metadata);

            if (metadata == null)
            {
                Debug.LogWarning("Test cancelled: metadata is empty.");
                return;
            }

            Debug.Log(
                "Existing score loaded. Attempting a DIRECT write. " +
                "Expected result: Forbidden / access denied.");

            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(
                    OnlineLeaderboard.LeaderboardId,
                    existing.Score,
                    new AddPlayerScoreOptions
                    {
                        Metadata = metadata
                    });

                Debug.LogError(
                    "TEST FAILED: the direct write was accepted. " +
                    "Check the deployed access-control rule.");
            }
            catch (Exception exception)
            {
                // A network failure is not proof of access control.
                // Inspect the actual error for Forbidden / authorization denial.
                Debug.LogWarning(
                    "Direct write returned an error. " +
                    "Confirm it says Forbidden or access denied:\n" +
                    exception);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "Test could not reach the direct-write step:\n" +
                exception);
        }
        finally
        {
            testing = false;
        }
    }
#endif
}