using UnityEngine;

public class PowerupDropTable : MonoBehaviour
{
    public static PowerupDropTable Instance { get; private set; }

    [Header("Powerup Prefabs")]
    public GameObject doublePointsPrefab;
    public GameObject doubleTapPrefab;
    public GameObject maxAmmoPrefab;

    [Range(0f, 1f)]
    public float dropChance = 0.2f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void TryDrop(Vector3 position)
    {
        GameObject[] prefabs = { doublePointsPrefab, doubleTapPrefab, maxAmmoPrefab };
        int index = UnityEngine.Random.Range(0, prefabs.Length);
        if (prefabs[index] == null) return;

        if (UnityEngine.Random.value < dropChance)
            Instantiate(prefabs[index], position + Vector3.up * 0.5f, Quaternion.identity);
    }
}
