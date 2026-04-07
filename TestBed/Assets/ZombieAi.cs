using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
//navmesh info:
//the agent 
public class ZombieAi : MonoBehaviour, IDamageAble
{
    [SerializeField] entityStatSO stats;
    NavMeshAgent agent;
   
    //Animation variables
    //maybe for the future an animation manager? it would produce cleaner code
    Animation animationer;
    AnimationState[] animationStates = new AnimationState[8];
    
    GameObject player;
    Coroutine attackCoroutine;

    bool wasOnLink = false;

    float health;
    
    void Start()
    {
        health = stats.hp;
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        animationer = GetComponent<Animation>();
        int i = 0;
        //get them into an array so we can access for later
        foreach (AnimationState states in animationer) 
        {
             Debug.Log(states);
             animationStates[i] = states;
             i++;
        }
        //we only need the zombie to jump the window once(play the animation once
        animationer[animationStates[5].name].wrapMode = WrapMode.Once;
        agent.destination = player.transform.position;
        agent.autoTraverseOffMeshLink = false;    
        TickSystem.frequenttickTime.AddListener(trackPlayerPoistion);
    }


    //tracks the player position; should be called on every frame;
    void trackPlayerPoistion(float time)
    {
        agent.destination = player.transform.position;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        //this should only happen when a board enters the zombies range AND only once so no need to check if theres an coroutine happening

        if (other.gameObject.CompareTag("PotentialBoard") && other.gameObject.GetComponent<IDamageAble>() != null && other.gameObject.GetComponent<IDamageAble>().returnHP() > 0)
        {
           attackCoroutine = StartCoroutine(attackboard(other.gameObject));
        }else if (other.gameObject.CompareTag("Player")) 
        {
            animationer.Stop();
            animationer.Play(animationStates[0].name);
            agent.isStopped = true;
            attackCoroutine = StartCoroutine(attackPlayer(other.gameObject));
        }
        
    }
    
    IEnumerator attackPlayer(GameObject player) 
    {
        
        if (agent.remainingDistance < 1.3f)
        {
            agent.isStopped = true;
            //cancels the current animation and switches to smaking right away;
            animationer.Play(animationStates[0].name);
            //player Damage Logic
            //use attack(player,0);
        } 
        yield return new WaitForSeconds(1);
        //if the player get out of range this doesn't happen
        if (agent.remainingDistance < 1.3f) { StartCoroutine(attackPlayer(player)); }
        else { attackCoroutine = null; animationer.Play(animationStates[4].name); agent.isStopped = false; }
    
    }
    //attacks the board on the window
    //param board: the board gameObject to attack
    IEnumerator attackboard(GameObject board)
    {
        float hpLeft = attack(board,1);
        Debug.Log(hpLeft);
        if (hpLeft <= 0)
        {
            //climb
            attackCoroutine = null;
            StartCoroutine(waitUntilAnimFinishPlaying(animationStates[5], 0));
        }
        else
        {
            //attack again
            yield return new WaitForSeconds(1);
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
      
        //wait for animation to finish playing
        yield return new WaitForSeconds(animation.length);
        
        Debug.Log("passed");
        switch (actionAfterWards)
        {
            case 0:  OffMeshLinkData linkData = agent.currentOffMeshLinkData; agent.CompleteOffMeshLink(); animationer.Play(animationStates[4].name); break;
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
