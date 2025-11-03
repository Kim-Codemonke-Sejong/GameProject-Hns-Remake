using UnityEngine;
using HnS.Health;

public class DeathHandler : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 0f;
    [SerializeField] private GameObject deathEffectPrefab;
    
    private HealthSystem healthSystem;
    
    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
    }
    
    private void OnEnable()
    {
        if (healthSystem != null)
            healthSystem.OnDeath += HandleDeath;
    }
    
    private void OnDisable()
    {
        if (healthSystem != null)
            healthSystem.OnDeath -= HandleDeath;
    }
    
    private void HandleDeath()
    {
        // 죽음 이펙트 생성
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, transform.rotation);
        }
        
        // 오브젝트 제거
        if (destroyDelay > 0f)
            Destroy(gameObject, destroyDelay);
        else
            Destroy(gameObject);
    }
}