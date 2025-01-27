﻿using System.Linq;
using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    private static SoundHandler _instance;

    public static SoundHandler GetInstance() => _instance;

    public SoundAudioClip[] SoundAudioClips;

    private void Start()
    {
        _instance = this;
    }

    public void PlaySound(Sounds sound)
    {
        AudioSource audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.PlayOneShot(GetAudioClip(sound));
    }

    private AudioClip GetAudioClip(Sounds sound)
        => SoundAudioClips.FirstOrDefault(audio => audio.Sound == sound)?.AudioClip;
}
