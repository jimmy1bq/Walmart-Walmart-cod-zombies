using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;


public class woodenBoardHp : MonoBehaviour, IDamageAble, IHealAble, IHeapItem<woodenBoardHp>

{
    [SerializeField] entityStatSO stats;
    int index;
    float health;
    public bool dead = false;

    //get set
    public int HeapIndex { get; set; }
    private void Start()
    {
        health = stats.hp;
    }
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
    //interface to return the hp upon getting called
    public float returnHP()
    {
        return health;
    }

    public int CompareTo(woodenBoardHp other)
    {
        if (other.health < health) 
        {
            return 1;
        }
        if (other.health > health)
        {
            return -1;
        }
        return 0;
    }
}
