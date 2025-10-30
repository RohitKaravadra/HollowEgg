
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Button _ResumeButton;
    [SerializeField] Button _OptionsButton;
    [SerializeField] Button _MainMenuButton;
    [Space(5)]
    [SerializeField] GameObject _MainPanel;
    [SerializeField] GameObject _PausePanel;
    [SerializeField] GameObject _GameOverPanel;
    [SerializeField] OptionsPanel _OptionsPanel;
    [Space(5)]
    [SerializeField] HealthBar _HealthBar;

    public static System.Action<int, int> OnHealthUpdate;

    enum Panels
    {
        None,
        Pause,
        Options,
        GameOver
    }

    Panels _CurrentPanel;

    private void Awake()
    {
        SetPanel(Panels.None);
        _MainPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (_ResumeButton != null)
            _ResumeButton.onClick.AddListener(OnResumeButton);
        if (_OptionsButton != null)
            _OptionsButton.onClick.AddListener(OptionsPanel);
        if (_MainMenuButton != null)
            _MainMenuButton.onClick.AddListener(MainMenuButton);

        GameManager.OnBossDeathTriggered += GameOver;
        GameManager.OnGamePaused += OnGamePaused;

        OnHealthUpdate += _HealthBar.SetHealth;
        if (InputManager.HasInstance)
        {
            InputManager.Instance.UI.Cancel.performed += ctx => OnCancel();
        }
    }

    private void OnDisable()
    {
        if (_ResumeButton != null)
            _ResumeButton.onClick.RemoveListener(OnResumeButton);
        if (_OptionsButton != null)
            _OptionsButton.onClick.RemoveListener(OptionsPanel);
        if (_MainMenuButton != null)
            _MainMenuButton.onClick.RemoveListener(MainMenuButton);

        GameManager.OnBossDeathTriggered -= GameOver;
        GameManager.OnGamePaused -= OnGamePaused;

        OnHealthUpdate -= _HealthBar.SetHealth;
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
        _MainPanel.SetActive(panel != Panels.None && panel != Panels.GameOver);
        _GameOverPanel.SetActive(panel == Panels.GameOver);
    }

    private void OnResumeButton() => GameManager.OnGamePaused?.Invoke(false);

    private void OnGamePaused(bool paused)
    {
        if (paused)
            SetPanel(Panels.Pause);
        else
            SetPanel(Panels.None);
    }

    private void OnCancel()
    {
        if (_CurrentPanel == Panels.GameOver)
            return;

        switch (_CurrentPanel)
        {
            case Panels.None:
                GameManager.OnGamePaused?.Invoke(true);
                break;
            case Panels.Pause:
                GameManager.OnGamePaused?.Invoke(false);
                break;
            case Panels.Options:
                SetPanel(Panels.Pause);
                break;
            default:
                break;
        }
    }

    private void OptionsPanel() => SetPanel(Panels.Options);

    private void MainMenuButton() => SceneManager.LoadScene(0);

    private void GameOver()
    {
        InputManager.Instance.SetPlayerInput(false);
        SetPanel(Panels.GameOver);
    }

    public void CancelButton() => OnCancel();

    public void BackToMenu()
    {
        GameManager.OnGamePaused?.Invoke(false);
        InputManager.Instance.SetPlayerInput(false);
        SceneManager.LoadScene(0);
    }
}
