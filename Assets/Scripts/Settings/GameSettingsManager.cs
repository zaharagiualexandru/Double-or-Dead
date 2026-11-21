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
    public bool Fullscreen { get; private set; } = true;

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
        Fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
    }

    private void Start()
    {
        if (Instance != this)
            return;

        started = true;
        ApplyAudio();
        ApplyFullscreen();
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
        Fullscreen = value;
        ApplyFullscreen();
        QueueSave();
    }

    public void ResetToDefaults()
    {
        Sensitivity = 1f;
        MasterVolume = 1f;
        MusicVolume = 1f;
        SfxVolume = 1f;
        Fullscreen = true;

        ApplyAudio();
        ApplyFullscreen();

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
                $"Audio Mixer parameter '{parameter}' was not found.", this);
        }
    }

    private void ApplyFullscreen()
    {
#if !UNITY_EDITOR
        Screen.fullScreenMode = Fullscreen
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
        PlayerPrefs.SetInt(FullscreenKey, Fullscreen ? 1 : 0);

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