using UnityEngine;
using UnityEngine.UI;

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager Instance { get; private set; }

   

    public bool IsDoublePoints { get; private set; }
    public bool IsDoubleTap    { get; private set; }
    public bool IsMaxAmmo      { get; private set; }

    float _doublePointsEnd;
    float _doubleTapEnd;
    float _maxAmmoEnd;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (IsDoublePoints && Time.time >= _doublePointsEnd) { GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().doublePointsIconHere = false;  IsDoublePoints = false; }
        if (IsDoubleTap && Time.time >= _doubleTapEnd) { GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().doubleShotIconHere = false; IsDoubleTap = false; }
        if (IsMaxAmmo && Time.time >= _maxAmmoEnd) { GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().maxAMMOIconHere = false; IsMaxAmmo = false; }
    }

    public void Reset()
    {
        IsDoublePoints = false;
        IsDoubleTap = false;
    }

    public void ActivateDoublePoints(float duration = 15f)
    {
        IsDoublePoints    = true;
        _doublePointsEnd  = Time.time + duration;
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().addPowerupUI(0);
    }

    public void ActivateDoubleTap(float duration = 15f)
    {
        IsDoubleTap    = true;
        _doubleTapEnd  = Time.time + duration;
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().addPowerupUI(2);
    }

    public void ActivateMaxAmmo()
    {
        foreach (Weapon w in FindObjectsByType<Weapon>(FindObjectsSortMode.None))
            w.RefillAmmo();
         IsMaxAmmo = true;
        _maxAmmoEnd = Time.time + 15f;
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().addPowerupUI(1);
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
