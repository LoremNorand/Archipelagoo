using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundView : MonoBehaviour
{
    public Text VolumeText;
    public SoundController SoundController;
    public Slider Slider;
    public void Start()
    {
        Slider.value = SoundController.MusicVolume;
        Slider.onValueChanged.AddListener(ChangeVolumeMusic);
        ViewVolumeMusicText(Slider.value);
    }
    void ChangeVolumeMusic(float value)
    {
        SoundController.GetMusicVolume(value);
        ViewVolumeMusicText(value);
    }
    void ViewVolumeMusicText(float n)
    {
        int viewint = (int)((n + 50) * 2);
        VolumeText.text = $"{viewint}%";
    }

}
