using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//manages the boards of the map using a heap
public class WoodenBoardManager : MonoBehaviour
{
    //at the start fill the heap with boarded window
    //if a zombie targets a window remove it from queue
    //randomly select it from the list and queue into a untargetable queue
    //queue size is 4
    //+1 for attack zombie
    public static WoodenBoardManager instance;
    //stores the boards that haven't been killed
    public List<woodenBoardHp> notFullqueued = new List<woodenBoardHp>();

    public List<woodenBoardHp> fullQueued = new List<woodenBoardHp>();


    private void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(this); }
        foreach (GameObject boards in GameObject.FindGameObjectsWithTag("PotentialBoard"))
        {
            notFullqueued.Add(boards.GetComponent<woodenBoardHp>());
        }
    }
    //removes from q1 and puts it into q2
    public void switchQueueToFull(woodenBoardHp objeck) 
    {
        if (notFullqueued.Contains(objeck)) 
        {
        notFullqueued.Remove(objeck);
        fullQueued.Add(objeck);
        }
    }
    //q2 to q1
    public void switchQueueToNotFull(woodenBoardHp objeck)
    {
        if (fullQueued.Contains(objeck))
        {
            fullQueued.Remove(objeck);
            notFullqueued.Add(objeck);
        }
    }
    //returns a randomBoardOn the Mapadd
    public woodenBoardHp randomQueue() 
    {
        return notFullqueued[(int)UnityEngine.Random.Range(0, WoodenBoardManager.instance.notFullqueued.Count)];
    }
}
