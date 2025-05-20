using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class VideoData
{
    public string videoName;
    public float volume;
}

[Serializable]
public class VideoSetting
{
    public VideoData[] videoData;
}

[Serializable]
public class AudioData
{
    public string audioName;
    public float volume;
}

[Serializable]
public class AudioSetting
{
    public AudioData[] audioData;
}

public enum AudioName
{
    MainBGM,
    Touch,
    Down,
    Up
}

public class AudioManager : MonoBehaviour
{
    public AudioSetting AudioSetting { get; set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource mainBgm;
    [SerializeField] private AudioSource touch;
    [SerializeField] private AudioSource down;
    [SerializeField] private AudioSource up;

    [Space]
    [Header("Video Sources")]
    //[SerializeField] private AudioSource video000;
    
    
    public static AudioManager Instance;
    
    private Dictionary<AudioName, AudioSource> _audioSources;
    private Dictionary<string, AudioSource> _audioSourcesFromString;
    private Dictionary<string, AudioSource> _videoSourcesFromString;
    
    private bool _dataSet;

    private void Awake()
    {
        Instance = this;
                
        if (!_dataSet)
        {
            SetDictionary();
        }
    }

    private void SetDictionary()
    {
        _audioSources = new Dictionary<AudioName, AudioSource>
        {
            { AudioName.MainBGM, mainBgm },
            { AudioName.Touch, touch },
            { AudioName.Down, down },
            { AudioName.Up, up },
        };
        /*_audioSourcesFromString = new Dictionary<string, AudioSource>
        {
            { "batteryBlink", batteryBlink },
        };

        _videoSourcesFromString = new Dictionary<string, AudioSource>
        {
            { "video000", video000 },
        };*/
    }
    
    public void SetAudioSetting(AudioSetting audioSetting)
    {
        if (!_dataSet)
        {
            SetDictionary();
        }
        
        foreach (var audioData in audioSetting.audioData)
        {
            if (_audioSourcesFromString.TryGetValue(audioData.audioName, out AudioSource source))
            {
                source.volume = audioData.volume;
            }
        }
    }
    
    public void SetVideoSetting(VideoSetting videoSetting)
    {
        if (!_dataSet)
        {
            SetDictionary();
        }
        
        foreach (var videoData in videoSetting.videoData)
        {
            if (_videoSourcesFromString.TryGetValue(videoData.videoName, out AudioSource source))
            {
                source.volume = videoData.volume;
            }
        }
    }

    public void PlayAudio(AudioName audioName, bool playSound)
    {
        if (_audioSources.TryGetValue(audioName, out AudioSource source))
        {
            if (playSound)
                source.Play();
            else
                source.Stop();
        }
        else
        {
            Debug.LogWarning($"Audio does not exist! : {audioName}");
        }
    }
    
    public void PlayOneShotAudio(AudioName audioName)
    {
        if (_audioSources.TryGetValue(audioName, out AudioSource source))
        {
            source.PlayOneShot(source.clip);
        }
        else
        {
            Debug.LogWarning($"Audio does not exist! : {audioName}");
        }
    }

    private IEnumerator PlayContinuously(AudioSource firstAudio, AudioSource secondAudio)
    {
        firstAudio.Play();
        yield return new WaitForSeconds(firstAudio.clip.length);
        secondAudio.Play();
    }

    public void PlayAudioDelay(AudioName audioName, float delaySeconds)
    {
        StartCoroutine(DelayAudio(audioName, delaySeconds));
    }

    private IEnumerator DelayAudio(AudioName audioName, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        PlayAudio(audioName, true);
    }

    public void StopAllAudio()
    {
        mainBgm.Stop();
    }

    public AudioSource GetAudioSource(AudioName audioName)
    {
        if (_audioSources.TryGetValue(audioName, out AudioSource source))
        {
            return source;
        }
        else
        {
            Debug.LogWarning($"Audio does not exist! : {audioName}");
            return null;
        }
    }
    
    public void FadeAudio(AudioName audioName, float value, float fadeSeconds)
    {
        if (_audioSources.TryGetValue(audioName, out AudioSource source))
        {
            source.DOFade(value, fadeSeconds);
        }
        else
        {
            Debug.LogWarning($"Audio does not exist! : {audioName}");
        }
    }
}