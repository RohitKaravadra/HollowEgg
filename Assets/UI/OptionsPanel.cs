using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    [SerializeField] private Slider _MasterVolumeSlider;
    [SerializeField] private Slider _MusicVolumeSlider;
    [SerializeField] private Slider _SFXVolumeSlider;
    [SerializeField] private AudioMixer _AudioMixer;

    public bool Enabled { get => gameObject.activeSelf; set => gameObject.SetActive(value); }

    private void Awake() => LoadSettings();

    private void OnEnable()
    {
        _MasterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderUpdate);
        _MusicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderUpdate);
        _SFXVolumeSlider.onValueChanged.AddListener(OnSFXVolumeSliderUpdate);
    }

    private void OnDisable()
    {
        _MasterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderUpdate);
        _MusicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeSliderUpdate);
        _SFXVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeSliderUpdate);
    }

    private void LoadSettings()
    {
        _AudioMixer.GetFloat("MasterVolume", out float masterDb);
        float masterPercent = AudioManager.DBToPercent(masterDb);
        _MasterVolumeSlider.SetValueWithoutNotify(masterPercent);

        _AudioMixer.GetFloat("MusicVolume", out float musicDb);
        float musicPercent = AudioManager.DBToPercent(musicDb);
        _MusicVolumeSlider.SetValueWithoutNotify(musicPercent);

        _AudioMixer.GetFloat("SFXVolume", out float sfxDb);
        float sfxPercent = AudioManager.DBToPercent(sfxDb);
        _SFXVolumeSlider.SetValueWithoutNotify(sfxPercent);
    }

    private void OnMasterVolumeSliderUpdate(float percent) => _AudioMixer.SetFloat("MasterVolume", AudioManager.PercentToDB(percent));
    private void OnMusicVolumeSliderUpdate(float percent) => _AudioMixer.SetFloat("MusicVolume", AudioManager.PercentToDB(percent));
    private void OnSFXVolumeSliderUpdate(float percent) => _AudioMixer.SetFloat("SFXVolume", AudioManager.PercentToDB(percent));

}
