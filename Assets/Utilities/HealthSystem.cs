
using UnityEngine;
using UnityEngine.Events;

public interface IDamageable
{
    int Health { get; }
    void TakeDamage(int damage);
    void Heal(int amount);
    void Reset();
}

[System.Serializable]
public class HealthSystem : MonoBehaviour, IDamageable
{
    [SerializeField] int _MaxHealth = 100;
    public UnityEvent OnHit;
    public UnityEvent OnDeath;

    int _Health;

    public bool IsAlive => _Health > 0;
    public int Health => _Health;
    public int MaxHealth => _MaxHealth;
    public void Heal(int amount) => _Health = Mathf.Min(_MaxHealth, _Health + amount);
    public void Reset() => _Health = _MaxHealth;
    public void TakeDamage(int damage)
    {
        if (!IsAlive)
            return;

        _Health = Mathf.Max(0, _Health - damage);
        OnHit?.Invoke();
        if (!IsAlive) OnDeath?.Invoke();
    }
}
