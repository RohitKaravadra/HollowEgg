using UnityEngine;

public class BossFightTrigger : MonoBehaviour
{
    [SerializeField] Transform _Wall;
    [SerializeField] Transform _Trigger;

    private void Awake()
    {
        if (_Wall != null) _Wall.gameObject.SetActive(false);
        if (_Trigger != null) _Trigger.gameObject.SetActive(true);
    }

    private void OnEnable() => GameManager.OnPlayerRespawn += Reset;
    private void OnDisable() => GameManager.OnPlayerRespawn -= Reset;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.OnBossFightTriggered?.Invoke();
            if (_Wall != null) _Wall.gameObject.SetActive(true);
            if (_Trigger != null) _Trigger.gameObject.SetActive(false);
        }
    }

    private void Reset()
    {
        if (_Wall != null) _Wall.gameObject.SetActive(false);
        if (_Trigger != null) _Trigger.gameObject.SetActive(true);
    }
}
