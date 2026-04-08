using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Zombies/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "Pistol";
    public int buyCost = 500;

    [Header("Damage")]
    public float damage = 25f;
    public int pelletCount = 1;         // >1 for shotguns (fires N raycasts per shot)

    [Header("Fire")]
    public float fireRate = 0.5f;       // seconds between shots (= 60 / RPM)
    public bool isAutomatic = false;

    [Header("Accuracy")]
    public float spread = 2f;           // base spread in degrees
    public float spreadGainPerShot = 1f;// spread added per shot
    public float spreadRecoveryRate = 8f;// degrees recovered per second
    public float recoil = 0.3f;         // upward camera kick per shot

    [Header("Range")]
    public float range = 100f;
    public float projectileSpeed = 180f; // kept for reference / future projectile mode

    [Header("Ammo")]
    public int clipSize = 12;
    public int maxAmmo = 120;           // total ammo including starting clip
    public float reloadTime = 1.5f;
}
