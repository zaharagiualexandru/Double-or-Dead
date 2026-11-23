using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup musicOutput;

    [Header("Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "SampleScene";

    [Header("Starting Volumes")]
    [SerializeField, Range(0f, 1f)]
    private float mainMenuVolume = 0.4f;

    [SerializeField, Range(0f, 1f)]
    private float gameplayVolume = 0.3f;

    private AudioSource musicSource;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
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

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.outputAudioMixerGroup = musicOutput;

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (musicOutput == null)
        {
            Debug.LogWarning(
                "Assign the Music mixer group on MusicManager " +
                "so the Music Volume setting controls it.",
                this);
        }
    }

    private void Start()
    {
        if (Instance != this)
            return;

        // Also supports starting directly in the gameplay scene.
        PlayForScene(SceneManager.GetActiveScene().name, false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this || mode != LoadSceneMode.Single)
            return;

        // Restart the gameplay track for each new run.
        PlayForScene(scene.name, true);
    }

    private void PlayForScene(string sceneName, bool restart)
    {
        if (sceneName == mainMenuSceneName)
        {
            PlayTrack(mainMenuMusic, mainMenuVolume, restart);
        }
        else if (sceneName == gameplaySceneName)
        {
            PlayTrack(gameplayMusic, gameplayVolume, restart);
        }
        else
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }

    private void PlayTrack(AudioClip clip, float volume, bool restart)
    {
        if (clip == null)
        {
            musicSource.Stop();
            musicSource.clip = null;
            return;
        }

        musicSource.volume = volume;

        if (!restart &&
            musicSource.clip == clip &&
            musicSource.isPlaying)
        {
            return;
        }

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }
}