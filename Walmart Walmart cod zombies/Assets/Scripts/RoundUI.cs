using System.Collections;
using UnityEngine;
using TMPro;


public class RoundUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI roundText;

    [Header("Animation")]
    [Tooltip("Anchored position the text travels to (centre of screen). " +
             "(0,0) works when the text's RectTransform anchor is at the canvas centre.")]
    public Vector2 centerPosition = Vector2.zero;

    [Tooltip("Scale of the text while it sits in the corner.")]
    public float homeScale = 6f;

    [Tooltip("Scale the text grows to when it reaches the centre.")]
    public float centerScale = 6f;

    [Tooltip("Seconds to travel from corner to centre (and back).")]
    public float travelDuration = 0.5f;

    [Tooltip("Seconds the text stays large at the centre before returning.")]
    public float holdDuration = 1.5f;

    RectTransform _rt;
    Vector2       _homePosition;
    Coroutine     _animRoutine;

    void Awake()
    {
        _rt = roundText.GetComponent<RectTransform>();
        // Wherever the designer placed the text becomes the permanent home
        _homePosition = _rt.anchoredPosition;
        homeScale = 3f;
        centerScale = 4f;
        centerPosition = new Vector2(197, -118);
    }

    void Start()
    {
        roundText.color = Color.red;
        roundText.text  = "";

        if (RoundManager.Instance != null)
            RoundManager.Instance.onRoundStart.AddListener(OnRoundStart);
    }

    void OnDestroy()
    {
        if (RoundManager.Instance != null)
            RoundManager.Instance.onRoundStart.RemoveListener(OnRoundStart);
    }

    void OnRoundStart(int round)
    {
        roundText.text = round.ToString();

        if (_animRoutine != null) StopCoroutine(_animRoutine);
         _animRoutine = StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        Vector3 orignalPos = gameObject.transform.position;
        Vector3 originalScale = gameObject.transform.localScale;
        // Slide to centre and grow
        yield return StartCoroutine(Tween(_homePosition, centerPosition, homeScale, centerScale, travelDuration));

        // Hold at centre
        yield return new WaitForSeconds(holdDuration);

        // Slide back to corner and shrink
        yield return StartCoroutine(Tween(centerPosition, _homePosition, centerScale, homeScale, travelDuration));
    }

    IEnumerator Tween(Vector2 fromPos, Vector2 toPos, float fromScale, float toScale, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            _rt.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
            _rt.localScale       = Vector3.one * Mathf.Lerp(fromScale, toScale, t);
            yield return null;
        }
        _rt.anchoredPosition = toPos;
        _rt.localScale       = Vector3.one * toScale;
    }
    
}
