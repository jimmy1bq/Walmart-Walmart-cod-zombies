using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
//navmesh info:
//the agent 
public class ZombieAi : MonoBehaviour, IDamageAble, IQueue
{
    //entity stats like range agent and targeys
    [SerializeField] entityStatSO stats;
    [SerializeField] float range;
    float health;
    NavMeshAgent agent;
    NavMeshLink link;
    //---------------------------targets
    GameObject queuePosition = null;
    woodenBoardHp targetWindow = null;
    GameObject player = null;
    GameObject board = null;

    //Animation variables
    Animation animationer;
    AnimationState[] animationStates = new AnimationState[8];

    bool targetIsBoard = false;

    Coroutine attackCoroutine;


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

        //we only need the zombie to jump the window once(play the animation once)
        //don't need to loop attacl
        animationer[animationStates[5].name].wrapMode = WrapMode.Once;
        animationer[animationStates[1].name].wrapMode = WrapMode.Once;

        //targets a random window in the spawn area
        //so like if the zombie spawn in the back we would target back windows
        targetWindow = WoodenBoardManager.instance.randomQueue();
        queuePosition = targetWindow.addZombieOntoQueue(gameObject);
        if (targetWindow != null)
        {
            agent.destination = queuePosition.transform.position;
            board = queuePosition;
            StartCoroutine(onPosition());
            targetIsBoard = true;
        }

        //otherwise target player position
        //remove this later because we are going to make a coroutine to make the zombie wait until theres an aviable window
        //this should not happen but just in case yeahs
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
        if (player != null)
        {
            trackTarget(player);
        }

    }

    void trackTarget(GameObject target)
    {    
        agent.destination = target.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {

        //this should only happen when a board enters the zombies range AND only once so no need to check if theres an coroutine happening
        //if the board has more than 0 hp we attack
        if (targetIsBoard && (agent.remainingDistance < 1f) && other.gameObject.CompareTag("PotentialBoard") && other.gameObject.GetComponent<IDamageAble>().returnHP() >= 0)
        {
            animationer.Play(animationStates[3].name);
            agent.updateRotation = false;
            attackCoroutine = StartCoroutine(attackboard(other.gameObject));
        }

        //if the board doesn't have any hp we can skip the attack
        else if (targetIsBoard && (agent.remainingDistance < 1f) && other.gameObject.CompareTag("PotentialBoard") && other.gameObject.GetComponent<IDamageAble>().returnHP() <= 0)
        {
            animationer.Play(animationStates[3].name);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, other.transform.parent.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            StartCoroutine(waitUntilAnimFinishPlaying(animationStates[5], 0));
        }

        //if its the player we attack the player
        else if (other.gameObject.CompareTag("Player") && attackCoroutine == null)
        {
            animationer.Stop();
            attackCoroutine = StartCoroutine(attackPlayer(other.gameObject));
        }

    }
    //attacks the player if the zombie is close enough 
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



    //ok so funny story(not):
    //apprently unity doesn't realize if the agent is on a link if the agent is not using the link to the other side
    //The agent can literally be on the link and unity will still say NO NOT ONE LINK
    //so basically agent have to decide that its on the link in order to use it

    //attacks the board on the window(yes 2 seconds to register that it killed the board to jump over it apprently)
    //param board: the board gameObject to attack

    IEnumerator attackboard(GameObject board)
    {
        yield return new WaitForSeconds(2.0f);
        float hpLeft = attack(board, 1);


        link = board.transform.parent.GetComponent<NavMeshLink>();
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
            StartCoroutine(attackboard(board));
        }

    }

    //attacks the gameobject "other" and plays the animation based on the given int
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

    //param actionAfterWards: predetermined action determined by the switch case
    IEnumerator waittingSimulator(int actionAfterWards)
    {
        yield return new WaitForFixedUpdate();
        agent.updateRotation = true;
        switch (actionAfterWards)
        {
            case 0:
                agent.isStopped = false;
                player = GameObject.FindGameObjectWithTag("Player");
                GameObject endPoint = link.gameObject.transform.Find("p2").gameObject;
                agent.Warp(link.transform.TransformPoint(link.endPoint));
                animationer.Play(animationStates[4].name);
                targetWindow.moveQueueUp();
                break;

        }
    }

    //------------------------------------------------------------------------------------------------------------------------------------ 

    private void OnDestroy()
    {
        TickSystem.frequenttickTime.RemoveListener(trackPlayerPoistion);
    }

    //takes damage from something
    public float takeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
        return health;
    }

    //once the zombie is finished(animation and over the wall) we have to tell the zombie that its position has been update and now he should advance onto the next point
    //and wait there once they reach it(or idle and climb over the window if its at window position)
    //window position is handled by collision
    public void updateQueuePoistion(GameObject positionToMoveTo)
    {
        board = positionToMoveTo;
        animationer.Play(animationStates[4].name);
        agent.SetDestination(positionToMoveTo.transform.position);
        StartCoroutine(onPosition());
    }

    //if its on the poistion play idle animation
    IEnumerator onPosition()
    {
        //i just realized that coroutine can be used like a tick system but since this multithreads don't turn this into a update logic method
        while (true)
        {
            if ((gameObject.transform.position - board.transform.position).magnitude < 0.5f)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, board.transform.parent.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                animationer.Play(animationStates[3].name);
                break;
            }
            yield return null;
        }
    }

    //returns the hp left
    public float returnHP()
    {
        return health;
    }
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

