using UnityEngine;
using UnityEngine.Events;

public class TickSystem : MonoBehaviour
{  
    [SerializeField] GameObject gameManager;
    [SerializeField] GameObject UIManager;
    //normally you would implment a task scheduler with this since events are put in a random order;this allow determinstic order
    //tick system allows you to remove update logic without needing to call and check an if statement inside update
    //helps a lot since update is called every frame
    public static TickSystem instance;
    //every half of a second
    public static UnityEvent<float> tickEvent = new UnityEvent<float>();
    //every frame
    public static UnityEvent<float> frequenttickTime = new UnityEvent<float>();
    float tickRate = 0.1f;
    float tickTimer = 0f;
    float totalTIme = 0f;

    private float tick = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }


    // Update is called once per frame
    void Update()
    {
        tickTimer += Time.deltaTime;
        totalTIme += Time.deltaTime;
        if (tickTimer >= tickRate)
        {
            tickTimer -= tickRate;
            tick++;
            tickEvent.Invoke(tick);
        }
        frequenttickTime.Invoke(totalTIme);
    }

}
