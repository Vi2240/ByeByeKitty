using UnityEngine;
using NUnit.Framework.Internal;
using System.Collections;
using System;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] AudioClip[] musicSounds, sfxSounds;

    public AudioClip GetMusicClip(string name)
    {
        AudioClip musicClip = Array.Find(musicSounds, x => x.name == name);
        return musicClip;
    }

    public AudioClip GetSfxClip(string name)
    {
        AudioClip sfxClip = Array.Find(sfxSounds, x => x.name == name);
        return sfxClip;
    }
}
