
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Button _ResumeButton;
    [SerializeField] Button _OptionsButton;
    [SerializeField] Button _QuitButton;
    [SerializeField] Button _CrossButton;
    [Space(5)]
    [SerializeField] GameObject _MainPanel;
    [SerializeField] GameObject _PausePanel;
    [SerializeField] OptionsPanel _OptionsPanel;

    enum Panels
    {
        None,
        Pause,
        Options
    }

    Panels _CurrentPanel;

    private void Awake()
    {
        _CurrentPanel = Panels.None;
        _MainPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (_ResumeButton != null) _ResumeButton.onClick.AddListener(Close);
        if (_OptionsButton != null) _OptionsButton.onClick.AddListener(OptionsPanel);
        if (_QuitButton != null) _QuitButton.onClick.AddListener(QuitButton);
        if (_CrossButton != null) _CrossButton.onClick.AddListener(OnCancel);
        if (InputManager.HasInstance)
        {
            InputManager.Instance.UI.Cancel.performed += ctx => OnCancel();
        }
    }

    private void OnDisable()
    {
        if (_ResumeButton != null) _ResumeButton.onClick.RemoveListener(Close);
        if (_OptionsButton != null) _OptionsButton.onClick.RemoveListener(OptionsPanel);
        if (_QuitButton != null) _QuitButton.onClick.RemoveListener(QuitButton);
        if (_CrossButton != null) _CrossButton.onClick.RemoveListener(OnCancel);
        if (InputManager.HasInstance)
        {
            InputManager.Instance.UI.Cancel.performed -= ctx => OnCancel();
        }
    }

    private void SetPanel(Panels panel)
    {
        _CurrentPanel = panel;
        _PausePanel.SetActive(panel == Panels.Pause);
        _OptionsPanel.Enabled = panel == Panels.Options;
        _MainPanel.SetActive(panel != Panels.None);
        _CrossButton.gameObject.SetActive(panel != Panels.None);
    }

    private void Close()
    {
        Time.timeScale = 1;
        InputManager.Instance.SetPlayerInput(true);
        SetPanel(Panels.None);
    }

    private void Open()
    {
        Time.timeScale = 0;
        InputManager.Instance.SetPlayerInput(false);
        SetPanel(Panels.Pause);
    }

    private void OnCancel()
    {
        switch (_CurrentPanel)
        {
            case Panels.None:
                Open();
                break;
            case Panels.Pause:
                Close();
                break;
            case Panels.Options:
                SetPanel(Panels.Pause);
                break;
            default:
                break;
        }
    }

    private void OptionsPanel() => SetPanel(Panels.Options);

    private void QuitButton()
    {
        Time.timeScale = 1;
        InputManager.Instance.SetPlayerInput(false);
        SceneManager.LoadScene(0);
    }
}
