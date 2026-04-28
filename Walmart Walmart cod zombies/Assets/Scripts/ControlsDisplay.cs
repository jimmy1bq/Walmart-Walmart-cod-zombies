using UnityEngine;


public class ControlsDisplay : MonoBehaviour
{
    [Header("UI")]
    public GameObject controlsPanel;

    [Header("Settings")]
    [Tooltip("Also allow toggling the overlay with this key during gameplay (Tab by default).")]
    public KeyCode toggleKey = KeyCode.Tab;

    bool _dismissed;

    void Start()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(true);

        // Pause time while controls are shown so the player isn't caught off-guard
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (_dismissed)
        {
            // Toggle overlay with the configured key
            if (Input.GetKeyDown(toggleKey))
                SetOverlay(!controlsPanel.activeSelf);
            return;
        }

        // Dismiss on any key press or mouse click
        if (Input.anyKeyDown)
            Dismiss();
    }

    void Dismiss()
    {
        _dismissed = true;
        SetOverlay(false);
        Time.timeScale = 1f;

        // Re-lock cursor in case it was freed
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void SetOverlay(bool visible)
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(visible);

        if (visible)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }
}
