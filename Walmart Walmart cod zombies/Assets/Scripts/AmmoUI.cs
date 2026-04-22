using UnityEngine;
using TMPro;

/// <summary>
/// Attach to a UI Canvas GameObject.
/// Reads the active weapon from PlayerController and updates the ammo display every frame.
///
/// UI Layout (set up in Inspector):
///   ammoText    — shows  "12 / 140"  (clip / reserve)
///   weaponName  — shows  "Pistol"
///   reloadText  — shows  "RELOADING..."  (hidden when not reloading)
/// </summary>
public class AmmoUI : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;

    [Header("UI Elements")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI reloadText;

    void Start()
    {
        // Auto-find the player if not assigned
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        if (reloadText != null)
            reloadText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        // Use the currently tracked weapon; fall back to the first slot with data.
        Weapon w = player.CurrentWeapon;
        if (w == null || w.weaponData == null)
        {
            w = null;
            foreach (Weapon slot in player.weaponSlots)
            {
                if (slot != null && slot.weaponData != null)
                {
                    w = slot;
                    break;
                }
            }
        }

        if (w == null)
        {
            if (ammoText != null)       ammoText.text       = "-- / --";
            if (weaponNameText != null) weaponNameText.text = "";
            if (reloadText != null)     reloadText.gameObject.SetActive(false);
            return;
        }

        if (ammoText != null)
            ammoText.text = $"<b>{w.CurrentAmmo}</b> / {w.ReserveAmmo}";

        if (weaponNameText != null)
            weaponNameText.text = w.weaponData.weaponName.ToUpper();

        if (reloadText != null)
            reloadText.gameObject.SetActive(w.IsReloading);
    }
}
