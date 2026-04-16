using System;
using System.Collections;
using Unity.AI.Navigation;
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
    NavMeshLink link;
    OffMeshLinkData link2;

    //Animation variables
    //maybe for the future an animation manager? it would produce cleaner code
    Animation animationer;
    AnimationState[] animationStates = new AnimationState[8];

    GameObject player = null;
    bool targetIsBoard = false;
    bool isNotAnimating = true;
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

        if (woodenBoardScript != null)
        {
            agent.destination = woodenBoardScript.gameObject.transform.parent.Find("p1").transform.position;
            player = woodenBoardScript.gameObject.transform.parent.Find("p1").transform.gameObject;
           
            targetIsBoard = true;
        }
        //otherwise target player position
        else
        {
            targetIsBoard = false;
            player = GameObject.FindGameObjectWithTag("Player");
            agent.destination = player.transform.position;

        }
        TickSystem.frequenttickTime.AddListener(trackPlayerPoistion);
        agent.autoTraverseOffMeshLink = false;

    }


    //tracks the player position; should be called on every frame;
    void trackPlayerPoistion(float time)
    {
     //   Debug.Log("zombie: "+gameObject.name + " target: " + player.name + " disLeft: " + agent.remainingDistance);
      
        if (player != null) 
        {
            trackTarget(player);
        }
       
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
        
        if (targetIsBoard && (agent.remainingDistance < 1f) && other.gameObject.CompareTag("PotentialBoard") && isNotAnimating)
        {
            agent.updateRotation = false;
            attackCoroutine = StartCoroutine(attackboard(other.gameObject));
        }
        else if (!targetIsBoard && other.gameObject.CompareTag("PotentialBoard") && (agent.remainingDistance < 1.3f))
        {
            if (other.gameObject.GetComponent<IDamageAble>().returnHP() <= 0)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, other.transform.parent.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                StartCoroutine(waitUntilAnimFinishPlaying(animationStates[5], 0));
            } else if (other.gameObject.GetComponent<IDamageAble>().returnHP() >= 0)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, other.transform.parent.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                StartCoroutine(wait(other.gameObject));
            }

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
    //waits
    IEnumerator wait(GameObject board)
    {
        //waits until the board is dead
        
        yield return new WaitUntil(() => board.GetComponent<IDamageAble>().returnHP() <= 0);
        StartCoroutine(waitUntilAnimFinishPlaying(animationStates[5], 0));
        agent.SetDestination(player.transform.position);
    }
    //attacks the board on the window
    //param board: the board gameObject to attack
    IEnumerator attackboard(GameObject board)
    {
        float hpLeft = attack(board, 1);
        //ok so funny story(not):
        //apprently unity doesn't realize if the agent is on a link if the agent is not using the link to the other side
        //The agent can literally be on the link and unity will still say NO NOT ONE LINK
        isNotAnimating = false;
        link = board.transform.parent.GetComponent<NavMeshLink>();
        player = board.transform.parent.GetChild(2).gameObject;
        agent.isStopped = true;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, board.transform.parent.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
     
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
    IEnumerator waitUntilAnimFinishPlaying(AnimationState animation, int actionAfterWards)
    {

        
        //animation clips ranges from 0 to 1 if you don't loop
        animationer.Play(animation.name);
        //wait for animation to finish playing
       
           
       
        yield return new WaitForSeconds(animation.length);
        
        StartCoroutine(waittingSimulator(actionAfterWards));


    }
    IEnumerator waittingSimulator(int actionAfterWards)
    {
        yield return new WaitForFixedUpdate();
        agent.updateRotation = true;
        switch (actionAfterWards)
        {
            case 0:
                player = GameObject.FindGameObjectWithTag("Player");
                GameObject endPoint = link.gameObject.transform.Find("p2").gameObject;
                /* endPoint.transform.parent = null;
                 Debug.Log( endPoint.transform.position);
                 gameObject.transform.position = endPoint.transform.position;
                 endPoint.transform.parent = link.gameObject.transform;*/
                //welp best I can do because it seems like theres no force complete on a link when you have  alink
                agent.Warp(link.transform.TransformPoint(link.endPoint));
                animationer.Play(animationStates[4].name);
                isNotAnimating = true;
                
              
                
                break;
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
            Destroy(gameObject);
        }
         return health;
    }

    public float returnHP()
    {
        return health;
    }
}
