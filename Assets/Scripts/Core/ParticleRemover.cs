using UnityEngine;

public class AutoDestroyParticle : MonoBehaviour
{
    private ParticleSystem ps;
    
    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        
        if (ps == null)
            Debug.LogWarning($"{gameObject.name} NOT HAVE PARTICLESYSTEM");
    }
    
    private void Start()
    {
        if (ps != null && ps.main.loop)
            Debug.LogWarning($"{gameObject.name} ON LOOP");
    }
    
    private void Update()
    {
        if (ps != null && !ps.IsAlive())
            Destroy(gameObject);
    }
}