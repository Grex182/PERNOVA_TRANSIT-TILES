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
    [SerializeField] private AudioSource announcementSource;
    [SerializeField] private AudioSource tutorialSource;

    [Header("------ Audio Clips ------")]
    public AudioClip[] musicClips; // [0] = Main Menu, [1] = Game Scene
    public AudioClip[] sfxClips;
    public AudioClip[] maleVoiceClips;
    public AudioClip[] femaleVoiceClips;
    public AudioClip[] announcerVoiceClips;
    public AudioClip[] claireVoiceClips;

    [Header("------ Audio Volume ------")]
    public float musicVolume = 0.5f;
    public float sfxVolume = 0.5f;

    // Announcements
    public Coroutine announcementCoroutine;

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

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        voiceSource.volume = sfxVolume;
        tutorialSource.volume = 0.5f;
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
        voiceSource.pitch = Random.Range(.8f, 1.2f);
        voiceSource.Play();
    }

    public void PlayTutorial(int index)
    {
        tutorialSource.clip = claireVoiceClips[index];
        tutorialSource.PlayOneShot(tutorialSource.clip);
    }

    public void PlayAnnouncement(int index)
    {
        AudioClip clip = announcerVoiceClips[index];
        announcementSource.clip = clip;
        announcementSource.PlayOneShot(announcementSource.clip);
    }

    public void StopTutorial()
    {
        if (tutorialSource.isPlaying)
        {
            tutorialSource.Stop();
        }
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

    public void StopAnnounce()
    {
        if (announcementSource.isPlaying)
        {
            announcementSource.Stop();
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

        if (announcementSource.isPlaying)
        {
            announcementSource.Pause();
        }

        if (tutorialSource.isPlaying)
        {
            tutorialSource.Pause();
        }
    }

    public void ResumeAudio()
    {
        if (musicSource != null) musicSource.UnPause();
        if (sfxSource != null) sfxSource.UnPause();
        if (voiceSource != null) voiceSource.UnPause();
        if (announcementSource != null) announcementSource.UnPause();
        if (tutorialSource != null) tutorialSource.UnPause();

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
        announcementSource.volume = sfxVolume;
    }

    public void DoAnnouncementCoroutine(MovementState state, StationColor station)
    {
        if (announcementCoroutine != null)
        {
            StopCoroutine(announcementCoroutine);
            announcementCoroutine = null;
        }

        announcementCoroutine = StartCoroutine(PlayStationAnnouncement(state, station));
    }

    private IEnumerator PlayStationAnnouncement(MovementState state, StationColor station)
    {
        int stationIndex = GetStationAudioIndex(station);

        switch (state)
        {
            case MovementState.Station:
                PlaySFX(sfxClips[1], false); // Announcer Chime
                yield return new WaitForSeconds(sfxClips[1].length);

                PlayAnnouncement(stationIndex); // Station
                yield return null;
                break;

            case MovementState.Travel:
                PlaySFX(sfxClips[1], false); // Announcer Chime
                yield return new WaitForSeconds(sfxClips[1].length);

                PlayAnnouncement(10); // Now Approaching...
                yield return new WaitForSeconds(announcerVoiceClips[10].length);

                PlayAnnouncement(stationIndex); // Station
                yield return null;
                break;
        }
    }

    private int GetStationAudioIndex(StationColor station)
    {
        return station switch
        {
            StationColor.Red => 12,
            StationColor.Pink => 13,
            StationColor.Orange => 14,
            StationColor.Yellow => 15,
            StationColor.Green => 16,
            StationColor.Blue => 17,
            StationColor.Violet => 18,
            _ => -1 // Invalid case
        };
    }
}