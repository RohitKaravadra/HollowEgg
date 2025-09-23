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

    private void OnEnable() => OnPlayerDead += PlayerDead;
    private void OnDisable() => OnPlayerDead -= PlayerDead;

    private void OnDestroy() => CancelInvoke();

    private void RespawnPlayer()
    {
        OnPlayerRespawn?.Invoke();
        OnResetEnemies?.Invoke();
    }

    private void PlayerDead() => Invoke(nameof(RespawnPlayer), _RespawnDelay);
}
