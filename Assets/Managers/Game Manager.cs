using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] float _RespawnDelay = 3f;

    public static Action OnPlayerDead;
    public static Action OnPlayerRespawn;
    public static Action OnBossFightTriggered;
    public static Action OnBossDeathTriggered;
    public static Action OnResetEnemies;
    public static Action<bool> OnGamePaused;

    public static bool HasInstance => Instance != null;
    public static GameManager Instance { get; private set; }

    private bool _IsPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (InputManager.HasInstance)
            InputManager.Instance.SetPlayerInput(true);
    }

    private void OnEnable()
    {
        OnPlayerDead += PlayerDead;
        OnGamePaused += GamePaused;
    }

    private void OnDisable()
    {
        OnPlayerDead -= PlayerDead;
        OnGamePaused -= GamePaused;
    }

    private void OnDestroy() => CancelInvoke();

    private void RespawnPlayer()
    {
        OnPlayerRespawn?.Invoke();
        OnResetEnemies?.Invoke();

        if (InputManager.HasInstance)
            InputManager.Instance.SetPlayerInput(!_IsPaused);
    }

    private void PlayerDead()
    {
        if (InputManager.HasInstance)
            InputManager.Instance.SetPlayerInput(false);

        Invoke(nameof(RespawnPlayer), _RespawnDelay);
    }

    private void GamePaused(bool paused)
    {
        _IsPaused = paused;
        if (InputManager.HasInstance)
            InputManager.Instance.SetPlayerInput(!_IsPaused);
    }
}
