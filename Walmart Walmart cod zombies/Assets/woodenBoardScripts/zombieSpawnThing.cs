using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class zombieSpawnThing : MonoBehaviour
{
    [SerializeField] GameObject zombies;
    public ZombieSpawnPosition spawnSide = ZombieSpawnPosition.Front;
    public float spawnInterval = 1.5f;
    [SerializeField] RoomId roomId = RoomId.G0;

    Coroutine _spawnLoop;
    bool _roomUnlocked;

    void Start()
    {
        _roomUnlocked = WoodenBoardManager.instance != null && WoodenBoardManager.instance.IsRoomUnlocked(roomId);
        WoodenBoardManager.onRoomUnlocked.AddListener(OnRoomUnlocked);

        if (RoundManager.Instance != null)
            RoundManager.Instance.onRoundStart.AddListener(OnRoundStart);
    }

    void OnDestroy()
    {
        WoodenBoardManager.onRoomUnlocked.RemoveListener(OnRoomUnlocked);
        if (RoundManager.Instance != null)
            RoundManager.Instance.onRoundStart.RemoveListener(OnRoundStart);
    }

    void OnRoomUnlocked(RoomId unlockedRoom)
    {
        if (unlockedRoom == roomId)
            _roomUnlocked = true;
    }

    void OnRoundStart(int round)
    {
        if (!_roomUnlocked) return;
        if (_spawnLoop != null) StopCoroutine(_spawnLoop);
        _spawnLoop = StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        // Wait for round announcement before first spawn
        if (RoundManager.Instance != null)
            yield return new WaitForSeconds(RoundManager.Instance.roundStartDelay);

        while (RoundManager.Instance != null && RoundManager.Instance.ZombiesLeftToSpawn > 0)
        {
            if (RoundManager.Instance.TryClaimSpawn())
            {
                Vector3 spawnPos = transform.position;
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 3f, NavMesh.AllAreas))
                    spawnPos = navHit.position;

                GameObject zombie = Instantiate(zombies, spawnPos, transform.rotation);
                zombie.GetComponent<ZombieAi>().zombieSpawnPos(spawnSide);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
        _spawnLoop = null;
    }
}
