using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("------ Audio Sources ------")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("------ Audio Clips ------")]
    public AudioClip[] musicClips; // [0] = Main Menu, [1] = Game Scene
    public AudioClip[] sfxClips;

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
    }

    public void PlayBGM(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.loop = true;
    }

    public void PlaySFX(AudioClip clip)
    {
        foreach (AudioClip sfx in sfxClips)
        {
            if (sfx == clip)
            {
                Debug.LogWarning($"SFX {clip.name} already playing. Ignoring duplicate play request.");
                return; // Avoid playing the same SFX again
            }
        }
        sfxSource.clip = clip;
        sfxSource.loop = false;
        sfxSource.Play();
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

    public void ChangeBgmVolume(float volume)
    {
        musicVolume = volume; //Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void ChangeSfxVolume(float volume)
    {
        sfxVolume = volume;
        sfxSource.volume = sfxVolume;
    }
}
