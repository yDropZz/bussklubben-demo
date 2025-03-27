using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

public static SoundManager Instance;

[Header("Audio Sources")]
[SerializeField] private AudioSource effectsSource;
[SerializeField] private AudioSource musicSource;

[Header("Audio Clips")]
public AudioClip[] soundEffects;


    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if(clip != null)
        {
            effectsSource.PlayOneShot(clip);
        }
    }

    public void PlaySoundEffect(AudioClip clip, float volume)
    {
        if(clip != null)
        {
            //Play effectSource at the position
            effectsSource.PlayOneShot(clip, volume);

        }
    }

    public void PlaySoundEffect(string clipName)
    {
        AudioClip clip = GetClipByName(clipName);
        if(clip != null)
        {
            effectsSource.PlayOneShot(clip);
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if(clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    private AudioClip GetClipByName(string clipName)
    {
        foreach(AudioClip clip in soundEffects)
        {
            if(clip.name == clipName)
            {
                return clip;
            }
        }
        Debug.Log("No clip found with name: " + clipName);
        return null;
    }

}
