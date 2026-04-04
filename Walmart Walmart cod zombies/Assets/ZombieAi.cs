using UnityEngine;
using UnityEngine.AI;
//navmesh info:
//the agent 
public class ZombieAi : MonoBehaviour
{
    [SerializeField] entityStatSO stats;
    NavMeshAgent agent;
    Animator animator;
    GameObject player;
    bool wasOnLink = false;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
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
        Debug.Log("trigger:" + other.name);
        if (other.gameObject.CompareTag("PotentialBoard") && other.gameObject.GetComponent<healthManager>()!=null) 
        {
            other.gameObject.GetComponent<healthManager>().takeDamage(stats.meleeDamage);
        }
    }


}
