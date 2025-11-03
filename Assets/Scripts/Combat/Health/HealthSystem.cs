using UnityEngine;
using System;

namespace HnS.Health
{
    public class HealthSystem : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;

        public event Action<float, float> OnHealthChanged; //current / max
        public event Action<DamageInfo> OnDamageTaken;
        public event Action OnDeath;

        [field: SerializeField] public float CurrentHealth { get; private set; }
        public float MaxHealth { get { return maxHealth; } }
        public float HealthPercentage { get { return maxHealth > 0 ? CurrentHealth / maxHealth : 0f; } }

        private bool isDead = false;
        private float invincibilityTimer = 0f;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }
        private void Start()
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
        private void Update()
        {
            if (invincibilityTimer > 0f)
                invincibilityTimer -= Time.deltaTime;
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if ( isDead )
                return;

            CurrentHealth -= damageInfo.damageAmount;

            if (CurrentHealth <= 0f)
            {
                CurrentHealth = 0f;
                Die();
            }

            OnDamageTaken?.Invoke(damageInfo);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        }

        public void Heal(float amount)
        {
            if (isDead)
                return;

            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void ControlMaxHealth(bool isIncrease, float amount)
        {
            if( isIncrease )
                maxHealth += amount;
            else if( !isIncrease )
                maxHealth -= amount;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public float GetCurrentHealth()
        {
            return CurrentHealth;
        }
        public float GetMaxHealth()
        {
            return MaxHealth;
        }

        private void Die()
        {
            if (isDead)
                return;

            isDead = true;
            OnDeath?.Invoke();
        }
        public bool IsDead()
        {
            return isDead;
        }
        public void Kill()
        {
            CurrentHealth = 0f;
            Die();
        }
    }
}
