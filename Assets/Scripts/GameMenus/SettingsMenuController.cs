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
    private const string MasterVolumePrefKey = "settings.masterVolume";

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
        float volume = PlayerPrefs.GetFloat(MasterVolumePrefKey, 1f);

        ApplyFullscreen(fullscreen);
        ApplyMasterVolume(volume);

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = fullscreen;

        if (masterVolumeSlider != null)
            masterVolumeSlider.value = volume;
    }

    public void SetFullscreen(bool fullscreen)
    {
        ApplyFullscreen(fullscreen);
        PlayerPrefs.SetInt(FullscreenPrefKey, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float sliderVolume)
    {
        ApplyMasterVolume(sliderVolume);
        PlayerPrefs.SetFloat(MasterVolumePrefKey, sliderVolume);
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
