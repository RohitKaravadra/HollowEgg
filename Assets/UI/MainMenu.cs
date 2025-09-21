
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button _PlayButton;
    [SerializeField] private Button _OptionsButton;
    [SerializeField] private Button _CreditsButton;
    [SerializeField] private Button _QuitButton;
    [SerializeField] private Button _CrossButton;
    [Space(5)]
    [SerializeField] private GameObject _MainPanel;
    [SerializeField] private GameObject _OptionsPanel;
    [SerializeField] private GameObject _CreditsPanel;
    [Space(5)]
    [SerializeField] private Slider _MasterVolumeSlider;
    [SerializeField] private Slider _MusicVolumeSlider;
    [SerializeField] private Slider _SFXVolumeSlider;
    [SerializeField] private AudioMixer _AudioMixer;

    enum Panels
    {
        Main,
        Options,
        Credits
    }

    Panels _CurrentPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetPanel(Panels.Main);
        LoadSettings();
    }

    private void OnEnable()
    {
        _PlayButton.onClick.AddListener(OnPlayButton);
        _OptionsButton.onClick.AddListener(OnOptionsButton);
        _CreditsButton.onClick.AddListener(OnCreditsButton);
        _QuitButton.onClick.AddListener(OnQuitButton);
        _CrossButton.onClick.AddListener(OnCross);

        _MasterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderUpdate);
        _MusicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderUpdate);
        _SFXVolumeSlider.onValueChanged.AddListener(OnSFXVolumeSliderUpdate);
    }

    private void OnDisable()
    {
        _PlayButton.onClick.RemoveListener(OnPlayButton);
        _OptionsButton.onClick.RemoveListener(OnOptionsButton);
        _CreditsButton.onClick.RemoveListener(OnCreditsButton);
        _QuitButton.onClick.RemoveListener(OnQuitButton);
        _CrossButton.onClick.RemoveListener(OnCross);

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

    private void SetPanel(Panels panel)
    {
        _CurrentPanel = panel;
        _MainPanel.SetActive(panel == Panels.Main);
        _OptionsPanel.SetActive(panel == Panels.Options);
        _CreditsPanel.SetActive(panel == Panels.Credits);
        _CrossButton.gameObject.SetActive(panel != Panels.Main);
    }

    private void OnPlayButton() => SceneManager.LoadScene(1);

    private void OnOptionsButton() => SetPanel(Panels.Options);

    private void OnCreditsButton() => SetPanel(Panels.Credits);

    private void OnCross()
    {
        switch (_CurrentPanel)
        {
            case Panels.Main:
                break;
            case Panels.Options:
                SetPanel(Panels.Main);
                break;
            case Panels.Credits:
                SetPanel(Panels.Main);
                break;
        }
    }

    private void OnQuitButton()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#elif !UNITY_WEBGL
        Application.Quit();
#endif
    }
}
