using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
//navmesh info:
//the agent 
public class ZombieAi : MonoBehaviour, IDamageAble, IQueue
{
    //entity stats like range agent and targeys
    [SerializeField] GameObject head;
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
    bool isWalking = false;
    bool isClmbingANDOutside = false;
    bool collided = false;
    ZombieSpawnPosition spawnPosition;

    int groanChance = 0;
    int groanTheresHold = 100;
    float timer = 0;

    Coroutine attackCoroutine;

    AudioSource zombieSrc;
    AudioSource zombieFootStepSrc;

    //i can't find the graon interval so Im going to assume every 3 second it has an 100% chance to groan if it hasn't already

    void Start()
    {
        head = transform.GetChild(0).gameObject.transform.Find("Head").gameObject;
        ZombieSpawnPosition[] spawnPositions = (ZombieSpawnPosition[])Enum.GetValues(typeof(ZombieSpawnPosition));
        spawnPosition = spawnPositions[UnityEngine.Random.Range(0, spawnPositions.Length - 1)];
        
        AudioSource[] arrayOfSrcs = GetComponents<AudioSource>();
        zombieSrc = arrayOfSrcs[0];
        zombieFootStepSrc = arrayOfSrcs[1];
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
        animationer[animationStates[7].name].wrapMode = WrapMode.Once;
        animationer[animationStates[6].name].wrapMode = WrapMode.Once;
        animationer[animationStates[5].name].wrapMode = WrapMode.Once;
        animationer[animationStates[1].name].wrapMode = WrapMode.Once;
        animationer[animationStates[0].name].wrapMode = WrapMode.Once;
       
        //targets a random window in the spawn area
        //so like if the zombie spawn in the back we would target back windows
        targetWindow = WoodenBoardManager.instance.randomQueue(spawnPosition);
        queuePosition = targetWindow.addZombieOntoQueue(gameObject);
        if (targetWindow != null)
        {
            agent.destination = queuePosition.transform.position;
            board = queuePosition;
            isWalking = true;
            StartCoroutine(onPosition(queuePosition));
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
        TickSystem.tickEvent.AddListener(groan);
        agent.autoTraverseOffMeshLink = false;
        
    }


    //tracks the player position; should be called on every frame;
    void trackPlayerPoistion(float time)
    {
        if (isWalking) 
        {
            audioManagerZombies.instance.playZombieWalkingSound(zombieFootStepSrc, gameObject.transform.position, 50);
        }
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

        IDamageAble damageAble = other.gameObject.GetComponent<IDamageAble>();
        //this should only happen when a board enters the zombies range AND only once so no need to check if theres an coroutine happening
        //if the board has more than 0 hp we attack
        if (damageAble != null)
        {
            if (targetIsBoard && (agent.remainingDistance < 1f) && other.gameObject.CompareTag("PotentialBoard") && other.gameObject.GetComponent<IDamageAble>().returnHP() > 0 && !collided)
            {
                collided = true;
                animationer.Play(animationStates[3].name);
                agent.updateRotation = false;
                attackCoroutine = StartCoroutine(attackboard(other.gameObject));
            }

            //if the board doesn't have any hp we can skip the attack
            else if (targetIsBoard && (agent.remainingDistance < 1f) && other.gameObject.CompareTag("PotentialBoard") && other.gameObject.GetComponent<IDamageAble>().returnHP() <= 0 && !collided)
            {
                collided = true;
                animationer.Play(animationStates[3].name);
                link = other.transform.parent.GetComponent<NavMeshLink>();
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, other.transform.parent.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                StartCoroutine(waitUntilAnimFinishPlaying(animationStates[5], 0, 2, other.gameObject));
            }
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
            isWalking = false;
            //cancels the current animation and switches to smaking right away;
            animationer.Play(animationStates[0].name);
            audioManagerZombies.instance.playRandomZombieSound(zombieSrc,gameObject.transform.position,50,audioManagerZombies.instance.zombieAttackClips,1);
            agent.velocity = Vector3.zero;
            //player Damage Logic
            //use attack(player,0);
            //player is damaged first before the animation finish playing
        }
        yield return new WaitForSeconds(2.00f);

        //if the player get out of range this doesn't happen
        //setting the destination to get the agent.remaining distance
        if (agent.remainingDistance < range)
        {
            attackCoroutine = StartCoroutine(attackPlayer(player));
        }
        else
        {
            isWalking = true;
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

    //not to get confused with the variable board; boards is the acutal window board
    IEnumerator attackboard(GameObject boards)
    {
        link = boards.transform.parent.GetComponent<NavMeshLink>();
        isWalking = false;
        agent.isStopped = true;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, boards.transform.parent.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        yield return new WaitForSeconds(2.0f);
        float hpLeft = attack(boards, 1);
       

    }
    //when the zombie climbs the board the first x second its considered outside so we have to play climb out death animation if it dies
    IEnumerator climbBoardTimer(AnimationState animation,int timeTowait) 
    {
        yield return new WaitForSeconds(timeTowait);
        while (true) 
        {
            //this is so peak
            //for the first 0.25f second ish the zombie is climbing the window and outside and if it dies should play the climb window out death anim
            isClmbingANDOutside = true;
            timer += Time.deltaTime;
            if (timer>0.25f) { Debug.Log("INSIDE"); isClmbingANDOutside = false; break;}
            yield return new WaitForFixedUpdate();
        }
    }
    //attacks the gameobject "other" and plays the animation based on the given int
    float attack(GameObject other, int animationToPlay)
    {
        List<AudioClip> zombieClip = audioManagerZombies.instance.zombieAttackClips;
        //0 for player 1 for window
        audioManagerZombies.instance.playRandomZombieSound(zombieSrc, gameObject.transform.position, 50, zombieClip,1);
        StartCoroutine(waitUntilAnimFinishPlaying(animationStates[animationToPlay], 1 , 0 , other));
        return other.gameObject.GetComponent<IDamageAble>().returnHP();
    }

    //----------------------------------------------------Composition Functions: functions used to make other funcrtions like composition for classes;
    //a reuseable coroutine where the general idea is to wait for an animation to finish and we do an action based off int using switch case
    //param animation: animation to play
    //param actionAfterWards : actoin to do after playing animations
    //param wairPeriod: Time to wait for animation
    //param other: gameObject to do something with
    IEnumerator waitUntilAnimFinishPlaying(AnimationState animation, int actionAfterWards,int waitPeriod,GameObject other)
    {     
        yield return new WaitForSeconds(waitPeriod);    
        //animation clips ranges from 0 to 1 if you don't loop
        animationer.Play(animation.name);
        
        //wait for animation to finish playing
        switch (actionAfterWards) 
        {
            case 0: yield return new WaitForSeconds(animation.length); break;

            case 1: yield return new WaitForSeconds(animation.length-0.5f); break;
        }
       
        StartCoroutine(waittingSimulator(actionAfterWards,other));


    }

    //param actionAfterWards: predetermined action determined by the switch case
    IEnumerator waittingSimulator(int actionAfterWards,GameObject directedGameObject)
    {
        yield return new WaitForFixedUpdate();
        agent.updateRotation = true;
        switch (actionAfterWards)
        {
            case 0:
               
                agent.isStopped = false;
                player = GameObject.FindGameObjectWithTag("Player");             
                GameObject endPoints = link.gameObject.transform.Find("p2").gameObject;
                endPoints.transform.parent = null;
                //for some reason unity's navmesh is high as hell and apprently doesn't get link.endpoint right
                agent.Warp(endPoints.transform.position);
                //agent.Warp(link.transform.TransformPoint(link.endPoint));
                endPoints.transform.parent = link.transform;

                animationer.Play(animationStates[4].name);
                targetWindow.moveQueueUp();
                isWalking= true;    
                break;

            case 1:
                ///ignore the int args
                float hpLeft = directedGameObject.GetComponent<IDamageAble>().takeDamage(stats.meleeDamage,0);
                if (hpLeft <= 0)
                {
                    //climb
                    targetIsBoard = false;
                    attackCoroutine = null;
                    StartCoroutine(waitUntilAnimFinishPlaying(animationStates[5], 0 , 2 , board));
                    StartCoroutine(climbBoardTimer(animationStates[5], 2));
                }
                else
                {
                    
                    StartCoroutine(attackboard(directedGameObject));
                }
                break;
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------------ 

    private void OnDestroy()
    {
        TickSystem.frequenttickTime.RemoveListener(trackPlayerPoistion);
    }

    //takes damage from something
    //check if its correct the damageType
    public float takeDamage(float damage,int damageType)
    {
        bool headShotkIll = false;
        //1 for headshots
        switch (damageType) 
        {
            case 0:  health-=damage;  headShotkIll = false; break;
            case 1: health -= damage*2.5f; headShotkIll = true; break;
        }
        if (health <= 0)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            if (PointsManager.Instance != null)
                PointsManager.Instance.AddPoints(PointsManager.Instance.killPoints);
            if (RoundManager.Instance != null)
                RoundManager.Instance.OnZombieKilled();
            StartCoroutine(zombieDeath(headShotkIll));
        }
        return health;
    }
    IEnumerator zombieDeath(bool headShotKill) 
    {

        Debug.Log(headShotKill);
        if (headShotKill)
        {
            Destroy(head);
            GameObject gibParticle = transform.GetChild(0).transform.Find("gib").transform.gameObject;
            gibParticle.SetActive(true);
        }
        if (isClmbingANDOutside)
        {
            animationer.Play(animationStates[7].name);
        }
        else {animationer.Play(animationStates[6].name);}
      
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);

    }

    //once the zombie is finished(animation and over the wall) we have to tell the zombie that its position has been update and now he should advance onto the next point
    //and wait there once they reach it(or idle and climb over the window if its at window position)
    //window position is handled by collision
    public void updateQueuePoistion(GameObject positionToMoveTo)
    {
    
        animationer.Play(animationStates[4].name);
        agent.SetDestination(positionToMoveTo.transform.position);
        StartCoroutine(onPosition(positionToMoveTo));
    }

    //if its on the poistion play idle animation
    IEnumerator onPosition(GameObject positionToMove)
    {
        //i just realized that coroutine can be used like a tick system but since this multithreads don't turn this into a update logic method
        while (true)
        {

            if ((gameObject.transform.position - positionToMove.transform.position).magnitude < 0.5f)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, targetWindow.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                isWalking = false;
                animationer.Play(animationStates[3].name);
                break;
            }
            yield return null;
        }
    }
    //polish and adjust this later
    void groan(float time) 
    {
        //only groan when the zombie is walking
        if (isWalking)
        {
            groanChance += 1;
            if (groanChance >= groanTheresHold)
            {
                audioManagerZombies.instance.playRandomZombieSound(zombieSrc, gameObject.transform.position, 50f, audioManagerZombies.instance.zombieGroanClips, 1.1f);
            }
            else
            {
                int rng = UnityEngine.Random.Range(0, groanTheresHold);
                if (groanChance >= rng)
                {
                    audioManagerZombies.instance.playRandomZombieSound(zombieSrc, gameObject.transform.position, 50f, audioManagerZombies.instance.zombieGroanClips, 1.1f);
                }
            }
        }
    }
    //tells the which position it spawns in. Happens before start. Awake->interfaces->start.
    //this is important because we need to know the spawn position before start to determine which window to target
    public void zombieSpawnPos(ZombieSpawnPosition position) 
    {
        spawnPosition = position;
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

