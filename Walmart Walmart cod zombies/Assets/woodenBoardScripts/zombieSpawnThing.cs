using UnityEngine;

public class zombieSpawnThing : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject zombies;
    public int number;
    int i = 0;
    float tick = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        spawnZombies(2);
    }
    void spawnZombies(float time)
    {
        tick+=Time.deltaTime;
        if (i < number && tick>= 0.1f)
        {
            Instantiate(zombies);
            i++;
        }
    }
}
