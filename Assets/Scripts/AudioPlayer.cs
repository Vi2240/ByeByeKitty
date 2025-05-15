using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Current { get; private set; }

    [Header("Audio Clips")]
    [SerializeField] AudioClip[] musicClips;
    [SerializeField] AudioClip[] sfxClips;

    [Header("Audio Sources")]
    [Tooltip("AudioSource for background music. Should ideally be 2D.")]
    [SerializeField] AudioSource musicSource;
    [Tooltip("AudioSource for 2D UI SFX or global non-positional SFX. Should be 2D (Spatial Blend = 0).")]
    [SerializeField] AudioSource uiSfxSource;

    [Header("Looping SFX Settings")]
    [Tooltip("Default Min Distance for looping 3D SFX.")]
    [SerializeField] float loopingSfxMinDistance = 1f;
    [Tooltip("Default Max Distance for looping 3D SFX.")]
    [SerializeField] float loopingSfxMaxDistance = 50f;
    [Tooltip("Default Rolloff Mode for looping 3D SFX.")]
    [SerializeField] AudioRolloffMode loopingSfxRolloffMode = AudioRolloffMode.Logarithmic;

    private Dictionary<string, AudioClip> _musicLibrary;
    private Dictionary<string, AudioClip> _sfxLibrary;
    private Transform _loopingSfxContainer;

    private void Awake()
    {
        if (Current != null && Current != this)
        {
            Debug.LogWarning("Another AudioPlayer instance already exists. Destroying this duplicate: " + gameObject.name);
            Destroy(gameObject);
            return;
        }
        Current = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioLibraries();

        if (uiSfxSource != null) uiSfxSource.spatialBlend = 0f;
        if (musicSource != null) musicSource.spatialBlend = 0f;

        _loopingSfxContainer = new GameObject("LoopingSfxSources").transform;
        _loopingSfxContainer.SetParent(transform);
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }

    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name} (Mode: {mode}) - Checking for music.");
        // Play music based on the newly loaded scene's name
        switch (scene.name) // Use scene.name from the event argument
        {
            case "Manager":
                break;
            case "MenuScene":
                PlayMusic("MenuMusic");
                break;
            case "GamePlay":
                PlayMusic("Forest_Sound");
                break; ;
            default:
                StopMusic();
                Debug.Log($"No specific music configured for scene: {scene.name}");
                break;
        }
    }

    private void InitializeAudioLibraries()
    {
        _musicLibrary = new Dictionary<string, AudioClip>();
        foreach (AudioClip clip in musicClips)
        {
            if (clip == null) { Debug.LogWarning("AudioPlayer: Null music clip in array."); continue; }
            if (!_musicLibrary.ContainsKey(clip.name)) _musicLibrary.Add(clip.name, clip);
            else Debug.LogWarning($"AudioPlayer: Duplicate music clip name '{clip.name}'.");
        }

        _sfxLibrary = new Dictionary<string, AudioClip>();
        foreach (AudioClip clip in sfxClips)
        {
            if (clip == null) { Debug.LogWarning("AudioPlayer: Null SFX clip in array."); continue; }
            if (!_sfxLibrary.ContainsKey(clip.name)) _sfxLibrary.Add(clip.name, clip);
            else Debug.LogWarning($"AudioPlayer: Duplicate SFX clip name '{clip.name}'.");
        }
    }

    public void PlayMusic(string musicName, bool loop = true)
    {
        if (musicSource == null) { Debug.LogError("AudioPlayer: MusicSource not assigned!"); return; }
        if (_musicLibrary.TryGetValue(musicName, out AudioClip clipToPlay))
        {
            // Check if the same music is already playing and looping.
            // If you want to force a restart even if it's the same, remove this check.
            if (musicSource.clip == clipToPlay && musicSource.isPlaying && musicSource.loop == loop)
            {
                Debug.Log($"AudioPlayer: Music '{musicName}' is already playing with the same loop setting.");
                return;
            }
            musicSource.clip = clipToPlay;
            musicSource.loop = loop;
            musicSource.Play();
            Debug.Log($"AudioPlayer: Playing music '{musicName}'.");
        }
        else Debug.LogWarning($"AudioPlayer: Music clip '{musicName}' not found.");
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("AudioPlayer: Music stopped.");
        }
    }

    public void PlayUISfx(string sfxName, float volumeScale = 1.0f)
    {
        if (uiSfxSource == null) { Debug.LogError("AudioPlayer: UISfxSource not assigned!"); return; }
        if (_sfxLibrary.TryGetValue(sfxName, out AudioClip clipToPlay))
        {
            uiSfxSource.PlayOneShot(clipToPlay, volumeScale);
        }
        else Debug.LogWarning($"AudioPlayer: SFX clip '{sfxName}' not found for UI.");
    }

    public void PlaySfxAtPoint(string sfxName, Vector3 position, float volumeScale = 1.0f)
    {
        if (_sfxLibrary.TryGetValue(sfxName, out AudioClip clipToPlay))
        {
            AudioSource.PlayClipAtPoint(clipToPlay, position, GetSfxVolume() * volumeScale);
        }
        else Debug.LogWarning($"AudioPlayer: SFX clip '{sfxName}' not found for PlayAtPoint.");
    }

    public void PlaySfxOnTransform(string sfxName, Transform sourceTransform, float volumeScale = 1.0f)
    {
        if (sourceTransform == null) { Debug.LogError("AudioPlayer: sourceTransform is null."); return; }
        PlaySfxAtPoint(sfxName, sourceTransform.position, volumeScale);
        Transform a = sourceTransform;
    }

    public AudioSource PlayLoopingSfx(string sfxName, Vector3 position, float volumeScale = 1.0f, Transform parentTo = null, bool shouldLoop = true) // Added 'shouldLoop' parameter with a default
    {
        if (!_sfxLibrary.TryGetValue(sfxName, out AudioClip clipToPlay))
        {
            Debug.LogWarning($"AudioPlayer: Looping SFX clip '{sfxName}' not found.");
            return null;
        }

        GameObject sfxObject = new GameObject($"LoopingSFX_{sfxName}"); // Consider renaming if not always looping, e.g., "ManagedSFX_"
        sfxObject.transform.position = position;
        if (parentTo != null) sfxObject.transform.SetParent(parentTo);
        else sfxObject.transform.SetParent(_loopingSfxContainer);

        AudioSource audioSource = sfxObject.AddComponent<AudioSource>();
        audioSource.clip = clipToPlay;
        audioSource.volume = GetSfxVolume() * volumeScale;
        audioSource.loop = shouldLoop;
        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = loopingSfxMinDistance;
        audioSource.maxDistance = loopingSfxMaxDistance;
        audioSource.rolloffMode = loopingSfxRolloffMode;
        audioSource.Play();
        return audioSource;
    }

    public void StopLoopingSfx(AudioSource sourceToStop)
    {
        if (sourceToStop != null)
        {
            sourceToStop.Stop();
            if (sourceToStop.gameObject != null) Destroy(sourceToStop.gameObject);
        }
        else Debug.LogWarning("AudioPlayer: Attempted to stop a null AudioSource for looping SFX.");
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null) musicSource.volume = Mathf.Clamp01(volume);
    }
    public float GetMusicVolume() => musicSource != null ? musicSource.volume : 0f;

    public void SetSfxVolume(float volume)
    {
        if (uiSfxSource != null) uiSfxSource.volume = Mathf.Clamp01(volume);
    }
    public float GetSfxVolume() => uiSfxSource != null ? uiSfxSource.volume : 1f;

    private void OnDestroy()
    {
        if (Current == this) Current = null;
        // OnDisable should have already unsubscribed, but good practice if object is destroyed directly.
        // SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}