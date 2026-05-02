using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public Slider reloadSlider;

    void Start()
    {
        // Auto-find the player if not assigned
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        if (reloadText != null)
            reloadText.gameObject.SetActive(false);
        if (reloadSlider != null) reloadSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
            if (player == null) return;
        }

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
            if (ammoText != null) ammoText.text = "-- / --";
            if (weaponNameText != null) weaponNameText.text = "";
            if (reloadText != null) reloadText.gameObject.SetActive(false);

            return;
        }

        if (ammoText != null)
            ammoText.text = $"<b>{w.CurrentAmmo}</b> / {w.ReserveAmmo}";

        if (weaponNameText != null)
            weaponNameText.text = w.weaponData.weaponName.ToUpper();

        if (reloadText != null)
            reloadText.gameObject.SetActive(w.IsReloading);

        if (reloadSlider != null)
        {
           // reloadSlider.value = Mathf.Clamp01(w._reloadStartTime / w._reloadEndTime);
            reloadSlider.gameObject.SetActive(w.IsReloading);
            //for some stupid reason this only works with Time.time and not TIme.deltatime
            //so like if I do _startTime(start at 0) and add delta time onto it and divide it with the _reloadEndTime it doesn't work
            //so after a hour I gave up and decided to just treat  w._reloadStartTime(or the starting Time.time) as my 0 
            float elapsed = Time.time - w._reloadStartTime;
            float duration = w._reloadEndTime - w._reloadStartTime;
            reloadSlider.value = Mathf.Clamp01(elapsed / duration);
           
          
        }
    }
}
