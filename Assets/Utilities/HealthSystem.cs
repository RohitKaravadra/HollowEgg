
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HealthSystem : MonoBehaviour, IDamageable
{
    [SerializeField] int _MaxHealth = 100;
    public UnityEvent OnHit;
    public UnityEvent OnDeath;

    float _Health;

    public bool IsAlive => _Health > 0;
    public float Health => _Health;
    public void Heal(float amount) => _Health = Mathf.Min(_MaxHealth, _Health + amount);
    public void Reset() => _Health = _MaxHealth;
    public void TakeDamage(float damage)
    {
        if (!IsAlive)
            return;

        _Health = Mathf.Max(0, _Health - damage);
        OnHit?.Invoke();
        if (!IsAlive) OnDeath?.Invoke();
    }
}
