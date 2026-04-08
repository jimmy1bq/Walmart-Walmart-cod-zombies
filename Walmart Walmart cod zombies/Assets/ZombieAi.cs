using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 150f;
    float _currentHealth;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4.5f;

    [Header("Melee Attack")]
    public float attackRange = 1.5f;
    public float attackDamage = 15f;
    public float attackCooldown = 1.2f;

    NavMeshAgent _agent;
    Transform _player;
    PlayerHealth _playerHealth;
    Animator _animator;

    float _nextAttackTime;
    bool _isDead;

    // Animator parameter hashes (optional – works without an Animator too)
    static readonly int HashWalk    = Animator.StringToHash("Walk");
    static readonly int HashAttack  = Animator.StringToHash("Attack");
    static readonly int HashDead    = Animator.StringToHash("Dead");

    void Awake()
    {
        _agent    = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _currentHealth = maxHealth;
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player      = playerObj.transform;
            _playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (_isDead || _player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist <= attackRange)
        {
            _agent.isStopped = true;
            SetWalkAnim(false);
            TryMeleeAttack();
        }
        else
        {
            _agent.isStopped = false;
            _agent.speed = dist > 8f ? runSpeed : walkSpeed;
            _agent.SetDestination(_player.position);
            SetWalkAnim(true);
        }
    }

    void TryMeleeAttack()
    {
        if (Time.time < _nextAttackTime) return;
        _nextAttackTime = Time.time + attackCooldown;

        if (_animator != null)
            _animator.SetTrigger(HashAttack);

        _playerHealth?.TakeDamage(attackDamage);
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. HP: {_currentHealth}/{maxHealth}");

        if (_currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        _isDead = true;
        _agent.isStopped = true;

        if (_animator != null)
            _animator.SetTrigger(HashDead);

        // Disable collider so bullets pass through
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 3f);
    }

    void SetWalkAnim(bool walking)
    {
        if (_animator != null)
            _animator.SetBool(HashWalk, walking);
    }

    // Visualise attack range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
