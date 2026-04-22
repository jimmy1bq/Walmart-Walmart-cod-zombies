using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class interactables : MonoBehaviour
{
    [Header("Cost")]
    public int cost = 750;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Audio")]
    public AudioClip clearSound;

    [Header("Screen-Space Prompt (HUD Canvas)")]
    public GameObject promptPanel;
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI cantAffordText;

    PlayerController _playerInRange;
    float _cantAffordTimer;
    bool _purchased;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        SetPromptVisible(false);
        if (cantAffordText != null) cantAffordText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_playerInRange == null || _purchased) return;

        if (Input.GetKeyDown(interactKey))
            TryPurchase();

        if (_cantAffordTimer > 0f)
        {
            _cantAffordTimer -= Time.deltaTime;
            if (_cantAffordTimer <= 0f && cantAffordText != null)
                cantAffordText.gameObject.SetActive(false);
        }
    }

    void TryPurchase()
    {
        if (PointsManager.Instance == null) return;

        if (!PointsManager.Instance.TrySpend(cost))
        {
            ShowCantAfford();
            return;
        }

        _purchased = true;
        SetPromptVisible(false);

        StartCoroutine(PlaySoundThenDestroy());
    }

    IEnumerator PlaySoundThenDestroy()
    {
        if (clearSound != null)
        {
            // Spawn a temporary AudioSource so the sound survives the destroy
            GameObject soundObj = new GameObject("ObstructionClearSound");
            soundObj.transform.position = transform.position;
            AudioSource src = soundObj.AddComponent<AudioSource>();
            src.clip = clearSound;
            src.Play();
            Destroy(soundObj, clearSound.length + 0.1f);
        }

        Destroy(gameObject);
        yield break;
    }

    void ShowCantAfford()
    {
        if (cantAffordText == null) return;
        cantAffordText.text = "Not enough points!";
        cantAffordText.gameObject.SetActive(true);
        _cantAffordTimer = 2f;
    }

    void SetPromptVisible(bool visible)
    {
        if (promptPanel != null) promptPanel.SetActive(visible);
        RefreshPrompt();
    }

    void RefreshPrompt()
    {
        if (promptText != null)
            promptText.text = $"[{interactKey}]  Clear Obstruction  –  {cost} pts";
    }

    void OnTriggerEnter(Collider other)
    {
        if (_purchased) return;
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
}
