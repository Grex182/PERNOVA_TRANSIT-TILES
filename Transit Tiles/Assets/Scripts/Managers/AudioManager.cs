using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("------ Audio Sources ------")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource voiceSource;

    [Header("------ Audio Clips ------")]
    public AudioClip[] musicClips; // [0] = Main Menu, [1] = Game Scene
    public AudioClip[] sfxClips;
    public AudioClip[] maleVoiceClips;
    public AudioClip[] femaleVoiceClips;

    [Header("------ Audio Volume ------")]
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);  // Destroy any duplicates
        }
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
        sfxSource.PlayOneShot(sfxSource.clip);
    }

    public void PlayVoice(bool isFemale, int index)
    {
        AudioClip clip = isFemale ? femaleVoiceClips[index] : maleVoiceClips[index];
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

    public void PauseAudio()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
        }

        if (sfxSource.isPlaying)
        {
            sfxSource.Pause();
        }

        if (voiceSource.isPlaying)
        {
            voiceSource.Pause();
        }
    }

    public void ResumeAudio()
    {
        musicSource.Play();
        sfxSource.Play();
        voiceSource.Play();
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