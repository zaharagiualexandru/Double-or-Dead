using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("Output")]
    [SerializeField] private AudioMixerGroup sfxOutput;

    [Header("Pickups")]
    [SerializeField] private AudioClip treasureClip;
    [SerializeField] private AudioClip keyClip;
    [SerializeField] private AudioClip crownClip;

    [Header("Run")]
    [SerializeField] private AudioClip countdownClip;
    [SerializeField] private AudioClip escapeClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip doorClip;

    [Header("Names")]
    [SerializeField] private AudioClip nameAcceptedClip;
    [SerializeField] private AudioClip nameRejectedClip;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float treasureVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float keyVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float crownVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float countdownVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float resultVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float doorVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float nameVolume = 0.6f;

    private AudioSource effectsSource;
    private AudioSource countdownSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        effectsSource = CreateSource();
        countdownSource = CreateSource();

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (sfxOutput == null)
        {
            Debug.LogError(
                "Assign the SFX mixer group on GameAudio.", this);
        }
    }

    private AudioSource CreateSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 1f;
        source.outputAudioMixerGroup = sfxOutput;
        return source;
    }

    public void PlayTreasure()
    {
        Play(treasureClip, treasureVolume);
    }

    public void PlayKey()
    {
        Play(keyClip, keyVolume);
    }

    public void PlayCrown()
    {
        Play(crownClip, crownVolume);
    }

    public void PlayDoor()
    {
        Play(doorClip, doorVolume);
    }

    public void PlayNameAccepted()
    {
        Play(nameAcceptedClip, nameVolume);
    }

    public void PlayNameRejected()
    {
        Play(nameRejectedClip, nameVolume);
    }

    public void PlayCountdown()
    {
        if (countdownClip == null)
            return;

        // A new tick replaces the previous tick instead of stacking.
        countdownSource.Stop();
        countdownSource.PlayOneShot(countdownClip, countdownVolume);
    }

    public void StopCountdown()
    {
        countdownSource.Stop();
    }

    public void PlayEscape()
    {
        StopCountdown();
        Play(escapeClip, resultVolume);
    }

    public void PlayDeath()
    {
        StopCountdown();
        Play(deathClip, resultVolume);
    }

    private void Play(AudioClip clip, float volume)
    {
        if (clip != null)
            effectsSource.PlayOneShot(clip, volume);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Don't carry sounds from the previous run into a new scene.
        effectsSource.Stop();
        countdownSource.Stop();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }
}