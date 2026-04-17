using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;



public class woodenBoardHp : MonoBehaviour, IDamageAble, IHealAble
{
    [SerializeField] entityStatSO stats;
    //first position is attacking position
    //rest are climbing positions
    //start at 5 size and remove one by one
    //macdonalds simulator outhere
    List<GameObject> queuePosition = new List<GameObject>();
    List<GameObject> zombieQueue = new List<GameObject>();
    int index;
    float health;
    public bool dead = false;

    //get set
    public int HeapIndex { get; set; }
    private void Awake()
    {
        //populate list
        Transform parentTransform = gameObject.transform.parent.transform;
        queuePosition.Add(parentTransform.Find("p1").gameObject);
        queuePosition.Add(parentTransform.Find("i2").gameObject);
        queuePosition.Add(parentTransform.Find("i3").gameObject);
        queuePosition.Add(parentTransform.Find("i4").gameObject);
        queuePosition.Add(parentTransform.Find("i5").gameObject);
    }

    private void Start()
    {
        health = stats.hp;   
    }

    //heals
    public float heal(float healing) 
    {
        health+=healing;
        if (health > 0) 
        {
            dead = false;
        }
        return health;
    }

    //param  damageToTake: float to subtract off hp
    //return the health left after taking damage
    public float takeDamage(float damageToTake) 
    {
        health -= damageToTake;
        if (health < 0) { health = 0; dead = true; }   
        return health;    
    }

    //repair board
    public float action(float healHp) 
    {
        health += healHp;
        if (health > 0) { health = 100; }
        return health;
    }

    //interface to return the hp upon getting called
    public float returnHP()
    {
        return health;
    }

    //adds a zombie onto the Queue and if its greater than size we move this list onto the full queue
    public GameObject addZombieOntoQueue(GameObject zombie) 
    {
        
        if (zombieQueue.Count<=5)
        {
           
            //adds a zombie onto the queue
            //return a queue position otherwise return null
            zombieQueue.Add(zombie);
            if (zombieQueue.Count >= 5)
            {
                
                //tells the manager to get it out of queue
                WoodenBoardManager.instance.switchQueueToFull(this);
            }
          
            return queuePosition[zombieQueue.Count - 1];
        }
        return null;
    }

    //moves the zombies up the Queue once the first zombie finish climbing the window
    public GameObject moveQueueUp() 
    {
        if (zombieQueue.Count > 0) 
        {
            zombieQueue.Remove(zombieQueue[0]);
            int i = 0;
            foreach (GameObject zombies in zombieQueue) 
            {
                zombies.GetComponent<IQueue>().updateQueuePoistion(queuePosition[i]);
                i++;
            }
        }
        return null;
    }
}
