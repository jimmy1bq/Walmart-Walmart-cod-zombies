using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine.Events;
//manages the boards of the map using a heap
public class WoodenBoardManager : MonoBehaviour
{
    //boards to unlock for zombie when buying a place
    public List<GameObject> G0Board;
    public List<GameObject> G1Board;
    public List<GameObject> G2Board;

    public List<GameObject> F1Board;
    public List<GameObject> F2Board;
    public List<GameObject> F3Board;

    //at the start fill the heap with boarded window
    //if a zombie targets a window remove it from queue
    //randomly select it from the list and queue into a untargetable queue
    //queue size is 4
    //+1 for attack zombie
    public static WoodenBoardManager instance;
    public static UnityEvent<RoomId> onRoomUnlocked = new UnityEvent<RoomId>();
    private HashSet<RoomId> _unlockedRooms = new HashSet<RoomId>();

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
        UnlockRoom(RoomId.G0);
    }

    public bool IsRoomUnlocked(RoomId room) => _unlockedRooms.Contains(room);

    public void UnlockRoom(RoomId room)
    {
        if (_unlockedRooms.Contains(room)) return;
        _unlockedRooms.Add(room);
        switch (room)
        {
            case RoomId.G0: addGR0Room(); break;
            case RoomId.G1: addGR1Room(); break;
            case RoomId.G2: addGR2Room(); break;
            case RoomId.F1: addF1Room();  break;
            case RoomId.F2: addF2oom();   break;
            case RoomId.F3: addF3Room();  break;
        }
        onRoomUnlocked.Invoke(room);
    }
    //removes from q1 and puts it into q2 or removes from the not full list and moves it into the full list
    public void switchQueueToFull(woodenBoardHp objeck,int maskLayer)
    {
        List<woodenBoardHp> notFull = listToReturnOffLayer(maskLayer);
        List<woodenBoardHp> full = listToReturnOffLayerFULL(maskLayer);
        if (notFull.Contains(objeck))
        {
            notFull.Remove(objeck);
            full.Add(objeck);
        }
       
    }
    //q2 to q1 vice versa to the method above
    public void switchQueueToNotFull(woodenBoardHp objeck,int maskLayer)
    {
        List<woodenBoardHp> notFull = listToReturnOffLayer(maskLayer);
        List<woodenBoardHp> full = listToReturnOffLayerFULL(maskLayer);
        if (full.Contains(objeck))
        {
            full.Remove(objeck);
            notFull.Add(objeck);
        }
    }
    void Update()
    {
        Debug.Log(backNotFullqueued.Count);
    }
    //returns a randomBoardOn the Mapadd
    public woodenBoardHp randomQueue(ZombieSpawnPosition position)
    {
        List<woodenBoardHp> list;
        switch (position)
        {
            case ZombieSpawnPosition.Front:  list = notFullqueued;      break;
            case ZombieSpawnPosition.Left:   list = leftNotFullqueued;  break;
            case ZombieSpawnPosition.Back:   list = backNotFullqueued;  break;
            case ZombieSpawnPosition.Right:  list = rightNotFullqueued; break;
            default: return null;
        }
        if (list == null || list.Count == 0) return null;
        return list[UnityEngine.Random.Range(0, list.Count)];
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
        foreach (GameObject boards in G1Board)
        {
            listToAddTo(boards, boards.layer);
        }
    }
    public void addGR2Room()
    {
        foreach (GameObject boards in G2Board)
        {
            listToAddTo(boards, boards.layer);
        }
    }
    public void addF1Room()
    {
        foreach (GameObject boards in F1Board)
        {
            listToAddTo(boards, boards.layer);
        }
    }
    public void addF2oom()
    {
        foreach (GameObject boards in F2Board)
        {
            listToAddTo(boards, boards.layer);
        }
    }
    public void addF3Room()
    {
        foreach (GameObject boards in F3Board)
        {
            listToAddTo(boards, boards.layer);
        }
    }

    //populates the list based off the given board and its layermask
    void listToAddTo(GameObject board,int maskLayer) 
    {
      
        switch (maskLayer) 
        {
            case  9: notFullqueued.Add(board.GetComponent<woodenBoardHp>());   break;
            case 10: backNotFullqueued.Add(board.GetComponent<woodenBoardHp>()); break;
            case 11: rightNotFullqueued.Add(board.GetComponent<woodenBoardHp>()); break;
            case 12: leftNotFullqueued.Add(board.GetComponent<woodenBoardHp>()); break;
        }
    }
    //returns the list need to add/remove based of the object mask layer
    List<woodenBoardHp> listToReturnOffLayer(int maskLayer) 
    {
        switch (maskLayer)
        {
            case 9:  return notFullqueued;
            case 10: return backNotFullqueued;
            case 11: return rightNotFullqueued;
            case 12: return leftNotFullqueued;
            default: return null;
        }
    }
    List<woodenBoardHp> listToReturnOffLayerFULL(int maskLayer)
    {
        switch (maskLayer)
        {
            case 9: return fullQueued;
            case 10: return baclFullQueued;
            case 11: return rightFullQueued;
            case 12: return leftFullQueued;
            default: return null;
        }
    }
}
