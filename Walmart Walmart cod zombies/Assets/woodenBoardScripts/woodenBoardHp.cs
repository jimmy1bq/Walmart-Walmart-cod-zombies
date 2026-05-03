using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;



public class woodenBoardHp : MonoBehaviour, IDamageAble, IHealAble,IInteractable
{
    [SerializeField] entityStatSO stats;
    //first position is attacking position
    //rest are climbing positions
    //start at 5 size and remove one by one
    //macdonalds simulator outhere
    List<GameObject> queuePosition = new List<GameObject>();
    List<GameObject> zombieQueue = new List<GameObject>();
    AudioSource woodenBoardSrc; 
    int index = 1;
    float health;
    public bool zombieAttacking = false;    
    public bool dead = false;

    //get set
    public int HeapIndex { get; set; }
    private void Awake()
    {
        woodenBoardSrc = GetComponent<AudioSource>();
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
    public float takeDamage(float damageToTake,int damageType) 
    {
        if (health > 0) 
        {
            health -= damageToTake;

            //get the board's animation 1-6(based on what board is left and index)

            GameObject boardToRemove = gameObject.transform.parent.Find("board" + index).gameObject;
            Animation animateComp = obtainAnimationComp(boardToRemove);
            audioManagerZombies.instance.playWoodenBoard(woodenBoardSrc, transform.TransformPoint(gameObject.transform.position), 1, 1);
            animateComp.Play("BoardAnimation" + index);
            index++;

            //this is just a safety set
            if (health <= 0) { health = 0; dead = true; index = 6; }

        }
        return health;    
    }

    //repair board
    public float action(float healHp)
    {
        if (health < stats.hp)
        {
            health += healHp;
            dead = false;
            index = Mathf.Clamp(index - 1, 1, 5);
            playRepairAnim(index);
            PointsManager.Instance?.AddPoints(10);
            if (health >= stats.hp) { health = stats.hp; index = 1; }
        }
        return health;
    }

    void playRepairAnim(int boardIndex)
    {
        GameObject boardObj = gameObject.transform.parent.Find("board" + boardIndex).gameObject;
        audioManagerZombies.instance.playWoodenBoard(woodenBoardSrc, transform.TransformPoint(gameObject.transform.position), 1, 0);
        obtainAnimationComp(boardObj).Play("repairBoard" + boardIndex + "Anim");
    }

    // Repairs all broken boards at once — used by the room wall-buy. No points awarded.
    public void repairAll()
    {
        while (index > 1)
        {
            index--;
            playRepairAnim(index);
        }
        health = stats.hp;
        dead = false;
        index = 1;
    }

    Animation obtainAnimationComp(GameObject gameObjek) 
    {
        return gameObjek.GetComponent<Animation>();
    }

    //interface to return the hp upon getting called
    public float returnHP()
    {
        return health;
    }

    //adds a zombie onto the Queue and if its greater than size we move this list onto the full queue
    public GameObject addZombieOntoQueue(GameObject zombie)
    {     
        if (zombieQueue.Count < 5)
        {
           
            //adds a zombie onto the queue
            //return a queue position otherwise return null
            zombieQueue.Add(zombie);
            if (zombieQueue.Count >= 5)
            {
                
                //tells the manager to get it out of queue
                WoodenBoardManager.instance.switchQueueToFull(this,gameObject.layer);
            }
          
            return queuePosition[zombieQueue.Count - 1];
        }
        return null;
    }

    //moves the zombies up the Queue once the first zombie finish climbing the window
    public GameObject moveQueueUp()
    {
        if (zombieQueue.Count - 1 > 0)
        {
            //switch to not full
            WoodenBoardManager.instance.switchQueueToNotFull(this, gameObject.layer);
            zombieQueue.Remove(zombieQueue[0]);
            /* foreach (GameObject zombies in zombieQueue)
             {
                 zombies.GetComponent<IQueue>().updateQueuePoistion(queuePosition[i]);
                 i++;
             }*/
            zombieQueue[0].GetComponent<IQueue>().updateQueuePoistion(queuePosition[0]);
        }
        return null;
    }

    // Called when a zombie that was queued at this window dies so it doesn't permanently occupy a slot.
    public void onZombieDied(GameObject zombie)
    {
        int idx = zombieQueue.IndexOf(zombie);
        if (idx < 0) return;
        bool wasFull = zombieQueue.Count >= 5;
        zombieQueue.Remove(zombie);
        // If the front-of-queue (attacker) died, advance the next zombie to the attack position.
        if (idx == 0 && zombieQueue.Count > 0)
            zombieQueue[0].GetComponent<IQueue>()?.updateQueuePoistion(queuePosition[0]);
        if (wasFull && zombieQueue.Count < 5)
            WoodenBoardManager.instance.switchQueueToNotFull(this, gameObject.layer);
    }
    public void checkDeath(GameObject zombie) 
    {
        if (zombieQueue.IndexOf(zombie) == 0) { zombieAttacking = false; }
    }

    public bool zombieInteract()
    {
        return zombieAttacking;
    }
}
