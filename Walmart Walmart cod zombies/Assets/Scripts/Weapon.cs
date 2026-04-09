using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;

    [Header("Visuals / Audio")]
    public Transform muzzlePoint;
    public GameObject muzzleFlashPrefab;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    int _currentAmmo;
    int _reserveAmmo;
    float _nextFireTime;
    bool _isReloading;
    float _currentSpread;   // dynamic spread that grows per shot and recovers over time
    AudioSource _audio;

    public int CurrentAmmo     => _currentAmmo;
    public int ReserveAmmo     => _reserveAmmo;
    public bool IsReloading    => _isReloading;
    public float CurrentSpread => _currentSpread;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (weaponData != null)
        {
            _currentAmmo   = weaponData.clipSize;
            _reserveAmmo   = weaponData.maxAmmo - weaponData.clipSize;
            _currentSpread = weaponData.spread;
        }
    }

    void OnEnable()
    {
        _isReloading   = false;
        _currentSpread = weaponData != null ? weaponData.spread : 0f;
    }

    void Update()
    {
        // Spread recovers toward base spread over time
        if (weaponData != null && _currentSpread > weaponData.spread)
        {
            _currentSpread -= weaponData.spreadRecoveryRate * Time.deltaTime;
            _currentSpread  = Mathf.Max(_currentSpread, weaponData.spread);
        }
    }

    public void TryShoot(Transform aimOrigin)
    {
        if (_isReloading) return;
        if (Time.time < _nextFireTime) return;

        if (_currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        Shoot(aimOrigin);
    }

    void Shoot(Transform aimOrigin)
    {
        _currentAmmo--;
        _nextFireTime = Time.time + weaponData.fireRate;

        // Fire one raycast per pellet (1 for normal guns, 8 for shotguns, etc.)
        for (int i = 0; i < weaponData.pelletCount; i++)
        {
            Vector3 direction = ApplySpread(aimOrigin.forward);
            Ray ray = new Ray(aimOrigin.position, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
            {
                IDamageAble zombie = hit.collider.GetComponent<IDamageAble>();
                if (zombie != null)
                    zombie.takeDamage(weaponData.damage);

                Debug.DrawLine(ray.origin, hit.point, Color.red, 0.5f);
            }
        }

        // Grow spread after firing
        _currentSpread += weaponData.spreadGainPerShot;

        if (muzzleFlashPrefab != null && muzzlePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
            Destroy(flash, 0.05f);
        }

        if (_audio != null && shootSound != null)
            _audio.PlayOneShot(shootSound);

        if (_currentAmmo <= 0 && _reserveAmmo > 0)
            StartCoroutine(Reload());
    }

    // Returns a direction with random spread applied (in degrees)
    Vector3 ApplySpread(Vector3 forward)
    {
        float halfAngle = _currentSpread * 0.5f;
        float x = Random.Range(-halfAngle, halfAngle);
        float y = Random.Range(-halfAngle, halfAngle);
        return Quaternion.Euler(x, y, 0f) * forward;
    }

    System.Collections.IEnumerator Reload()
    {
        if (_isReloading || _reserveAmmo <= 0) yield break;

        _isReloading = true;

        if (_audio != null && reloadSound != null)
            _audio.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(weaponData.reloadTime);

        int needed = weaponData.clipSize - _currentAmmo;
        int take   = Mathf.Min(needed, _reserveAmmo);
        _currentAmmo  += take;
        _reserveAmmo  -= take;
        _isReloading   = false;
    }

    public void ForceReload()
    {
        if (!_isReloading && _reserveAmmo > 0)
            StartCoroutine(Reload());
    }
}
