using UnityEngine;
//ok so in the og game the zombies target the wooden boards then the player and if theres already a zombie at the spot it stays there
//so it priotize the strongest board 
//we are going to implment this with the perfect data struct: max heap(priority queue)
public class Heap<T> where T : IHeapItem<T>
{
    //speaking of Heaps this data structure is acutally super good for ECS
    T[] items;
    int currentItemCount;
    //constructor for heap data struct
    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }
    //adds a item to the list
    public void add(T item)
    {
        //add the item and update the count(not size)
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        bubbleUp(item);
        currentItemCount++;
    }
    //remove root
    public T RemoveFirst()
    {
        if (currentItemCount > 0)
        {
            T firstItem = items[0];
            currentItemCount--;
            items[0] = items[currentItemCount];
            items[0].HeapIndex = 0;
            SortDown(items[0]);
            return firstItem;
        }
        //return null if empty
        return default(T);
    }
    //bubble up the item 
    //the formula index-1/2 is the parent index formula
    public void bubbleUp(T itemToBubble)
    {
        int parentIndex = (itemToBubble.HeapIndex - 1) / 2;
        while (true)
        {
            //get parent item
            T parentItem = items[parentIndex];
            //i think if item is less than parentItem then we return -1
            if (itemToBubble.CompareTo(parentItem) > 0)
            {
                swap(itemToBubble, parentItem);
            }
            else { break; }
            parentIndex = (itemToBubble.HeapIndex - 1) / 2;
        }
    }
    public void swap(T item1, T item2)
    {
        //change item at index
        items[item1.HeapIndex] = item2;
        items[item2.HeapIndex] = item1;
        //save index 
        int item1Index = item1.HeapIndex;
        //swap index
        item1.HeapIndex = item2.HeapIndex;
        item2.HeapIndex = item1Index;


    }
    void SortDown(T item)
    {
        while (true)
        {
            //get child index
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            //check if child is in bound then find the less valued child
            if (childIndexLeft < currentItemCount)
            {
                swapIndex = childIndexLeft;

                if (childIndexRight < currentItemCount)
                {
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }
                //compare the item and swap if needed
                if (item.CompareTo(items[swapIndex]) < 0)
                {
                    swap(item, items[swapIndex]);
                }
                else
                {
                    return;
                }

            }
            else
            {
                return;
            }

        }
    }



    //returns true if the item exist otherwise false
    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }
    //returns size
    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }
    public void UpdateItem(T item)
    {
        bubbleUp(item);
    }

}
