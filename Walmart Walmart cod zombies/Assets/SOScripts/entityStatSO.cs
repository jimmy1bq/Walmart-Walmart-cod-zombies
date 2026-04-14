using UnityEngine;

//stores stats for player, zombies and woodenboards since they all share some data

[CreateAssetMenu(fileName = "entityStatsSO", menuName = "ScriptableObjects/Stats")]
public class entityStatSO : ScriptableObject
{
    public float hp;
    //melee damage
    public float meleeDamage;
    public float walkSpeed;
    public bool destroyOnDeath;
    public Animation animationToPlayOnDamageTaken;
    // points awarded to the player when this entity is killed
    public int killPoints = 50;
}
