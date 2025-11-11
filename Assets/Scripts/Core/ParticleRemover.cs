using UnityEngine;

public class AutoDestroyParticle : MonoBehaviour
{
    public float lifeTine;

    public void Start()
    {
        Destroy(gameObject, lifeTine);
    }
}