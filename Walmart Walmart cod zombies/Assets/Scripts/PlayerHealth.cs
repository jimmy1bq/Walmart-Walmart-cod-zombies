using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }

    public UnityEvent<float> onHealthChanged;   // passes current health
    public UnityEvent onDeath;

    bool _isDead;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        onHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        _isDead = true;
        onDeath?.Invoke();
        Debug.Log("Player died.");
        // Disable controls
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;
    }
}
