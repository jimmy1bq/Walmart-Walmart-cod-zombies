using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public class audioManagerZombies : MonoBehaviour
{
    public static audioManagerZombies instance;
    public List<AudioClip> zombieAttackClips;
    public List<AudioClip> zombieGroanClips;
    public AudioClip zombieFootStep;
  

    private void Awake()
    {
         if (instance == null) { instance = this; } else { Destroy(this); }
    }

    //plays a random sound given the source position  volume and list
    public void playRandomZombieSound(AudioSource zombSource, Vector3 position, float volume, List<AudioClip> list)
    {
        //speed up the pitch
        zombSource.pitch = 1f;
        zombSource.volume = volume;
        zombSource.clip = list[Random.Range(0, list.Count - 1)];
        zombSource.Play();
    }

    public void playZombieWalkingSound(AudioSource zombSource, Vector3 position, float volume) 
    {
        if (!zombSource.isPlaying) 
        {
            //slow down the sound 
            zombSource.pitch = 0.5f; 
            zombSource.clip = zombieFootStep;
            zombSource.Play();
        }
    }
}
    
    
