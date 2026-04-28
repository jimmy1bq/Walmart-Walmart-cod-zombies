using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager Instance { get; private set; }

    public bool IsDoublePoints { get; private set; }
    public bool IsDoubleTap    { get; private set; }

    float _doublePointsEnd;
    float _doubleTapEnd;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (IsDoublePoints && Time.time >= _doublePointsEnd) IsDoublePoints = false;
        if (IsDoubleTap    && Time.time >= _doubleTapEnd)    IsDoubleTap    = false;
    }

    public void ActivateDoublePoints(float duration = 30f)
    {
        IsDoublePoints    = true;
        _doublePointsEnd  = Time.time + duration;
    }

    public void ActivateDoubleTap(float duration = 30f)
    {
        IsDoubleTap    = true;
        _doubleTapEnd  = Time.time + duration;
    }

    public void ActivateMaxAmmo()
    {
        foreach (Weapon w in FindObjectsByType<Weapon>(FindObjectsSortMode.None))
            w.RefillAmmo();
    }

    public static PowerupManager GetOrCreate()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("PowerupManager");
        var pm = go.AddComponent<PowerupManager>();
        DontDestroyOnLoad(go);
        return pm;
    }
}
