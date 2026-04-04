using UnityEngine;

//an abstract composition class 
//ok so apprently abstract classes can be treated like inhertiance but every method could be accessed like an interface
public abstract class healthManager : MonoBehaviour
{
    [SerializeField] protected entityStatSO stats;
    protected float health;
    protected virtual void Start()
    {
        health = stats.hp;
    }
    //zombies,board,players could "inherit" this "interface" method to either keep functionailty or override
    public virtual void takeDamage(float damage) 
    {
        health -= damage;
        Debug.Log(gameObject.name + " :" + damage);
        if (health < 0 && stats.destroyOnDeath) 
        {
            Destroy(gameObject);
        }
        Debug.Log(gameObject.name + " :" + health);
    }
}
