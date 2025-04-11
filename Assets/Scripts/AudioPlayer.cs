using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioPlayer : Singleton<AudioPlayer>
{
    [SerializeField] AudioClip[] musicSounds, sfxSounds;
    [SerializeField] AudioSource musicSource, sfxSource;

    //AudioManager audioManager;

    /*Scene managerScene = SceneManager.GetSceneByName("Manager");
        audioManager = ;*/
    private void Start()
    {
        //audioManager = FindAnyObjectByType<AudioManager>();

        string sceneName = SceneManager.GetActiveScene().name;
        switch(sceneName)
        {
            case "MenuScene":
                MusicPlayer("MenuMusic_Sound");
                break;
        }
    }

    public void MusicPlayer(string musicName)
    {
        AudioClip musicClipToPlay = Array.Find(musicSounds, x => x.name == musicName);

        if (musicClipToPlay != null)
        {
            musicSource.clip = musicClipToPlay;
            Debug.Log(musicSource.clip);
            musicSource.Play();
        }
        else
        {
            Debug.Log("Music sound not found");
        }
    }

    public void SfxPlayer(string sfxName)
    {
        AudioClip sfxClipToPlay = Array.Find(sfxSounds, x => x.name == sfxName);

        if (sfxClipToPlay != null)
        {
            sfxSource.clip = sfxClipToPlay;
            Debug.Log(sfxSource.clip);
            sfxSource.Play();
        }
        else
        {
            Debug.Log("Sfx sound not found");
        }
    }
}
