using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ScreenFade : MonoBehaviour
{
    public static ScreenFade Instance { get; private set; }

    [SerializeField] private CanvasGroup blackOverlay;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.5f;

    public bool IsBusy { get; private set; }

    private bool loadingScene;

    private readonly List<Behaviour> pausedComponents =
        new List<Behaviour>();

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
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

        if (blackOverlay == null)
        {
            Debug.LogError("Assign BlackOverlay to ScreenFade.", this);
            enabled = false;
            return;
        }

        Instance = this;

        // This script must be on the root TransitionCanvas.
        DontDestroyOnLoad(gameObject);

        blackOverlay.alpha = 0f;
        blackOverlay.blocksRaycasts = false;
        blackOverlay.interactable = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void FadeAndRun(Action action)
    {
        if (IsBusy)
            return;

        StartCoroutine(PanelTransition(action));
    }

    public bool LoadSceneWithFade(string sceneName)
    {
        if (IsBusy)
            return false;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"Scene '{sceneName}' is not available. " +
                "Check its name and the build scene list.",
                this
            );

            return false;
        }

        StartCoroutine(SceneTransition(sceneName));
        return true;
    }

    private void BeginTransition()
    {
        IsBusy = true;
        blackOverlay.blocksRaycasts = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void EndTransition()
    {
        blackOverlay.blocksRaycasts = false;
        IsBusy = false;
    }

    private IEnumerator PanelTransition(Action action)
    {
        BeginTransition();

        yield return FadeTo(1f);

        action?.Invoke();

        yield return null;
        yield return FadeTo(0f);

        EndTransition();
    }

    private IEnumerator SceneTransition(string sceneName)
    {
        BeginTransition();

        Time.timeScale = 1f;

        yield return FadeTo(1f);

        loadingScene = true;

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Single
        );

        while (!operation.isDone)
            yield return null;

        // Allow the new scene to finish its initial setup behind black.
        yield return null;

        yield return FadeTo(0f);

        bool resumePlayer = false;

        foreach (Behaviour component in pausedComponents)
        {
            if (component == null)
                continue;

            component.enabled = true;

            if (component is FirstPersonController)
                resumePlayer = true;
        }

        pausedComponents.Clear();

        if (resumePlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        loadingScene = false;
        EndTransition();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!loadingScene)
            return;

        pausedComponents.Clear();

        // sceneLoaded runs before the scene's first Update.
        // Block input and timer/restart handling until the fade finishes.
        foreach (FirstPersonController player in
                 FindObjectsByType<FirstPersonController>(
                     FindObjectsSortMode.None))
        {
            PauseComponent(player);
        }

        foreach (RunManager manager in
                 FindObjectsByType<RunManager>(
                     FindObjectsSortMode.None))
        {
            PauseComponent(manager);
        }
    }

    private void PauseComponent(Behaviour component)
    {
        // Preserve components that were already disabled.
        if (!component.enabled)
            return;

        pausedComponents.Add(component);
        component.enabled = false;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startingAlpha = blackOverlay.alpha;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            blackOverlay.alpha = Mathf.Lerp(
                startingAlpha,
                targetAlpha,
                Mathf.SmoothStep(0f, 1f, progress)
            );

            yield return null;
        }

        blackOverlay.alpha = targetAlpha;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }
}