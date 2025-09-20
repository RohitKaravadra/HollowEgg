using UnityEngine;

public interface IDamageable
{
    float Health { get; }
    void TakeDamage(float damage);
    void Heal(float amount);
    void Reset();
}
