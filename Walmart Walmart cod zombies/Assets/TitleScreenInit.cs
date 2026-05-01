using UnityEngine;
using System.Collections.Generic;

public class TitleScreenInit : MonoBehaviour
{
    [SerializeField] List<GameObject> objectsToInsta;
    public static TitleScreenInit instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            gameObject.SetActive(true);
            DontDestroyOnLoad(gameObject);
            foreach (GameObject objects in objectsToInsta)
            {
                Instantiate(objects);
            }
        }
        else
        {
            Destroy(gameObject);
        }       
    }
}
