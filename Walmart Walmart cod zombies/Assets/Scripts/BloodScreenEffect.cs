using UnityEngine;
using UnityEngine.UI;

public class BloodScreenEffect : MonoBehaviour
{
    public static BloodScreenEffect Instance;

    Image _overlay;
    float _baseAlpha;
    float _flashAlpha;

    const float FlashDecay = 3.5f;
    const float MaxBaseAlpha = 0.92f;

    void Awake()
    {
        Instance = this;

        var canvasGO = new GameObject("BloodScreenCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        canvasGO.AddComponent<CanvasScaler>();

        var imgGO = new GameObject("BloodOverlay");
        imgGO.transform.SetParent(canvasGO.transform, false);
        _overlay = imgGO.AddComponent<Image>(); // AddComponent creates the RectTransform
        var rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        _overlay.sprite = CreateVignetteSprite(256);
        _overlay.color = new Color(0.8f, 0f, 0f, 0f);
        _overlay.raycastTarget = false;
    }

    void Update()
    {
        _flashAlpha = Mathf.MoveTowards(_flashAlpha, 0f, FlashDecay * Time.unscaledDeltaTime);
        Color c = _overlay.color;
        c.a = Mathf.Clamp01(_baseAlpha + _flashAlpha);
        _overlay.color = c;
    }

    public void OnHit(int hitsRemaining, int maxHits)
    {
        _baseAlpha = (float)(maxHits - hitsRemaining) / maxHits * MaxBaseAlpha;
        _flashAlpha = 0.6f;
    }

    public void OnRegen(int hitsRemaining, int maxHits)
    {
        _baseAlpha = hitsRemaining >= maxHits ? 0f : (float)(maxHits - hitsRemaining) / maxHits * MaxBaseAlpha;
    }

    public void ResetEffect()
    {
        _baseAlpha = 0f;
        _flashAlpha = 0f;
    }

    static Sprite CreateVignetteSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        var pixels = new Color[size * size];
        float half = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = Mathf.Abs((x - half) / half);
                float ny = Mathf.Abs((y - half) / half);
                // Rectangular distance so all four edges bleed in evenly
                float dist = Mathf.Max(nx, ny);
                // Vignette is transparent inside 15%, ramps up to full at the edge
                float alpha = Mathf.Clamp01((dist - 0.15f) / 0.85f);
                alpha *= alpha; // quadratic falloff for a softer inner edge
                pixels[y * size + x] = new Color(0.8f, 0f, 0f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
