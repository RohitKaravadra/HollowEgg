using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] float _RespawnDelay = 3f;

    public static Action OnPlayerDead;
    public static Action OnPlayerRespawn;
    public static Action OnBossFightTriggered;
    public static Action OnBossDeathTriggered;
    public static Action OnResetEnemies;

    bool _IsBossFight = false;

    public static bool HasInstance => Instance != null;
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        OnPlayerDead += PlayerDead;
        OnBossFightTriggered += BossFightTriggered;
        OnBossDeathTriggered += BossDeathTriggered;
    }

    private void OnDisable()
    {
        OnPlayerDead -= PlayerDead;
        OnBossFightTriggered -= BossFightTriggered;
        OnBossDeathTriggered -= BossDeathTriggered;
    }

    private void OnDestroy() => CancelInvoke();

    private void RespawnPlayer()
    {
        OnPlayerRespawn?.Invoke();
        OnResetEnemies?.Invoke();
    }

    private void BossFightTriggered()
    {
        _IsBossFight = true;
    }

    private void ReturnToMenu() => SceneManager.LoadScene(0);

    private void BossDeathTriggered()
    {
        _IsBossFight = false;
        if (InputManager.HasInstance)
        {
            InputManager.Instance.SetPlayerInput(false);
        }
        Invoke(nameof(ReturnToMenu), 2f);
    }

    private void PlayerDead()
    {
        if (_IsBossFight)
        {
            _IsBossFight = false;
        }
        Invoke(nameof(RespawnPlayer), _RespawnDelay);
    }
}
