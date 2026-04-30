using System;
using UnityEngine;
using UnityEngine.Rendering;

public interface IDamageAble 
{
    public float takeDamage(float damage,int damageType);
    public float returnHP();
}
public interface IInteractable
{
    public bool zombieInteract();

}
public interface IHealAble 
{
    public float action(float heal);

}
public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex { get; set; }
}
public interface IQueue 
{
    public void updateQueuePoistion(GameObject positionToMoveTo);
}
