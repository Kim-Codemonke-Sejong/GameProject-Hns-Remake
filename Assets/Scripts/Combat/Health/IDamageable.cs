using UnityEngine;
using HnS.Health;


public interface IDamageable
{
    void TakeDamage(DamageInfo damageInfo);
    bool IsDead();
    float GetCurrentHealth();
    float GetMaxHealth();
}
