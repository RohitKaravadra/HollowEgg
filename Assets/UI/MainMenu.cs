
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
    [SerializeField] private OptionsPanel _OptionsPanel;
    [SerializeField] private GameObject _CreditsPanel;

    enum Panels
    {
        Main,
        Options,
        Credits
    }

    Panels _CurrentPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start() => SetPanel(Panels.Main);

    private void OnEnable()
    {
        _PlayButton.onClick.AddListener(OnPlayButton);
        _OptionsButton.onClick.AddListener(OnOptionsButton);
        _CreditsButton.onClick.AddListener(OnCreditsButton);
        _QuitButton.onClick.AddListener(OnQuitButton);
        _CrossButton.onClick.AddListener(OnCross);
    }

    private void OnDisable()
    {
        _PlayButton.onClick.RemoveListener(OnPlayButton);
        _OptionsButton.onClick.RemoveListener(OnOptionsButton);
        _CreditsButton.onClick.RemoveListener(OnCreditsButton);
        _QuitButton.onClick.RemoveListener(OnQuitButton);
        _CrossButton.onClick.RemoveListener(OnCross);
    }

    private void SetPanel(Panels panel)
    {
        _CurrentPanel = panel;
        _MainPanel.SetActive(panel == Panels.Main);
        _OptionsPanel.Enabled = panel == Panels.Options;
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
