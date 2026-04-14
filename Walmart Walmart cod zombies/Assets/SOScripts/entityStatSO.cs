using UnityEditor.ShaderGraph.Internal;
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
}
