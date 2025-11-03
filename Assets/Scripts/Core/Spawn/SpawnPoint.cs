using UnityEngine;
using System.Collections.Generic;

public class SpawnPoint : MonoBehaviour
{
    private static Dictionary<string, SpawnPoint> spawnPoints = new Dictionary<string, SpawnPoint>();
    
    [Header("Spawn Settings")]
    public SpawnType spawnType = SpawnType.Enemy;
    public GameObject spawnPrefab;

    [Header("Spawn Options")]
    public int maxSpawnCount = 1;
    public float spawnRadius = 2f;

    private int currentSpawnCount = 0;
    private bool hasSpawned = false;

    void OnEnable()
    {
        if (!spawnPoints.ContainsKey(this.name))
            spawnPoints.Add(this.name, this);
    }
    
    void OnDisable()
    {
        if (spawnPoints.ContainsKey(this.name))
            spawnPoints.Remove(this.name);
    }

    public void Spawn()
    {
        if (hasSpawned || spawnPrefab == null)
            return;

        for (int i = 0; i < maxSpawnCount; i++)
        {
            Vector3 randomPosition = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPosition.y = transform.position.y;

            GameObject spawnedObject = Instantiate(spawnPrefab, randomPosition, Quaternion.identity);
            currentSpawnCount++;
        }

        hasSpawned = true;
    }
    public void ClearSpawned()
    {
        hasSpawned = false;
        currentSpawnCount = 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
    
    public enum SpawnType
    {
        Enemy,
        Decor,
        Item
    }
}