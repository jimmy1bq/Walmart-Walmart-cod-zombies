using UnityEngine;

public class zombieSpawnThing : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject zombies;
    public int number;
    int number2 = 5;
    int j = 0;
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
        if (i < number)
        {
            Instantiate(zombies);
            i++;
        }
        if (j < number2 && tick >= 30f) 
        {
            Instantiate(zombies);
            j++;
        }
    }
}
