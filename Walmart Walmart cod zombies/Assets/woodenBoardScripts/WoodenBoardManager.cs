using UnityEngine;
//manages the boards of the map using a heap
public class WoodenBoardManager : MonoBehaviour
{
    public static WoodenBoardManager instance;
    //stores the boards that haven't been killed
    public Heap<woodenBoardHp> notDeadBoards = new Heap<woodenBoardHp>(30);

    public Heap<woodenBoardHp> deadBoards = new Heap<woodenBoardHp>(30);


    private void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(this); }
        foreach (GameObject boards in GameObject.FindGameObjectsWithTag("PotentialBoard"))
        {
            notDeadBoards.add(boards.GetComponent<woodenBoardHp>());
        }
    }
    private void Update()
    {
        Debug.Log(notDeadBoards.Count);
    }
}
