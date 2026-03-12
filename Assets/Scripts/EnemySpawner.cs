using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject enemyPrefab;
    public float spawnRadius = 15f;

    [Header("Waves")]
    [Tooltip("Each element is the enemy count for that wave. E.g. [1, 5, 10]")]
    public int[] waveSizes = { 1, 5, 10 };

    [Tooltip("Seconds to wait before spawning the next wave after the current is cleared.")]
    public float delayBetweenWaves = 2f;

    public int CurrentWave { get; private set; }
    public bool AllWavesCleared { get; private set; }

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCleared;
    public event Action OnAllWavesCleared;

    private readonly List<GameObject> _activeEnemies = new List<GameObject>();

    void Start()
    {
        if (waveSizes == null || waveSizes.Length == 0)
        {
            AllWavesCleared = true;
            OnAllWavesCleared?.Invoke();
            return;
        }

        CurrentWave = 0;
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        for (int w = 0; w < waveSizes.Length; w++)
        {
            CurrentWave = w;
            SpawnWave(waveSizes[w]);
            OnWaveStarted?.Invoke(w);

            while (_activeEnemies.Count > 0)
            {
                _activeEnemies.RemoveAll(e => e == null);
                yield return null;
            }

            OnWaveCleared?.Invoke(w);

            if (w < waveSizes.Length - 1)
                yield return new WaitForSeconds(delayBetweenWaves);
        }

        AllWavesCleared = true;
        OnAllWavesCleared?.Invoke();
    }

    private void SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject enemy = SpawnEnemy();
            if (enemy != null)
                _activeEnemies.Add(enemy);
        }
    }

    private GameObject SpawnEnemy()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * spawnRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
        {
            return Instantiate(enemyPrefab, hit.position, Quaternion.identity);
        }

        return null;
    }
}
