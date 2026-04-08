using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Intialzer : MonoBehaviour
{
    [SerializeField] List<GameObject> systemToInstantiate;

    private void Awake()
    {
        foreach (GameObject systems in systemToInstantiate) 
        {
            Instantiate(systems);
        }
    }

}
