using UnityEngine;
using TMPro;

/// <summary>
/// Attach to a TextMeshPro UI element to display the player's current points.
/// Subscribes to PointsManager.onPointsChanged so it only redraws on change.
/// </summary>
public class PointsUI : MonoBehaviour
{
    public TextMeshProUGUI pointsText;
    public string prefix = "Points: ";

    void Start()
    {
        if (pointsText == null)
            pointsText = GetComponent<TextMeshProUGUI>();

        // Wait a frame for PointsManager to Awake, then subscribe
        Invoke(nameof(Subscribe), 0f);
    }

    void Subscribe()
    {
        if (PointsManager.Instance == null) return;
        PointsManager.Instance.onPointsChanged.AddListener(Refresh);
        Refresh(PointsManager.Instance.CurrentPoints);
    }

    void Refresh(int points)
    {
        if (pointsText != null)
            pointsText.text = $"{prefix}<b>{points}</b>";
    }

    void OnDestroy()
    {
        if (PointsManager.Instance != null)
            PointsManager.Instance.onPointsChanged.RemoveListener(Refresh);
    }
}
