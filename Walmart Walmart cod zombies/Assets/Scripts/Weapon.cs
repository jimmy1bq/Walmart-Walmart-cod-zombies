using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;

    [Header("Visuals / Audio")]
    public Transform muzzlePoint;
    public Sprite[] muzzleFlashSprites;   // Drag muzzle_flash_1/2/3 here
    public float muzzleFlashScale = 0.3f;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    [Header("Bullet Tracer")]
    public Color tracerColor = new Color(1f, 0.9f, 0.5f, 1f);
    public float tracerWidth = 0.015f;
    public float tracerDuration = 0.06f;

    int _currentAmmo;
    int _reserveAmmo;
    float _nextFireTime;
    bool _isReloading;
    public float _reloadEndTime;
    public float _reloadStartTime;
    float _currentSpread;
    AudioSource _audio;
    Animation animator;

    public int CurrentAmmo     => _currentAmmo;
    public int ReserveAmmo     => _reserveAmmo;
    public bool IsReloading    => _isReloading;
    public float CurrentSpread => _currentSpread;

    void Awake()
    {
        animator = GetComponent<Animation>();
        if (animator != null) 
        {          
            animator.GetClip("weaponrecoilAnim").wrapMode = WrapMode.Once;
        }     
        _audio = GetComponent<AudioSource>();
        Initialize();
    }

    public void Initialize()
    {
        if (weaponData == null) return;
        _currentAmmo   = weaponData.clipSize;
        _reserveAmmo   = weaponData.maxAmmo - weaponData.clipSize;
        _currentSpread = weaponData.spread;
        _isReloading   = false;
    }

    void OnEnable()
    {
        _isReloading   = false;
        _currentSpread = weaponData != null ? weaponData.spread : 0f;
    }

    void Update()
    {
        if (weaponData != null && _currentSpread > weaponData.spread)
        {
            _currentSpread -= weaponData.spreadRecoveryRate * Time.deltaTime;
            _currentSpread  = Mathf.Max(_currentSpread, weaponData.spread);
        }
    }

    // Called every frame by PlayerController regardless of this object's active state.
    public void TickReload()
    {
        if (!_isReloading || weaponData == null) return;
        if (Time.time < _reloadEndTime) {  return; }

        int needed    = weaponData.clipSize - _currentAmmo;
        int take      = Mathf.Min(needed, _reserveAmmo);
        _currentAmmo += take;
        _reserveAmmo -= take;
        _reloadStartTime = 0f;
        _isReloading  = false;
    }

    void BeginReload()
    {
        if (_isReloading || _reserveAmmo <= 0 || weaponData == null) return;
        if (_currentAmmo == weaponData.clipSize) return;
        _isReloading  = true;
        // _reloadEndTime = Time.time + weaponData.reloadTime;
        _reloadEndTime = Time.time + weaponData.reloadTime;
        _reloadStartTime = Time.time;
        if (_audio != null && reloadSound != null)
            _audio.PlayOneShot(reloadSound);
    }

    public void TryShoot(Transform aimOrigin)
    {
        if (_isReloading) return;
        if (Time.time < _nextFireTime) return;

        if (_currentAmmo <= 0)
        {
            if (_audio != null && emptySound != null)
                _audio.PlayOneShot(emptySound);
            return;
        }

        Shoot(aimOrigin);
    }
    public void RefillAmmo()
    {
        if (weaponData == null) return;
        _currentAmmo  = weaponData.clipSize;
        _reserveAmmo  = weaponData.maxAmmo - weaponData.clipSize;
        _isReloading  = false;
    }

    //check tag for headshot; if its the head then zombie takeDamage should be (weaponData.damage,1)
    void Shoot(Transform aimOrigin)
    {
        _currentAmmo--;
        _nextFireTime = Time.time + weaponData.fireRate;

        bool doubleTap = PowerupManager.Instance != null && PowerupManager.Instance.IsDoubleTap;
        int shots = doubleTap ? 2 : 1;
        if (_audio != null && shootSound != null)
        {
            _audio.PlayOneShot(shootSound);
            if (animator != null) 
            {
                animator.Play("weaponrecoilAnim");
            }
        }


        for (int s = 0; s < shots; s++)
        for (int i = 0; i < weaponData.pelletCount; i++)
        {
            Vector3 direction = ApplySpread(aimOrigin.forward);
            Ray ray = new Ray(aimOrigin.position, direction);
            Vector3 tracerEnd = ray.origin + ray.direction * weaponData.range;

            RaycastHit[] hits = Physics.RaycastAll(ray, weaponData.range);

            Debug.DrawRay(ray.origin, ray.direction * weaponData.range, Color.red, 1f);

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit hit in hits)
            {
              
                IDamageAble damageable = hit.collider.GetComponentInParent<IDamageAble>();

                // Skip non-damageable mesh/box colliders — walls, window frames, stairs, etc.
                if (damageable == null && (hit.collider is MeshCollider || hit.collider is BoxCollider)) continue;

                // Skip window boards — they're damaged only by zombies, not bullets
                if (hit.collider.CompareTag("PotentialBoard")) continue;

                if (damageable != null)
                {
                    int damageType = hit.collider.CompareTag("Head") ? 1 : 0;
                    damageable.takeDamage(weaponData.damage, damageType);
                }
                tracerEnd = hit.point;
                break;
            }

            StartCoroutine(SpawnTracer(ray.origin, tracerEnd));
        }

        _currentSpread += weaponData.spreadGainPerShot;

        if (muzzleFlashSprites != null && muzzleFlashSprites.Length > 0 && muzzlePoint != null)
        {
            Sprite sprite = muzzleFlashSprites[Random.Range(0, muzzleFlashSprites.Length)];
            GameObject flash = new GameObject("MuzzleFlash");
            flash.transform.position = muzzlePoint.position;
            flash.transform.rotation = muzzlePoint.rotation;
            flash.transform.localScale = Vector3.one * muzzleFlashScale;

            SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;

            Destroy(flash, 0.05f);
        }

      

    }

    IEnumerator SpawnTracer(Vector3 start, Vector3 end)
    {
        GameObject obj = new GameObject("BulletTracer");
        LineRenderer lr = obj.AddComponent<LineRenderer>();

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = tracerColor;
        lr.endColor   = new Color(tracerColor.r, tracerColor.g, tracerColor.b, 0f);
        lr.startWidth = tracerWidth;
        lr.endWidth   = tracerWidth * 0.1f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        float elapsed = 0f;
        while (elapsed < tracerDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / tracerDuration;
            Color fadeStart = new Color(tracerColor.r, tracerColor.g, tracerColor.b, 1f - t);
            Color fadeEnd   = new Color(tracerColor.r, tracerColor.g, tracerColor.b, 0f);
            lr.startColor = fadeStart;
            lr.endColor   = fadeEnd;
            yield return null;
        }

        Destroy(obj);
    }

    Vector3 ApplySpread(Vector3 forward)
    {
        float halfAngle = _currentSpread * 0.5f;
        float x = Random.Range(-halfAngle, halfAngle);
        float y = Random.Range(-halfAngle, halfAngle);
        return Quaternion.Euler(x, y, 0f) * forward;
    }

    public void ForceReload() => BeginReload();
}
