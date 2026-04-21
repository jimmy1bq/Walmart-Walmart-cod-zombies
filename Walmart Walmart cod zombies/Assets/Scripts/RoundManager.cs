using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("Zombie Prefab & Spawn Points")]
    public GameObject zombiePrefab;
    public Transform[] spawnPoints;

    [Header("Round Scaling")]
    public int baseZombieCount = 6;
    public int zombiesPerRoundIncrease = 2;

    [Header("Spawn Pacing")]
    public int maxZombiesAlive = 24;
    public float spawnInterval = 1.5f;
    public float roundStartDelay = 5f;

    public int CurrentRound       { get; private set; }
    public int ZombiesAlive       { get; private set; }
    public int ZombiesLeftToSpawn { get; private set; }

    public UnityEvent<int> onRoundStart;
    public UnityEvent      onRoundEnd;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => StartCoroutine(RoundLoop(1));

    int ZombieCountForRound(int round) =>
        baseZombieCount + (round - 1) * zombiesPerRoundIncrease;

    IEnumerator RoundLoop(int round)
    {
        while (true)
        {
            // ---- setup ----
            CurrentRound       = round;
            ZombiesAlive       = 0;
            ZombiesLeftToSpawn = ZombieCountForRound(round);
            onRoundStart?.Invoke(round);

            yield return new WaitForSeconds(roundStartDelay);

            // ---- spawn all zombies for this round ----
            while (ZombiesLeftToSpawn > 0)
            {
                yield return new WaitUntil(() => ZombiesAlive < maxZombiesAlive);
                SpawnZombie();
                ZombiesLeftToSpawn--;
                yield return new WaitForSeconds(spawnInterval);
            }

            // ---- wait for every zombie to be killed ----
            yield return new WaitUntil(() => ZombiesAlive <= 0);

            // ---- round over ----
            onRoundEnd?.Invoke();
            round++;
        }
    }

    void SpawnZombie()
    {
        if (zombiePrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(zombiePrefab, sp.position, sp.rotation);
        ZombiesAlive++;
    }

    // Called by ZombieAi on death — just a counter decrement now
    public void OnZombieKilled()
    {
        ZombiesAlive = Mathf.Max(0, ZombiesAlive - 1);
    }
}
