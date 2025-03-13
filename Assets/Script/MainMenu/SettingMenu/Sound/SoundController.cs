using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;


public class SoundController : MonoBehaviour
{
    public string audioFolderPath = "Assets/Sound";
    public AudioSource audioSource;
    private string[] audioFiles;
    public AudioMixerGroup Mixer;
    private int currentAudioIndex = 0;

    public float MusicVolume;
    private UnityWebRequest audioRequest;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
            GetMusicVolume(MusicVolume);
        }
        else
        {
            MusicVolume = -50f;
        }

        audioFiles = Directory.GetFiles(audioFolderPath, "*.mp3");
        Mixer.audioMixer.SetFloat("MusicVolume", MusicVolume);
        LoadAndPlayAudio(audioFiles[currentAudioIndex]);
    }

    private void Update()
    {
        Debug.Log(audioSource.time);
        if (audioRequest != null && audioRequest.isDone)
        {
            if (audioRequest.result == UnityWebRequest.Result.Success)
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(audioRequest);
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Error loading audio file: " + audioRequest.error);
            }
            audioRequest = null;
        }

        if (!audioSource.isPlaying && audioFiles.Length > 0)
        {
            currentAudioIndex = (currentAudioIndex + 1) % audioFiles.Length;
            LoadAndPlayAudio(audioFiles[currentAudioIndex]);
        }
    }

    public void GetMusicVolume(float value)
    {
        MusicVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        Mixer.audioMixer.SetFloat("MusicVolume", MusicVolume);
        

        if (audioSource.isPlaying)
        {
            audioSource.volume = Mathf.Pow(10, MusicVolume / 20);
        }
    }

    private void LoadAndPlayAudio(string filePath)
    {
        audioRequest = UnityWebRequestMultimedia.GetAudioClip("file:///" + filePath, AudioType.MPEG);
        audioRequest.SendWebRequest();
    }

    public void SaveTrack()
    {
        PlayerPrefs.SetInt("TrackNum", currentAudioIndex);
        PlayerPrefs.SetString("NameTrack", audioFiles[currentAudioIndex]);
        PlayerPrefs.SetFloat("TimeTrack", audioSource.time);
        PlayerPrefs.Save();
    }
}

//MusicVolume