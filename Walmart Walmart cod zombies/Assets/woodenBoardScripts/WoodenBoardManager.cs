using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//manages the boards of the map using a heap
public class WoodenBoardManager : MonoBehaviour
{

    public List<GameObject> G0Board;
    public List<GameObject> G1Board;
    public List<GameObject> G2Board;

    //at the start fill the heap with boarded window
    //if a zombie targets a window remove it from queue
    //randomly select it from the list and queue into a untargetable queue
    //queue size is 4
    //+1 for attack zombie
    public static WoodenBoardManager instance;
    LayerMask mask = 1 << 9 | 1 << 10 | 1 << 11 | 1 << 12;

    //it looks like theres zones where the zombie spawn and randomly chooses the board from?
    public List<woodenBoardHp> notFullqueued = new List<woodenBoardHp>();
    public List<woodenBoardHp> fullQueued = new List<woodenBoardHp>();
    //in code it doesn't say that there are 4 different code but in the game zombies seems to have perfernces
    //like the zombies that spawn in the back would only target the back boards etc
    public List<woodenBoardHp> leftNotFullqueued = new List<woodenBoardHp>();
    public List<woodenBoardHp> leftFullQueued = new List<woodenBoardHp>();

    public List<woodenBoardHp> rightNotFullqueued = new List<woodenBoardHp>();
    public List<woodenBoardHp> rightFullQueued = new List<woodenBoardHp>();

    public List<woodenBoardHp> backNotFullqueued = new List<woodenBoardHp>();
    public List<woodenBoardHp> baclFullQueued = new List<woodenBoardHp>();


    private void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(this); }
        addGR0Room();
        foreach (GameObject boards in GameObject.FindGameObjectsWithTag("FrontBoards"))
        {
            //populate

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
    //adds room 0's board so the zombie can target them
    public void addGR0Room()
    {
        foreach (GameObject boards in G0Board) 
        {
            
            listToAddTo(boards,boards.layer);
        }
       
    }
  
    public void addGR1Room()
    {

    }
    public void addGR2Room()
    {

    }
    //populates the list based off the given board and its layermask
    void listToAddTo(GameObject board,int maskLayer) 
    {
      
        switch (maskLayer) 
        {
            case  9: notFullqueued.Add(board.GetComponent<woodenBoardHp>()); break;
            case 10: backNotFullqueued.Add(board.GetComponent<woodenBoardHp>()); break;
            case 11: rightNotFullqueued.Add(board.GetComponent<woodenBoardHp>()); break;
            case 12: leftNotFullqueued.Add(board.GetComponent<woodenBoardHp>()); break;
        }
    }
}
