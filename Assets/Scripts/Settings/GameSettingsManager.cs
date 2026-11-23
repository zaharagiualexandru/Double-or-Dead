using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-100)]
public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    private const string SensitivityKey = "Settings.MouseSensitivity";
    private const string MasterKey = "Settings.MasterVolume";
    private const string MusicKey = "Settings.MusicVolume";
    private const string SfxKey = "Settings.SfxVolume";
    private const string FullscreenKey = "Settings.Fullscreen";

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    public float Sensitivity { get; private set; } = 1f;
    public float MasterVolume { get; private set; } = 1f;
    public float MusicVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;

    private bool savedFullscreen = true;

    public bool Fullscreen
    {
        get
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Reflect browser changes, including leaving with Escape.
            return Screen.fullScreen;
#else
            return savedFullscreen;
#endif
        }
    }

    public static float MouseSensitivityMultiplier =>
        Instance != null ? Instance.Sensitivity : 1f;

    private bool started;
    private bool savePending;
    private float saveAt;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Sensitivity = ReadFloat(SensitivityKey, 1f, 0.25f, 3f);
        MasterVolume = ReadFloat(MasterKey, 1f, 0f, 1f);
        MusicVolume = ReadFloat(MusicKey, 1f, 0f, 1f);
        SfxVolume = ReadFloat(SfxKey, 1f, 0f, 1f);
        savedFullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
    }

    private void Start()
    {
        if (Instance != this)
            return;

        started = true;
        ApplyAudio();
        ApplyDesktopFullscreen();
    }

    private void Update()
    {
        if (savePending && Time.unscaledTime >= saveAt)
            SaveNow();
    }

    public void SetSensitivity(float value)
    {
        Sensitivity = Mathf.Clamp(value, 0.25f, 3f);
        QueueSave();
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        ApplyAudio();
        QueueSave();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        ApplyAudio();
        QueueSave();
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        ApplyAudio();
        QueueSave();
    }

    public void SetFullscreen(bool value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Called by the player's fullscreen control.
        // Do not save or automatically restore browser fullscreen.
        Screen.fullScreen = value;
#else
        savedFullscreen = value;
        ApplyDesktopFullscreen();
        QueueSave();
#endif
    }

    public void ResetToDefaults()
    {
        Sensitivity = 1f;
        MasterVolume = 1f;
        MusicVolume = 1f;
        SfxVolume = 1f;

#if !UNITY_WEBGL || UNITY_EDITOR
        savedFullscreen = true;
        ApplyDesktopFullscreen();
#endif

        ApplyAudio();
        QueueSave();
        SaveNow();
    }

    private void ApplyAudio()
    {
        if (!started)
            return;

        if (audioMixer == null)
        {
            Debug.LogError(
                "Assign GameAudioMixer on GameSettingsManager.", this);
            return;
        }

        SetMixerVolume("MasterVolume", MasterVolume);
        SetMixerVolume("MusicVolume", MusicVolume);
        SetMixerVolume("SfxVolume", SfxVolume);
    }

    private void SetMixerVolume(string parameter, float value)
    {
        float decibels = value <= 0f
            ? -80f
            : Mathf.Clamp(20f * Mathf.Log10(value), -80f, 0f);

        if (!audioMixer.SetFloat(parameter, decibels))
        {
            Debug.LogError(
                $"Audio Mixer parameter '{parameter}' was not found.",
                this);
        }
    }

    private void ApplyDesktopFullscreen()
    {
#if !UNITY_EDITOR && !UNITY_WEBGL
        Screen.fullScreenMode = savedFullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;
#endif
    }

    private void QueueSave()
    {
        PlayerPrefs.SetFloat(SensitivityKey, Sensitivity);
        PlayerPrefs.SetFloat(MasterKey, MasterVolume);
        PlayerPrefs.SetFloat(MusicKey, MusicVolume);
        PlayerPrefs.SetFloat(SfxKey, SfxVolume);

#if !UNITY_WEBGL || UNITY_EDITOR
        PlayerPrefs.SetInt(FullscreenKey, savedFullscreen ? 1 : 0);
#endif

        savePending = true;
        saveAt = Time.unscaledTime + 0.5f;
    }

    public void SaveNow()
    {
        if (!savePending)
            return;

        PlayerPrefs.Save();
        savePending = false;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && Instance == this)
            SaveNow();
    }

    private void OnApplicationQuit()
    {
        if (Instance == this)
            SaveNow();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SaveNow();
        Instance = null;
    }

    private static float ReadFloat(
        string key, float fallback, float minimum, float maximum)
    {
        float value = PlayerPrefs.GetFloat(key, fallback);

        if (float.IsNaN(value) || float.IsInfinity(value))
            return fallback;

        return Mathf.Clamp(value, minimum, maximum);
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }
}