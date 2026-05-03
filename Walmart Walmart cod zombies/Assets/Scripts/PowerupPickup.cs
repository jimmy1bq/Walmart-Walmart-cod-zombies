using System.Collections;
using UnityEngine;

public enum PowerupType { DoublePoints, DoubleTap, MaxAmmo }

public class PowerupPickup : MonoBehaviour
{
    public PowerupType type;
    public AudioSource powerUpSrc;
    public AudioClip auraNoise;
    public AudioClip pickUpNoise;
    public float duration = 30f;
    public float lifetime = 30f;

    static readonly Color[] TypeColors =
    {
        new Color(1f, 0.84f, 0f),   // DoublePoints - gold
        new Color(1f, 0.2f,  0.2f), // DoubleTap    - red
        new Color(0f, 0.8f,  1f),   // MaxAmmo      - cyan
    };

    float _spawnY;
    float _destroyTime;

    private void Awake()
    {
        powerUpSrc = GetComponent<AudioSource>();
        powerUpSrc.clip = auraNoise;
        powerUpSrc.volume = audioManagerZombies.instance.sfxVolume; 
        powerUpSrc.Play();
    }
    void Start()
    {
        _spawnY      = transform.position.y;
        _destroyTime = Time.time + lifetime;

        Light glow = gameObject.AddComponent<Light>();
        glow.type      = LightType.Point;
        glow.color     = TypeColors[(int)type];
        glow.intensity = 2.5f;
        glow.range     = 3f;
    }

    void Update()
    {
        if (Time.time >= _destroyTime) { Destroy(gameObject); return; }

        transform.Rotate(Vector3.up * 90f * Time.deltaTime, Space.World);
        float newY = _spawnY + Mathf.Sin(Time.time * 2.5f) * 0.2f;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PowerupManager pm = PowerupManager.Instance ?? PowerupManager.GetOrCreate();
       
        switch (type)
        {
            case PowerupType.DoublePoints: pm.ActivateDoublePoints(duration); break;
            case PowerupType.DoubleTap:    pm.ActivateDoubleTap(duration);    break;
            case PowerupType.MaxAmmo:      pm.ActivateMaxAmmo();              break;
        }   
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().powerUpsCollected++;
        audioManagerZombies.instance.afterDestroyAudio(transform.position, pickUpNoise);
        Destroy(gameObject);
    }  
    
}
