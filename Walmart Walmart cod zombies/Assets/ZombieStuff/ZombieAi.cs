using System;
using System.Collections;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.AI;
//navmesh info:
//the agent 
public class ZombieAi : MonoBehaviour, IDamageAble
{
    [SerializeField] entityStatSO stats;
    [SerializeField] float range;
    NavMeshAgent agent;
   
    //Animation variables
    //maybe for the future an animation manager? it would produce cleaner code
    Animation animationer;
    AnimationState[] animationStates = new AnimationState[8];
    
    GameObject player = null;
    bool targetIsBoard = false;
    Coroutine attackCoroutine;

    float health;
    
    void Start()
    {
        health = stats.hp;
        agent = GetComponent<NavMeshAgent>();
        animationer = GetComponent<Animation>();
        int i = 0;
        //get them into an array so we can access for later
        foreach (AnimationState states in animationer) 
        {
             animationStates[i] = states;
             i++;
        }
        //we only need the zombie to jump the window once(play the animation once
        animationer[animationStates[5].name].wrapMode = WrapMode.Once;
        //target the highest hp board
        //hmm if multiple zombies spawn in at the same time they could take the same board which isn't idle
        //so lets make a queue
        //and on the next frame we just dequeue each zombie
        woodenBoardHp woodenBoardScript = WoodenBoardManager.instance.notDeadBoards.RemoveFirst();
        Debug.Log(woodenBoardScript.gameObject.name);
        if (woodenBoardScript != null)
        {
            
            agent.destination = woodenBoardScript.gameObject.transform.position;
            player = woodenBoardScript.gameObject;
            targetIsBoard = true;
        }
        //otherwise target player position
        else 
        {
            agent.destination = player.transform.position;
            player = GameObject.FindGameObjectWithTag("Player");
        }
        TickSystem.frequenttickTime.AddListener(trackPlayerPoistion);
        agent.autoTraverseOffMeshLink = false;    
       
    }


    //tracks the player position; should be called on every frame;
    void trackPlayerPoistion(float time)
    {
       
       trackTarget(player);
    }
    
    void trackTarget(GameObject target) 
    { 
        agent.destination = target.transform.position;
    }
     /*   if (_isDead || _player == null) return;

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
        }*/
    
    /*
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
    */
    private void OnTriggerEnter(Collider other)
    {
        //this should only happen when a board enters the zombies range AND only once so no need to check if theres an coroutine happening

        if (targetIsBoard && (agent.remainingDistance < 1.3f) && other.gameObject.CompareTag("PotentialBoard"))
        {
            agent.updateRotation = false;
            attackCoroutine = StartCoroutine(attackboard(other.gameObject));
        }
        else if (other.gameObject.CompareTag("Player") && attackCoroutine == null) 
        {
            animationer.Stop();
            attackCoroutine = StartCoroutine(attackPlayer(other.gameObject));
        }
        
    }
    
    IEnumerator attackPlayer(GameObject player) 
    {
        
        if (agent.remainingDistance < range)
        {        
            agent.isStopped = true;
            //cancels the current animation and switches to smaking right away;
            animationer.Play(animationStates[0].name);
            agent.velocity = Vector3.zero;
            //player Damage Logic
            //use attack(player,0);
        } 
        yield return new WaitForSeconds(0.25f);
        //if the player get out of range this doesn't happen
        //setting the destination to get the agent.remaining distance
        if (agent.remainingDistance < range)
        {
            attackCoroutine = StartCoroutine(attackPlayer(player));
        }
        else
        {
            animationer.Stop();
            attackCoroutine = null;
            animationer.Play(animationStates[4].name);
            agent.isStopped = false;
            agent.SetDestination(player.transform.position); 
        }

    }
    
    //attacks the board on the window
    //param board: the board gameObject to attack
    IEnumerator attackboard(GameObject board)
    {
        float hpLeft = attack(board,1);
        Debug.Log(board.transform.parent.rotation);
        transform.rotation =Quaternion.Euler(transform.rotation.eulerAngles.x,board.transform.parent.rotation.eulerAngles.y,transform.rotation.eulerAngles.z);
        if (hpLeft <= 0)
        {
            //climb
            targetIsBoard = false;
            attackCoroutine = null;
            StartCoroutine(waitUntilAnimFinishPlaying(animationStates[5], 0));
        }
        else
        {
            //attack again
            yield return new WaitForSeconds(0.75f);
            StartCoroutine(attackboard(board));
        }
        
    }
    float attack(GameObject other, int animationToPlay)
    {
        //0 for player 1 for window
        animationer.Play(animationStates[animationToPlay].name);
        other.gameObject.GetComponent<IDamageAble>().takeDamage(stats.meleeDamage);
        return other.gameObject.GetComponent<IDamageAble>().returnHP();
    }

    //----------------------------------------------------Composition Functions: functions used to make other funcrtions like composition for classes;
    //a reuseable coroutine where the general idea is to wait for an animation to finish and we do an action based off int using switch case
    //param animation: animation to play
    //param actionAfterWards : actoin to do after playing animations
    IEnumerator waitUntilAnimFinishPlaying(AnimationState animation,int actionAfterWards) 
    {
        //animation clips ranges from 0 to 1 if you don't loop
        animationer.Play(animationStates[5].name);
        player = GameObject.FindGameObjectWithTag("Player");
        //wait for animation to finish playing
        yield return new WaitForSeconds(animation.length);
        agent.updateRotation = true;
        switch (actionAfterWards)
        {
            case 0: OffMeshLinkData linkData = agent.currentOffMeshLinkData; agent.CompleteOffMeshLink(); animationer.Play(animationStates[4].name);  break;
        }
       
        
    }

    //------------------------------------------------------------------------------------------------------------------------------------ 

    private void OnDestroy()
    {
        TickSystem.frequenttickTime.RemoveListener(trackPlayerPoistion);
    }

    public float takeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            if (PointsManager.Instance != null)
                PointsManager.Instance.AddPoints(PointsManager.Instance.killPoints);
            if (RoundManager.Instance != null)
                RoundManager.Instance.OnZombieKilled();
            Destroy(gameObject);
        }
        return health;
    }

    public float returnHP()
    {
        return health;
    }
}
