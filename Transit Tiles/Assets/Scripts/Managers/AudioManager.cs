using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("------ Audio Sources ------")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource voiceSource;

    [Header("------ Audio Clips ------")]
    public AudioClip[] musicClips; // [0] = Main Menu, [1] = Game Scene
    public AudioClip[] sfxClips;
    public AudioClip[] voiceClips;

    [Header("------ Audio Volume ------")]
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        voiceSource.volume = sfxVolume;
    }

    public void PlayBGM(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, bool canLoop)
    {
        sfxSource.clip = clip;
        sfxSource.loop = canLoop;
        sfxSource.Play();
    }

    public void PlayVoice(AudioClip clip)
    {
        voiceSource.clip = clip;
        voiceSource.Play();
    }

    public void StopBGM()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void StopSFX()
    {
        if (sfxSource.isPlaying)
        {
            sfxSource.Stop();
        }
    }

    public void StopVoice()
    {
        if (voiceSource.isPlaying)
        {
            voiceSource.Stop();
        }
    }

    public void ChangeBgmVolume(float volume)
    {
        musicVolume = volume; //Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void ChangeSfxVolume(float volume)
    {
        sfxVolume = volume;
        sfxSource.volume = sfxVolume;
        voiceSource.volume = sfxVolume;
    }
}