using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class MenuAudio : MonoBehaviour
{
    public static MenuAudio Instance { get; private set; }

    [Header("Output")]
    [SerializeField] private AudioMixerGroup sfxOutput;

    [Header("Clips")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip hoverClip;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float clickVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float hoverVolume = 0.3f;

    private AudioSource source;
    private float nextHoverTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 1f;
        source.outputAudioMixerGroup = sfxOutput;

        if (sfxOutput == null)
        {
            Debug.LogError(
                "Assign the SFX mixer group on MenuAudio.", this);
        }
    }

    public void PlayClick()
    {
        if (clickClip != null)
            source.PlayOneShot(clickClip, clickVolume);
    }

    public void PlayHover()
    {
        // Prevent rapid mouse movement from stacking too many sounds.
        if (hoverClip == null || Time.unscaledTime < nextHoverTime)
            return;

        nextHoverTime = Time.unscaledTime + 0.08f;
        source.PlayOneShot(hoverClip, hoverVolume);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }
}