using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class audioManagerZombies : MonoBehaviour
{
    public List<AudioClip> attackClips;
    AudioSource agentAudioSource;

    private void Awake()
    {
        agentAudioSource = GetComponent<AudioSource>();
    }

    //attacking player and board audio
    public void playRandomAttaclAudioClip() 
    {
      

    }
}
