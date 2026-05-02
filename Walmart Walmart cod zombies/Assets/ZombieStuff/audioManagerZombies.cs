using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public class audioManagerZombies : MonoBehaviour
{
    //audio clips
    //put all audio clip here please
    //Should be Dont Destroy on Load
    //missing gun and heatbeat do later
    public static audioManagerZombies instance;
    public List<AudioClip> zombieAttackClips;
    public List<AudioClip> zombieGroanClips;
    public List<AudioClip> backgroundMusic;
    public List<AudioClip> gunFireSounds;
    public AudioClip PlayerAddBoard;
    public AudioClip zombieRemoveBoard;
    public AudioClip zombieFootStep;
    public AudioClip playerFootSteps;
    public float sfxVolume = 100;
    public float musicVolume = 100;
    public float masterVolume = 100;
    public float mouseSensitivity = 1f;
    int loop = 0;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); } else { Destroy(this); }
        playBackgroundMusic(GetComponent<AudioSource>(),gameObject.transform.position,0);
        TickSystem.tickEvent.AddListener(loopMusic);
    }
    //loops the background music 
    void loopMusic(float time) 
    {
        time = loop * 1060 + time;
        float case1 = loop * 1060;
        float case2 = 276 + loop * 1060;
        float case3 = 539 + loop * 1060;
        float case4 = 833 + loop * 1060;
        if (time == case1)
        {
            Debug.Log("looped");
            playBackgroundMusic(GetComponent<AudioSource>(), gameObject.transform.position, 0);
            loop++;
        }
        else if (time == case2)
        {
            Debug.Log("loopCase2");
            playBackgroundMusic(GetComponent<AudioSource>(), gameObject.transform.position, 1);
        }
        else if (time == case3)
        {
            Debug.Log("loopCase3");
            playBackgroundMusic(GetComponent<AudioSource>(), gameObject.transform.position, 2);
        }
        else if (time == case4) 
        {
            playBackgroundMusic(GetComponent<AudioSource>(), gameObject.transform.position, 3);
        }
            
         
        
    }
    //plays a random sound given the source position  volume and list
    public void playRandomZombieSound(AudioSource zombSource, Vector3 position, float volume, List<AudioClip> list, float speed)
    {

        //speed up the pitch
        zombSource.pitch = speed;
        zombSource.volume = sfxVolume;
        zombSource.clip = list[Random.Range(0, list.Count - 1)];
        zombSource.Play();
    }

    public void playZombieWalkingSound(AudioSource zombSource, Vector3 position, float volume)
    {

        //so basically the only other clips are groaning if we are playing walking sound because attacking turn isWalkingoff
        //do not do !zombSource.clip because that check nullity of zombSource
        if (!zombSource.isPlaying)
        {
            //slow down the sound 
            //    zombSource.volume = sfxVolume;
            zombSource.pitch = 0.5f;
            zombSource.clip = zombieFootStep;
            zombSource.volume = sfxVolume;
            zombSource.Play();
        }
    }

    public void playWoodenBoard(AudioSource zombSource, Vector3 position, float pitch,int removeOrAdd) 
    {
        zombSource.pitch = pitch;
        zombSource.volume = sfxVolume;
        switch (removeOrAdd) 
        {
            case 0: zombSource.clip = PlayerAddBoard; break;
            case 1: zombSource.clip = zombieRemoveBoard; break;
        }
        zombSource.Play();
    }

    public void playBackgroundMusic(AudioSource playerSource,Vector3 position,int number) 
    {
        switch (number)
        {
            //I think after 1060s it cycles back
            case 0: playerSource.clip = backgroundMusic[0]; break;
            case 1: playerSource.clip = backgroundMusic[1]; break;
            case 2: playerSource.clip = backgroundMusic[2]; break;
            case 3: playerSource.clip = backgroundMusic[3]; break;
        }
        //playervolume here by doing playersource.volume
        playerSource.volume = musicVolume;
        playerSource.Play();
    }
    //plays the footstep for the player if its not alreadying playing
    public void playPlayerFootStep(AudioSource playerSoruce,Vector3 playerPosition) 
    {
        if (!playerSoruce.isPlaying) 
        {
            //pitch is the speed 
            playerSoruce.pitch = 0.5f;
            playerSoruce.clip = playerFootSteps;
            playerSoruce.volume = sfxVolume;
            playerSoruce.Play();
        }
    }
   
    //changes the value so later on UI manager can call
    //so like if the sfx vol gets changed then this should also change
    public void changeSfxVolume(float val) 
    {
        
       sfxVolume = val * 100;
    }
    public void changeMasterVolume(float val)
    {
        
        masterVolume = val * 100;
    }
    public void changeMusicVolume(float val)
    {
        
        musicVolume = val * 100;
    }
    public void changeMouseSensitivity(float val)
    {
      
        mouseSensitivity = val * 100;
    }


}


