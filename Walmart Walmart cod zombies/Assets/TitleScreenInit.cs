using UnityEngine;
using System.Collections.Generic;

public class TitleScreenInit : MonoBehaviour
{
    [SerializeField] List<GameObject> objectsToInsta;
    private void Awake()
    {
        foreach(GameObject objects in objectsToInsta) 
        {
            Instantiate(objects);
        }
    }
}
