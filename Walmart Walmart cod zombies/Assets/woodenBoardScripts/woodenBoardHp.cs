using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class woodenBoardHp : MonoBehaviour, IDamageAble
{
    [SerializeField] entityStatSO stats;
    float health;
    private void Start()
    {
        health = stats.hp;
    }
    //param  damageToTake: float to subtract off hp
    //return the health left after taking damage
    public float takeDamage(float damageToTake) 
    {
        
        health -= damageToTake;
        if (health < 0) { health = 0; }

        return health;    
    }
    //interface to return the hp upon getting called
    public float returnHP()
    {
        return health;
    }
}
