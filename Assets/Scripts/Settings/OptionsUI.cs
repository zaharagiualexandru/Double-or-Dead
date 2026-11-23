using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Value Labels")]
    [SerializeField] private TMP_Text sensitivityValue;
    [SerializeField] private TMP_Text masterVolumeValue;
    [SerializeField] private TMP_Text musicVolumeValue;
    [SerializeField] private TMP_Text sfxVolumeValue;

    [Header("Other Controls")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button resetDefaultsButton;

    private GameSettingsManager Settings =>
        GameSettingsManager.Instance;

    private void Awake()
    {
        ConfigureSlider(sensitivitySlider, 0.25f, 3f);
        ConfigureSlider(masterVolumeSlider, 0f, 1f);
        ConfigureSlider(musicVolumeSlider, 0f, 1f);
        ConfigureSlider(sfxVolumeSlider, 0f, 1f);

        if (fullscreenToggle != null)
            fullscreenToggle.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        sensitivitySlider.onValueChanged.AddListener(ChangeSensitivity);
        masterVolumeSlider.onValueChanged.AddListener(ChangeMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(ChangeMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(ChangeSfxVolume);
        resetDefaultsButton.onClick.AddListener(ResetDefaults);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(ChangeFullscreen);

        RefreshControls();
    }

    private void Start()
    {
        if (Settings == null)
        {
            Debug.LogError(
                "Add GameSettingsManager to the scene.", this);
            return;
        }

        RefreshControls();
    }

    private void LateUpdate()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
#endif
    }

    private void OnDisable()
    {
        sensitivitySlider.onValueChanged.RemoveListener(ChangeSensitivity);
        masterVolumeSlider.onValueChanged.RemoveListener(ChangeMasterVolume);
        musicVolumeSlider.onValueChanged.RemoveListener(ChangeMusicVolume);
        sfxVolumeSlider.onValueChanged.RemoveListener(ChangeSfxVolume);
        resetDefaultsButton.onClick.RemoveListener(ResetDefaults);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(ChangeFullscreen);

        if (Settings != null)
            Settings.SaveNow();
    }

    private void RefreshControls()
    {
        if (Settings == null)
            return;

        sensitivitySlider.SetValueWithoutNotify(Settings.Sensitivity);
        masterVolumeSlider.SetValueWithoutNotify(Settings.MasterVolume);
        musicVolumeSlider.SetValueWithoutNotify(Settings.MusicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(Settings.SfxVolume);

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(Settings.Fullscreen);

        sensitivityValue.text = $"{Settings.Sensitivity:F2}x";
        masterVolumeValue.text = Percentage(Settings.MasterVolume);
        musicVolumeValue.text = Percentage(Settings.MusicVolume);
        sfxVolumeValue.text = Percentage(Settings.SfxVolume);
    }

    private void ChangeSensitivity(float value)
    {
        if (Settings == null)
            return;

        Settings.SetSensitivity(value);
        sensitivityValue.text = $"{Settings.Sensitivity:F2}x";
    }

    private void ChangeMasterVolume(float value)
    {
        if (Settings == null)
            return;

        Settings.SetMasterVolume(value);
        masterVolumeValue.text = Percentage(Settings.MasterVolume);
    }

    private void ChangeMusicVolume(float value)
    {
        if (Settings == null)
            return;

        Settings.SetMusicVolume(value);
        musicVolumeValue.text = Percentage(Settings.MusicVolume);
    }

    private void ChangeSfxVolume(float value)
    {
        if (Settings == null)
            return;

        Settings.SetSfxVolume(value);
        sfxVolumeValue.text = Percentage(Settings.SfxVolume);
    }

    private void ChangeFullscreen(bool value)
    {
        if (Settings != null)
            Settings.SetFullscreen(value);
    }

    private void ResetDefaults()
    {
        if (Settings == null)
            return;

        Settings.ResetToDefaults();
        RefreshControls();
    }

    private static string Percentage(float value)
    {
        return $"{Mathf.RoundToInt(value * 100f)}%";
    }

    private static void ConfigureSlider(
        Slider slider, float minimum, float maximum)
    {
        slider.wholeNumbers = false;
        slider.minValue = minimum;
        slider.maxValue = maximum;
    }
}