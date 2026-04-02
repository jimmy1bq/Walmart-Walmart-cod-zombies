using UnityEngine;
using UnityEngine.AI;

public class ZombieAi : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;
    GameObject player;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        agent.destination = player.transform.position;
        agent.autoTraverseOffMeshLink = false;
        
        TickSystem.frequenttickTime.AddListener(updateLogic);
    }

    // Update is called once per frame
   
    void updateLogic(float time) 
    {
       
        agent.destination = player.transform.position;
        if (agent.isOnOffMeshLink)
        {
           
        }
    }

}
