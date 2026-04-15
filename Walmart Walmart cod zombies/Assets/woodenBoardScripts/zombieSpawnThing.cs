using UnityEngine;

public class zombieSpawnThing : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject zombies;
    void Start()
    {
        TickSystem.tickEvent.AddListener(spawnZombies);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void spawnZombies(float time)
    {
    Instantiate(zombies);
    }
}
