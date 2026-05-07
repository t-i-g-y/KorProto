using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private string masterVolumeParameter = "MasterVolume";

    private const string FullscreenPrefKey = "settings.fullscreen";
    private const string MasterVolumePrefKey = MusicManager.MasterVolumePrefKey;

    private void Start()
    {
        LoadSettings();
        BindUI();
    }

    private void BindUI()
    {
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
    }

    private void LoadSettings()
    {
        bool fullscreen = PlayerPrefs.GetInt(FullscreenPrefKey, Screen.fullScreen ? 1 : 0) == 1;
        float volume = MusicManager.LoadSavedMasterVolume();

        ApplyFullscreen(fullscreen);

        if (MusicManager.Instance != null)
            MusicManager.Instance.ApplyMasterVolume(volume);
        else
            ApplyMasterVolume(volume);

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = fullscreen;

        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(volume);
    }

    public void SetFullscreen(bool fullscreen)
    {
        ApplyFullscreen(fullscreen);
        PlayerPrefs.SetInt(FullscreenPrefKey, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float sliderVolume)
    {
        float clamped = Mathf.Clamp01(sliderVolume);

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetMasterVolume(clamped);
            return;
        }

        ApplyMasterVolume(clamped);
        PlayerPrefs.SetFloat(MasterVolumePrefKey, clamped);
        PlayerPrefs.Save();
    }

    private void ApplyFullscreen(bool fullscreen)
    {
        Screen.fullScreenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreen = fullscreen;
    }

    private void ApplyMasterVolume(float sliderVolume)
    {
        if (audioMixer == null)
            return;

        float clamped = Mathf.Clamp(sliderVolume, 0.0001f, 1f);
        float decibel = Mathf.Log10(clamped) * 20f;
        audioMixer.SetFloat(masterVolumeParameter, decibel);
    }
}
