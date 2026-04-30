using UnityEngine;

public class DONTDESTROYONLOAD : MonoBehaviour
{
    private static DONTDESTROYONLOAD instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
