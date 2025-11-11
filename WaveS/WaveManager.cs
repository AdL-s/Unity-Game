using UnityEngine;
using System.Collections; // Required for IEnumerator

[System.Serializable]
public class EnemyType
{
    public string tag;
    public GameObject prefab;
}

[System.Serializable]
public class WaveData
{
    public string waveName;
    public Transform[] spawnPoints;
    public float delayBeforeWave = 1.0f; // Delay before this wave starts
}

public class WaveManager : MonoBehaviour
{
    public EnemyType[] enemyTypes;
    public WaveData[] waves;
    private int currentWaveIndex = 0;
    public bool waveEnded = true;

    void Start()
    {
        // Start the first wave after a short delay
        StartCoroutine(SpawnWaveCoroutine());
    }

    // Public method to be called from other scripts to start the next wave
    public void StartNextWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex < waves.Length)
        {
            StartCoroutine(SpawnWaveCoroutine());
            waveEnded = false;
        }
        else
        {
            Debug.Log("All waves completed!");
            waveEnded = true;
        }
    }

    private IEnumerator SpawnWaveCoroutine()
    {
        if (currentWaveIndex < waves.Length)
        {
            WaveData currentWave = waves[currentWaveIndex];
            yield return new WaitForSeconds(currentWave.delayBeforeWave);
            Debug.Log("Spawning Wave: " + currentWave.waveName);

            foreach (Transform spawnPoint in currentWave.spawnPoints)
            {
                if (spawnPoint == null)
                {
                    Debug.LogWarning("A spawn point in the wave is not assigned (null). Skipping it.");
                    continue;
                }

                string spawnTag = "";
                try
                {
                    spawnTag = spawnPoint.tag;
                }
                catch (UnityException)
                {
                    Debug.LogError("The spawn point " + spawnPoint.name + " does not have a tag assigned. Please assign a tag in the inspector.", spawnPoint);
                    continue; // Skip this spawn point
                }

                GameObject enemyPrefab = GetEnemyPrefabForTag(spawnTag);
                if (enemyPrefab != null)
                {
                    Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                }
                else
                {
                    Debug.LogWarning("No enemy prefab found for tag: " + spawnTag);
                }
            }
        }
    }

    private GameObject GetEnemyPrefabForTag(string tag)
    {
        foreach (EnemyType enemyType in enemyTypes)
        {
            if (enemyType.tag == tag)
            {
                return enemyType.prefab;
            }
        }
        return null;
    }
}