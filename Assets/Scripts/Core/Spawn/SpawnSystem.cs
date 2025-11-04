using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Hns.Spawn {
    public class SpawnSystem : MonoBehaviour
    {
        [Header("Spawn Points")]
        [SerializeField]private SpawnPoint[] spawnPoints;

        [Header("Wave Settings")]
        public bool enableWaves = false;
        public float waveDelay = 5f;

        void Awake()
        {
            spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint")
                        .Select(go => go.GetComponent<SpawnPoint>())
                        .Where(sp => sp != null)
                        .Distinct()
                        .ToArray();

            StartSpawning();
        }

        public void StartSpawning()
        {
            if (enableWaves)
                StartCoroutine(WaveSpawning());
            else
                SpawnAll();
        }
        
        private void SpawnAll()
        {
            foreach (SpawnPoint spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    spawnPoint.Spawn();
                }
            }
        }

        private IEnumerator WaveSpawning()
        {
            while (true)
            {
                SpawnAll();
                yield return new WaitForSeconds(waveDelay);
            }
        }

        public void ClearAllSpawns()
        {
            foreach (SpawnPoint spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                    spawnPoint.ClearSpawned();
            }
        }
    /*
        public void SpawnByType(SpawnType type)
        {
            foreach (SpawnPoint spawnPoint in spawnPoints)
            {
                if (spawnPoint != null && spawnPoint.spawnType == type)
                    spawnPoint.Spawn();
            }
        }
        */

    }
}
