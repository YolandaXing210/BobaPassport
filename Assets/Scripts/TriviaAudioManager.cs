using UnityEngine;
using System.Collections;

public class TriviaAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource effectsSource; // For sound effects
    public AudioSource musicSource; // For background music
    
    [Header("Sound Effects")]
    public AudioClip ballHitSound;
    public AudioClip correctAnswerSound;
    public AudioClip wrongAnswerSound;
    public AudioClip gameStartSound;
    public AudioClip gameEndSound;
    public AudioClip countdownSound;
    public AudioClip buttonClickSound;
    
    [Header("Background Music")]
    public AudioClip gameplayMusic;
    public AudioClip menuMusic;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)] public float effectsVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    public bool enableAudio = true;
    
    // Singleton pattern for easy access
    public static TriviaAudioManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Setup audio sources
        SetupAudioSources();
    }
    
    private void Start()
    {
        // Play menu music
        PlayMusic(menuMusic);
    }
    
    void SetupAudioSources()
    {
        // Create audio sources if not assigned
        if (effectsSource == null)
        {
            effectsSource = gameObject.AddComponent<AudioSource>();
            effectsSource.playOnAwake = false;
        }
        
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        // Set initial volumes
        UpdateVolume();
    }
    
    public void UpdateVolume()
    {
        if (effectsSource != null)
            effectsSource.volume = effectsVolume;
        
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }
    
    // Sound effect methods
    public void PlayBallHit()
    {
        PlayEffect(ballHitSound);
    }
    
    public void PlayCorrectAnswer()
    {
        PlayEffect(correctAnswerSound);
    }
    
    public void PlayWrongAnswer()
    {
        PlayEffect(wrongAnswerSound);
    }
    
    public void PlayGameStart()
    {
        PlayEffect(gameStartSound);
        PlayMusic(gameplayMusic);
    }
    
    public void PlayGameEnd()
    {
        PlayEffect(gameEndSound);
        PlayMusic(menuMusic);
    }
    
    public void PlayCountdown()
    {
        PlayEffect(countdownSound);
    }
    
    public void PlayButtonClick()
    {
        PlayEffect(buttonClickSound);
    }
    
    // Generic sound effect player
    public void PlayEffect(AudioClip clip)
    {
        if (!enableAudio || effectsSource == null || clip == null) return;
        
        effectsSource.PlayOneShot(clip);
    }
    
    // Music control
    public void PlayMusic(AudioClip clip)
    {
        if (!enableAudio || musicSource == null || clip == null) return;
        
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        
        StartCoroutine(FadeToNewMusic(clip));
    }
    
    public void StopMusic()
    {
        if (musicSource != null)
        {
            StartCoroutine(FadeOutMusic());
        }
    }
    
    // Fade transitions for smooth music changes
    IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        // Fade out current music
        float startVolume = musicSource.volume;
        
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / 0.5f; // 0.5 second fade
            yield return null;
        }
        
        // Change clip and fade in
        musicSource.clip = newClip;
        musicSource.Play();
        
        while (musicSource.volume < musicVolume)
        {
            musicSource.volume += musicVolume * Time.deltaTime / 0.5f;
            yield return null;
        }
        
        musicSource.volume = musicVolume;
    }
    
    IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / 0.5f;
            yield return null;
        }
        
        musicSource.Stop();
        musicSource.volume = musicVolume; // Reset for next time
    }
    
    // Audio settings
    public void SetEffectsVolume(float volume)
    {
        effectsVolume = Mathf.Clamp01(volume);
        UpdateVolume();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolume();
    }
    
    public void ToggleAudio()
    {
        enableAudio = !enableAudio;
        
        if (!enableAudio)
        {
            if (effectsSource != null) effectsSource.mute = true;
            if (musicSource != null) musicSource.mute = true;
        }
        else
        {
            if (effectsSource != null) effectsSource.mute = false;
            if (musicSource != null) musicSource.mute = false;
        }
    }
    
    // Countdown timer audio (play beep every second for last 10 seconds)
    public void StartCountdownAudio(float timeRemaining)
    {
        if (timeRemaining <= 10f && timeRemaining > 0f)
        {
            PlayCountdown();
        }
    }
}