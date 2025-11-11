using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
    public class EnemyGroup
    {
        public GameObject enemyPrefab;
        public int count;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public List<EnemyGroup> enemygroups = new List<EnemyGroup>();
        public float timeBetweenSpawns = 0.5f;
    }
public class WaveSpawner : MonoBehaviour
{
    [Header("Wave settings")]
    public List<Wave> waves;
    [Header("Spawn Points")]
    public List<Transform> spawnPoints;

    [Header("Wave Status")]
    public int currentWaveIndex = 0;
    public bool isSpawning = false;

    private int enemiesAlive = 0;

    public static Action WaveStarted;
    public static Action WaveEnded;


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
