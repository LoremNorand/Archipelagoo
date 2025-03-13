using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;

public class SoundControllerGame : MonoBehaviour
{
    public string audioFolderPath = "Assets/Sound";
    public AudioSource audioSource;
    private string[] audioFiles;
    private int currentAudioIndex = 0;
    bool first;
    private UnityWebRequest audioRequest;

    void Awake()
    {
        currentAudioIndex = PlayerPrefs.GetInt("TrackNum", 0);
        audioFiles = Directory.GetFiles(audioFolderPath, "*.mp3");
        FirstLoad();
        first = true;
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
                if(first)
                {
                    audioSource.time = PlayerPrefs.GetFloat("TimeTrack", 0f);
                    first= false;
                }
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

    private void FirstLoad()
    {
        string savedTrackPath = PlayerPrefs.GetString("NameTrack", audioFiles[0]);
        audioRequest = UnityWebRequestMultimedia.GetAudioClip("file:///" + savedTrackPath, AudioType.MPEG);
        audioRequest.SendWebRequest();
    }

    private void LoadAndPlayAudio(string filePath)
    {
        audioRequest = UnityWebRequestMultimedia.GetAudioClip("file:///" + filePath, AudioType.MPEG);
        audioRequest.SendWebRequest();
    }
}
