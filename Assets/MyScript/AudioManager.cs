using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Serializable]
    public class AudioClipClass
    {
        public PlayerAudioEnum playerAudioName;
        public AudioClip audioClip;
    }
    
    [SerializeField]
    public List<AudioClipClass> audioClipList;

    [HideInInspector] public List<AudioSource> playerAudioSourceList = new List<AudioSource>();

    public AudioSource BGMAudioSource;

    public Slider musicSlider;
    public Slider soundSlider;
    
    [Range(0f, 1f)]
    public float defaultMusicVolume;
    [Range(0f, 1f)]
    public float defaultSoundVolume;

    private int playerCount;

    public static AudioManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
        playerCount = 0;
        InitAudio();

    }

    public void InitAudio()
    {
        AssignBGMAudioSource();
        AssignPlayerAudioSource();
    }

    public void AssignBGMAudioSource()
    {
        SetVolume(BGMAudioSource, defaultMusicVolume);
        SetSliderValue(musicSlider, defaultMusicVolume);
    }


    public void AssignPlayerAudioSource()
    {
        //if player count changed then we need to rearrange the audio source
        if (playerCount != GameManager.Instance.playerList.Count)
        {
            playerAudioSourceList.Clear();
            foreach (var obj in GameManager.Instance.playerList)
            {
                playerAudioSourceList.Add(obj.GetComponent<AudioSource>());
                SetVolume(obj.GetComponent<AudioSource>(), defaultSoundVolume);
            }
            
            
        }
        
        SetSliderValue(soundSlider, defaultSoundVolume);
    }

    public void PlayerAudioSourcePlay(int playerIndex, PlayerAudioEnum playerAudioEnum)
    {
        playerAudioSourceList[playerIndex].clip = audioClipList[(int)playerAudioEnum].audioClip;
        playerAudioSourceList[playerIndex].Play();
    }

    private void SetVolume(AudioSource source, float volume)
    {
        source.volume = volume;
    }

    private void SetSliderValue(Slider slider, float value)
    {
        slider.value = value;
    }

    public void SliderSetSoundVolume()
    {
        foreach (var source in playerAudioSourceList)
        {
            source.volume = soundSlider.value;
        }
    }

    public void SliderSetMusicVolume()
    {
        BGMAudioSource.volume = musicSlider.value;
    }
    
    
}
