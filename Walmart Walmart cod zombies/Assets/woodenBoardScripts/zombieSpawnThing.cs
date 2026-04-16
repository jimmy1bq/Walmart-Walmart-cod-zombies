using UnityEngine;

public class zombieSpawnThing : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject zombies;
    public int number;
    int i = 0;
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
        if (i < number)
        {
            Instantiate(zombies);
            i++;
        }
    }
}
