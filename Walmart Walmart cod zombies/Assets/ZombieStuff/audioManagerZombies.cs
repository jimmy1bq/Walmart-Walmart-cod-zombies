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
    public float sfxVolume = 100;
    public float musicVolume = 100;
    public float masterVolume = 100;

    
    private void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(this); }
    }

    //plays a random sound given the source position  volume and list
    public void playRandomZombieSound(AudioSource zombSource, Vector3 position, float volume, List<AudioClip> list,float speed)
    {
        
        //speed up the pitch
        zombSource.pitch = speed;
        zombSource.volume = sfxVolume;
        Debug.Log(list.Count);
        zombSource.clip = list[Random.Range(0, list.Count - 1)];
        zombSource.Play();
    }

    public void playZombieWalkingSound(AudioSource zombSource, Vector3 position, float volume)
    {
        Debug.Log("attempt");
        //so basically the only other clips are groaning if we are playing walking sound because attacking turn isWalkingoff
        if (!zombSource.isPlaying)
        { 
            //slow down the sound 
        //    zombSource.volume = sfxVolume;
            zombSource.pitch = 0.5f;
            zombSource.clip = zombieFootStep;
            zombSource.Play();
        }
    }
    //changes the value so later on UI manager can call
    public void changeSfxVolume(float val) 
    {
    
    }
    public void changeMasterVolume(float val)
    {

    }
    public void changeMusicVolume(float val)
    {

    }

}
    
    
