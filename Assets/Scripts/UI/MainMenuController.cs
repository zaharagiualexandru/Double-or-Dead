using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject leaderboardPanel;

    [Header("Player Name")]
    [SerializeField] private PlayerNameSetup playerNameSetup;

    [Header("Transition")]
    [SerializeField] private ScreenFade screenFade;

    [Header("Options")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Desktop Controls")]
    [SerializeField] private GameObject exitGameButton;

    [Header("Gameplay")]
    [SerializeField] private string gameplaySceneName = "SampleScene";

    private bool loading;

    private ScreenFade Fade =>
        ScreenFade.Instance != null
            ? ScreenFade.Instance
            : screenFade;

    private bool IsBusy =>
        loading || (Fade != null && Fade.IsBusy);

    private void Awake()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        if (exitGameButton != null)
            exitGameButton.SetActive(false);
#endif

        if (fullscreenToggle != null)
        {
            fullscreenToggle.gameObject.SetActive(true);

            bool fullscreen = GameSettingsManager.Instance != null
                ? GameSettingsManager.Instance.Fullscreen
                : Screen.fullScreen;

            fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
        }

        SetPanel(homePanel);
    }

    public void Play()
    {
        if (IsBusy)
            return;

        if (!PlayerNameSetup.HasValidName)
        {
            SwitchPanel(leaderboardPanel, () =>
            {
                if (playerNameSetup != null)
                {
                    playerNameSetup.ShowNameRequired();
                }
                else
                {
                    Debug.LogError(
                        "Assign PlayerNameSetup on MainMenuManager.",
                        this);
                }
            });

            return;
        }

        if (Fade == null)
        {
            Debug.LogError(
                "Assign ScreenFade on MainMenuManager.",
                this);
            return;
        }

        loading = Fade.LoadSceneWithFade(gameplaySceneName);
    }

    public void ShowHome()
    {
        SwitchPanel(homePanel);
    }

    public void ShowOptions()
    {
        SwitchPanel(optionsPanel);
    }

    public void ShowHowToPlay()
    {
        SwitchPanel(howToPlayPanel);
    }

    public void ShowLeaderboard()
    {
        SwitchPanel(leaderboardPanel, () =>
        {
            if (playerNameSetup != null)
                playerNameSetup.ShowSavedName();
        });
    }

    private void SwitchPanel(GameObject panel, Action onOpened = null)
    {
        if (IsBusy || panel == null)
            return;

        if (panel.activeSelf)
        {
            onOpened?.Invoke();
            return;
        }

        if (Fade != null)
        {
            Fade.FadeAndRun(() =>
            {
                SetPanel(panel);
                onOpened?.Invoke();
            });
        }
        else
        {
            SetPanel(panel);
            onOpened?.Invoke();
        }
    }

    private void SetPanel(GameObject panel)
    {
        homePanel.SetActive(panel == homePanel);
        optionsPanel.SetActive(panel == optionsPanel);
        howToPlayPanel.SetActive(panel == howToPlayPanel);
        leaderboardPanel.SetActive(panel == leaderboardPanel);
    }

    public void SetFullscreen(bool enabled)
    {
        if (IsBusy)
            return;

        // Keep this method compatible with existing button events.
        if (GameSettingsManager.Instance != null)
            GameSettingsManager.Instance.SetFullscreen(enabled);
    }

    public void QuitGame()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return;
#else
        if (IsBusy)
            return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
#endif
    }
}