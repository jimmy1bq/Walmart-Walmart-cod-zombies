#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Creates all WeaponData ScriptableObject assets from the Godot gun stats.
/// Menu: Tools > Zombies > Create All Weapon Data
/// </summary>
public static class WeaponDataCreator
{
    const string OUTPUT_PATH = "Assets/WeaponData";

    // Godot FireRate is RPM; Unity fireRate is seconds between shots = 60 / RPM
    static float RPMtoSeconds(float rpm) => 60f / rpm;

    // Godot MaxAmmo is reserve only; Unity maxAmmo is total (reserve + clip)
    static int TotalAmmo(int reserve, int clip) => reserve + clip;

    [MenuItem("Tools/Zombies/Create All Weapon Data")]
    static void CreateAll()
    {
        if (!Directory.Exists(OUTPUT_PATH))
            AssetDatabase.CreateFolder("Assets", "WeaponData");

        CreateWeapon(new WeaponStats
        {
            name              = "Semi Auto Rifle",
            damage            = 30f,
            fireRate          = RPMtoSeconds(450),
            clipSize          = 20,
            maxAmmo           = TotalAmmo(140, 20),
            spread            = 2f,
            spreadGainPerShot = 3f,
            recoil            = 0.3f,
            pelletCount       = 1,
            projectileSpeed   = 180f,
            range             = 100f,
            reloadTime        = 2.0f,
            isAutomatic       = false,
            buyCost           = 900
        });

        CreateWeapon(new WeaponStats
        {
            name              = "Shotgun",
            damage            = 27f,
            fireRate          = RPMtoSeconds(175),
            clipSize          = 7,
            maxAmmo           = TotalAmmo(36, 7),
            spread            = 8f,
            spreadGainPerShot = 10f,
            recoil            = 0.8f,
            pelletCount       = 8,
            projectileSpeed   = 180f,
            range             = 100f,
            reloadTime        = 3.0f,
            isAutomatic       = false,
            buyCost           = 1100
        });

        CreateWeapon(new WeaponStats
        {
            name              = "Shotgun Sawn Off",
            damage            = 20f,
            fireRate          = RPMtoSeconds(325),
            clipSize          = 2,
            maxAmmo           = TotalAmmo(36, 2),
            spread            = 10.5f,
            spreadGainPerShot = 12f,
            recoil            = 0.8f,
            pelletCount       = 8,
            projectileSpeed   = 180f,
            range             = 100f,
            reloadTime        = 1.5f,
            isAutomatic       = false,
            buyCost           = 600
        });

        CreateWeapon(new WeaponStats
        {
            name              = "SMG",
            damage            = 22f,
            fireRate          = RPMtoSeconds(650),
            clipSize          = 35,
            maxAmmo           = TotalAmmo(340, 35),
            spread            = 6f,
            spreadGainPerShot = 1.5f,
            recoil            = 0.2f,
            pelletCount       = 1,
            projectileSpeed   = 180f,
            range             = 100f,
            reloadTime        = 2.0f,
            isAutomatic       = true,
            buyCost           = 1100
        });

        CreateWeapon(new WeaponStats
        {
            name              = "Revolver",
            damage            = 55f,
            fireRate          = RPMtoSeconds(325),
            clipSize          = 6,
            maxAmmo           = TotalAmmo(80, 6),
            spread            = 2f,
            spreadGainPerShot = 5f,
            recoil            = 0.55f,
            pelletCount       = 1,
            projectileSpeed   = 180f,
            range             = 100f,
            reloadTime        = 1.7f,
            isAutomatic       = false,
            buyCost           = 600
        });

        CreateWeapon(new WeaponStats
        {
            name              = "Pistol",
            damage            = 20f,
            fireRate          = RPMtoSeconds(450),
            clipSize          = 12,
            maxAmmo           = TotalAmmo(180, 12),
            spread            = 3f,
            spreadGainPerShot = 3f,
            recoil            = 0.25f,
            pelletCount       = 1,
            projectileSpeed   = 180f,
            range             = 100f,
            reloadTime        = 1.5f,
            isAutomatic       = false,
            buyCost           = 300
        });

        CreateWeapon(new WeaponStats
        {
            name              = "Assault Rifle",
            damage            = 27f,
            fireRate          = RPMtoSeconds(500),
            clipSize          = 30,
            maxAmmo           = TotalAmmo(260, 30),
            spread            = 5f,
            spreadGainPerShot = 2f,
            recoil            = 0.3f,
            pelletCount       = 1,
            projectileSpeed   = 180f,
            range             = 100f,
            reloadTime        = 2.0f,
            isAutomatic       = true,
            buyCost           = 1100
        });

        CreateWeapon(new WeaponStats
        {
            name              = "Bolt Action Rifle",
            damage            = 80f,
            fireRate          = RPMtoSeconds(125),
            clipSize          = 6,
            maxAmmo           = TotalAmmo(40, 6),
            spread            = 1f,
            spreadGainPerShot = 9f,
            recoil            = 0.4f,
            pelletCount       = 1,
            projectileSpeed   = 180f,
            range             = 100f,
            reloadTime        = 2.0f,
            isAutomatic       = false,
            buyCost           = 900
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[WeaponDataCreator] Created weapon assets in {OUTPUT_PATH}");
        EditorUtility.FocusProjectWindow();
    }

    static void CreateWeapon(WeaponStats s)
    {
        string safeName = s.name.Replace(" ", "_");
        string path     = $"{OUTPUT_PATH}/{safeName}.asset";

        // Overwrite if it already exists
        WeaponData existing = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
        WeaponData asset    = existing != null ? existing : ScriptableObject.CreateInstance<WeaponData>();

        asset.weaponName        = s.name;
        asset.damage            = s.damage;
        asset.fireRate          = s.fireRate;
        asset.clipSize          = s.clipSize;
        asset.maxAmmo           = s.maxAmmo;
        asset.spread            = s.spread;
        asset.spreadGainPerShot = s.spreadGainPerShot;
        asset.spreadRecoveryRate= 8f;
        asset.recoil            = s.recoil;
        asset.pelletCount       = s.pelletCount;
        asset.projectileSpeed   = s.projectileSpeed;
        asset.range             = s.range;
        asset.reloadTime        = s.reloadTime;
        asset.isAutomatic       = s.isAutomatic;
        asset.buyCost           = s.buyCost;

        if (existing == null)
            AssetDatabase.CreateAsset(asset, path);
        else
            EditorUtility.SetDirty(asset);

        Debug.Log($"  Created: {path}");
    }

    struct WeaponStats
    {
        public string name;
        public float  damage;
        public float  fireRate;
        public int    clipSize;
        public int    maxAmmo;
        public float  spread;
        public float  spreadGainPerShot;
        public float  recoil;
        public int    pelletCount;
        public float  projectileSpeed;
        public float  range;
        public float  reloadTime;
        public bool   isAutomatic;
        public int    buyCost;
    }
}
#endif
