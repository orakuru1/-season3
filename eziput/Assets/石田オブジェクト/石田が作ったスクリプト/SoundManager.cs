using UnityEngine;
using System.Collections.Generic;
using System;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("AudioSources")]
    [SerializeField] public AudioSource bgmSource;
    [SerializeField] public AudioSource seSource;
    [SerializeField] public AudioClip audioClip;
    public List<AudioClip> seClipList = new List<AudioClip>();
    private Dictionary<string, AudioClip> seClips = new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);

        foreach (var clip in seClipList)
        {
            if (!seClips.ContainsKey(clip.name))
                seClips.Add(clip.name, clip);
        }

    }

    void Update()
    {
        // Test Code
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlaySE(audioClip);
        }
    }

    public void InitializeSettings(AudioSource bgmSource, AudioSource seSource)
    {
        this.bgmSource = bgmSource;
        this.seSource = seSource;
    }

    // ===== SE =====
    public void PlaySE(AudioClip clip)
    {
        if (clip == null) return;
        Debug.Log("Play SE: " + clip.name);
        seSource.PlayOneShot(clip);
    }

    // ===== SE by Name =====
    public void PlaySE2(string clipName)
    {
        if (seClips.ContainsKey(clipName))
        {
            PlaySE(seClips[clipName]);
        }
        else
        {
            Debug.LogWarning("SE Clip not found: " + clipName);
        }
    }

    // ===== BGM =====
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }
}
