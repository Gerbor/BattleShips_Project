using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    AudioSource aSource;
    public AudioClip[] aClips;

    private void Start()
    {
        aSource = GetComponent<AudioSource>();
    }
    public void PlaySound(int i)
    {
        aSource.clip = aClips[i];
        aSource.Play();
    }
}
