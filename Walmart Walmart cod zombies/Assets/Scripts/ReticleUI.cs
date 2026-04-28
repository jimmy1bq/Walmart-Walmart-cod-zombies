using UnityEngine;
using UnityEngine.UI;

public class ReticleUI : MonoBehaviour
{
    [Header("Size")]
    public float minSize    = 60f;
    public float maxSize    = 220f;
    public float smoothSpeed = 10f;

    [Header("Appearance")]
    public Color ringColor = new Color(0.25f, 0.25f, 0.25f, 0.9f);

    RectTransform  _rt;
    float          _currentSize;
    PlayerController _player;

    void Start()
    {
        _player      = FindFirstObjectByType<PlayerController>();
        _currentSize = minSize;

        var go = new GameObject("Reticle");
        go.transform.SetParent(transform, false);

        _rt                   = go.AddComponent<RectTransform>();
        _rt.anchorMin         = _rt.anchorMax = new Vector2(0.5f, 0.5f);
        _rt.anchoredPosition  = Vector2.zero;
        _rt.sizeDelta         = new Vector2(minSize, minSize);

        var img     = go.AddComponent<RawImage>();
        img.texture = CreateRingTexture(128, 52, 62);
        img.color   = ringColor;
    }

    static Texture2D CreateRingTexture(int size, float innerR, float outerR)
    {
        var tex    = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var center = new Vector2(size * 0.5f, size * 0.5f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), center);
            tex.SetPixel(x, y, (d >= innerR && d <= outerR) ? Color.white : Color.clear);
        }
        tex.Apply();
        return tex;
    }

    void Update()
    {
        if (_player == null)
        {
            _player = FindFirstObjectByType<PlayerController>();
            return;
        }

        float targetSize = minSize;

        Weapon w = _player.CurrentWeapon;
        if (w != null && w.weaponData != null)
        {
            float t  = Mathf.Clamp01((w.CurrentSpread - w.weaponData.spread) / 25f);
            targetSize = Mathf.Lerp(minSize, maxSize, t);
        }

        CharacterController cc = _player.GetComponent<CharacterController>();
        if (cc != null)
        {
            Vector3 flatVel = new Vector3(cc.velocity.x, 0f, cc.velocity.z);
            if (flatVel.magnitude > 0.5f)
                targetSize = Mathf.Max(targetSize, minSize * 2f);
        }

        _currentSize  = Mathf.Lerp(_currentSize, targetSize, smoothSpeed * Time.deltaTime);
        _rt.sizeDelta = new Vector2(_currentSize, _currentSize);
    }
}
