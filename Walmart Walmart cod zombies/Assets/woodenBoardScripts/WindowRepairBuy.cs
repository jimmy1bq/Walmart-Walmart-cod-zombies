using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class WindowRepairBuy : MonoBehaviour
{
    [Header("Windows")]
    public woodenBoardHp[] windows;

    [Header("Cost")]
    public int cost = 1500;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    public GameObject promptPanel;
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI cantAffordText;

    PlayerController _playerInRange;
    float _cantAffordTimer;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        SetPromptVisible(false);
        if (cantAffordText != null) cantAffordText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_playerInRange == null) return;

        if (Input.GetKeyDown(interactKey))
            TryRepair();

        if (_cantAffordTimer > 0f)
        {
            _cantAffordTimer -= Time.deltaTime;
            if (_cantAffordTimer <= 0f && cantAffordText != null)
                cantAffordText.gameObject.SetActive(false);
        }
    }

    void TryRepair()
    {
        if (PointsManager.Instance == null) return;

        if (!PointsManager.Instance.TrySpend(cost))
        {
            ShowCantAfford();
            return;
        }

        foreach (woodenBoardHp window in windows)
        {
            if (window != null)
                window.repairAll();
        }
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
        if (promptText != null)
            promptText.text = $"[{interactKey}]  Repair All Windows  –  {cost} pts";
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
}
