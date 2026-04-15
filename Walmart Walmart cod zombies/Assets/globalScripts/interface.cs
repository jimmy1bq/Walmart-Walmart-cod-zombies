using System;
using UnityEngine;
using UnityEngine.Rendering;

public interface IDamageAble 
{
    public float takeDamage(float damage);
    public float returnHP();
}
public interface IInteractable
{
    public float action(float healing);

}
public interface IHealAble 
{
    public float action(float heal);

}
public interface IHeapItem<T> : IComparable<T>
{

    int HeapIndex { get; set; }
}
