using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(BoxCollider))]
public class buyGunHandler : MonoBehaviour
{
    [Header("Gun to sell")]
    public Guns gunToBuy;
    public WeaponData weaponData;

    [Header("World-Space Billboard (child Canvas)")]
    public Image weaponIcon;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI costText;

    [Header("Screen-Space Prompt (HUD Canvas)")]
    public GameObject promptPanel;
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI cantAffordText;

    [Header("Interaction")]
    public KeyCode buyKey = KeyCode.E;

    PlayerController _playerInRange;
    float _cantAffordTimer;

    void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        RefreshBillboard();
        SetPromptVisible(false);
        if (cantAffordText != null) cantAffordText.gameObject.SetActive(false);
    }

    void OnValidate()
    {
        RefreshBillboard();
    }

    void Update()
    {
        if (_playerInRange == null) return;

        if (Input.GetKeyDown(buyKey))
            buyGun(gunToBuy);

        if (_cantAffordTimer > 0f)
        {
            _cantAffordTimer -= Time.deltaTime;
            if (_cantAffordTimer <= 0f && cantAffordText != null)
                cantAffordText.gameObject.SetActive(false);
        }
    }

    public void buyGun(Guns gun)
    {
        if (weaponData == null || PointsManager.Instance == null)
        {
            Debug.LogWarning("buyGunHandler: Missing weaponData or PointsManager.");
            return;
        }

        // If the player already owns this gun, sell ammo at half price instead
        if (PlayerHasThisGun(out Weapon existing))
        {
            int ammoCost = weaponData.buyCost / 2;
            if (!PointsManager.Instance.TrySpend(ammoCost))
            {
                ShowCantAfford();
                return;
            }
            existing.RefillAmmo();
            Debug.Log($"Refilled ammo for {weaponData.weaponName} – {ammoCost} pts.");
            return;
        }

        // Otherwise buy the gun
        if (!PointsManager.Instance.TrySpend(weaponData.buyCost))
        {
            ShowCantAfford();
            return;
        }

        GiveGunToPlayer(weaponData);
        Debug.Log($"Bought {weaponData.weaponName} – {weaponData.buyCost} pts.");

        // Update the prompt immediately so it now shows "Buy Ammo"
        RefreshPrompt();
    }

    // Returns true if any weapon slot holds this exact WeaponData
    bool PlayerHasThisGun(out Weapon found)
    {
        found = null;
        if (_playerInRange == null) return false;

        foreach (Weapon slot in _playerInRange.weaponSlots)
        {
            if (slot != null && slot.weaponData == weaponData)
            {
                found = slot;
                return true;
            }
        }
        return false;
    }

    void GiveGunToPlayer(WeaponData data)
    {
        if (_playerInRange == null) return;

        int target = _playerInRange.GetTargetSlot();
        if (target == -1)
        {
            Debug.LogWarning("buyGunHandler: No valid weapon slot found. Make sure the Player has Weapon components assigned in both weaponSlots.");
            return;
        }

        _playerInRange.GiveWeapon(target, data);
    }

    void RefreshBillboard()
    {
        if (weaponData == null) return;

        if (weaponIcon != null)
        {
            weaponIcon.sprite = weaponData.icon;
            weaponIcon.gameObject.SetActive(weaponData.icon != null);
        }

        if (weaponNameText != null)
            weaponNameText.text = weaponData.weaponName.ToUpper();

        if (costText != null)
            costText.text = $"{weaponData.buyCost} pts";
    }

    void RefreshPrompt()
    {
        if (promptText == null || weaponData == null) return;
        int ammoCost = weaponData.buyCost / 2;
        if (PlayerHasThisGun(out _))
        {
           
            promptText.text = $"[{buyKey}]  Buy Ammo  –  {ammoCost} pts";
        }
        else
        {
            //Its [BUY - {weaponData.buyCost}], [Ammo - ammoCost]
            //not $"[{buyKey}]  Buy {weaponData.weaponName}  –  {weaponData.buyCost} pts"
            promptText.text = $"[BUY - {weaponData.buyCost}], [Ammo - {ammoCost}]";
        }
    }

    void SetPromptVisible(bool visible)
    {
        if (promptPanel != null) promptPanel.SetActive(visible);
        RefreshPrompt();
    }

    void ShowCantAfford()
    {
        if (cantAffordText == null) return;
        cantAffordText.text = "Not enough points!";
        cantAffordText.gameObject.SetActive(true);
        _cantAffordTimer = 2f;
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            _playerInRange = pc;
            SetPromptVisible(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            _playerInRange = null;
            SetPromptVisible(false);
            if (cantAffordText != null) cantAffordText.gameObject.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc != null) Gizmos.DrawCube(transform.position + bc.center, bc.size);
    }
}
