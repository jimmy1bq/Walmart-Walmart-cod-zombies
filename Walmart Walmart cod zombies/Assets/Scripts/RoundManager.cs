using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("Round Scaling")]
    public int baseZombieCount        = 6;
    public int zombiesPerRoundIncrease = 2;

    [Header("Pacing")]
    public int   maxZombiesAlive = 24;
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
        // Wait one frame so all Start() methods (including spawner subscriptions) finish
        yield return null;

        while (true)
        {
            CurrentRound       = round;
            ZombiesAlive       = 0;
            ZombiesLeftToSpawn = ZombieCountForRound(round);
            onRoundStart?.Invoke(round);

            yield return new WaitForSeconds(roundStartDelay);

            // Wait until all zombies have been spawned AND killed
            yield return new WaitUntil(() => ZombiesLeftToSpawn <= 0 && ZombiesAlive <= 0);

            onRoundEnd?.Invoke();
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().roundsSurvived++;
            round++;
        }
    }

    // Called by zombieSpawnThing to atomically claim one spawn slot.
    // Returns true if a zombie should be spawned.
    public bool TryClaimSpawn()
    {
        if (ZombiesLeftToSpawn <= 0 || ZombiesAlive >= maxZombiesAlive) return false;
        ZombiesLeftToSpawn--;
        ZombiesAlive++;
        return true;
    }

    public void OnZombieKilled()
    {
        ZombiesAlive = Mathf.Max(0, ZombiesAlive - 1);
    }
}
