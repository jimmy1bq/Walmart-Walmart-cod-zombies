using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Singleton that owns the player's point total.
/// Add to a persistent GameObject in the scene (e.g. GameManager).
/// </summary>
public class PointsManager : MonoBehaviour
{
    public static PointsManager Instance { get; private set; }

    [Header("Settings")]
    public int killPoints = 50;         // points awarded per zombie kill

    public int CurrentPoints { get; private set; }

    public UnityEvent<int> onPointsChanged; // fires with new total whenever points change

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddPoints(int amount)
    {
        if (PowerupManager.Instance != null && PowerupManager.Instance.IsDoublePoints)
            amount *= 2;
        CurrentPoints += amount;
        onPointsChanged?.Invoke(CurrentPoints);
    }

    /// <summary>Returns true and deducts points if the player can afford it.</summary>
    public bool TrySpend(int cost)
    {
        if (CurrentPoints < cost) return false;
        CurrentPoints -= cost;
        onPointsChanged?.Invoke(CurrentPoints);
        return true;
    }
}
