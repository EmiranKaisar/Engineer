using System;
using System.Collections.Generic;
using UnityEngine;

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
            }
        }
        
    }

    public void PlayerAudioSourcePlay(int playerIndex, PlayerAudioEnum playerAudioEnum)
    {
        playerAudioSourceList[playerIndex].clip = audioClipList[(int)playerAudioEnum].audioClip;
        playerAudioSourceList[playerIndex].Play();
    }
    
    
}
