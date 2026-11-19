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

        SetPanel(homePanel);

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        }
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

        Screen.fullScreenMode = enabled
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;
    }

    public void QuitGame()
    {
        if (IsBusy)
            return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}