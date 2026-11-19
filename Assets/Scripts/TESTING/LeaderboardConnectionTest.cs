using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;

public class LeaderboardConnectionTest : MonoBehaviour
{
    private async void Start()
    {
        try
        {
            Debug.Log("Connecting to leaderboard...");

            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log("Player signed in successfully.");

            var response = await LeaderboardsService.Instance.GetScoresAsync(
                "top_thieves",
                new GetScoresOptions
                {
                    Limit = 10,
                    IncludeMetadata = true
                });

            Debug.Log(
                $"Leaderboard connected! Retrieved {response.Results.Count} entries.");
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Leaderboard connection failed:\n{exception}");
        }
    }
}