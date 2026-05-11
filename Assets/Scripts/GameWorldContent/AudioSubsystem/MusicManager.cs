using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    public const string MasterVolumePrefKey = "settings.masterVolume";

    [SerializeField] private AudioClip defaultMusic;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup outputGroup;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private bool playOnStart = true;

    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = volume;
        musicSource.outputAudioMixerGroup = outputGroup;

        ApplyMasterVolume(LoadSavedMasterVolume());
    }

    private void Start()
    {
        if (playOnStart && defaultMusic != null)
            Play(defaultMusic);
    }

    public void Play(AudioClip clip)
    {
        if (clip == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void Stop()
    {
        musicSource.Stop();
    }

    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);
        musicSource.volume = volume;
    }

    public void SetMasterVolume(float sliderVolume)
    {
        float clamped = ApplyMasterVolume(sliderVolume);
        PlayerPrefs.SetFloat(MasterVolumePrefKey, clamped);
        PlayerPrefs.Save();
    }

    public static float LoadSavedMasterVolume()
    {
        return PlayerPrefs.GetFloat(MasterVolumePrefKey, 1f);
    }

    public float ApplyMasterVolume(float sliderVolume)
    {
        float clamped = Mathf.Clamp01(sliderVolume);
        volume = clamped;

        if (musicSource != null && audioMixer == null)
            musicSource.volume = clamped;

        if (audioMixer != null)
        {
            float mixerVolume = Mathf.Clamp(clamped, 0.0001f, 1f);
            float decibel = Mathf.Log10(mixerVolume) * 20f;
            audioMixer.SetFloat(masterVolumeParameter, decibel);
        }

        return clamped;
    }
}
