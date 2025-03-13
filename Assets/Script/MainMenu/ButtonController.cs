using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ButtonController : MonoBehaviour
{
    public Text HelloText;
    public Button ButtonAuthorization;
    public Button ButtonNewPlay;
    public Button ButtonSetting;
    public Button ButtonSettingExit;
    public Button ButtonExit;
    public GameObject SettingPanel;
    public SoundController soundController;
    void Start()
    {
        if (PlayerPrefs.HasKey("player name"))
        {
            HelloText.text =  $"Привет, {PlayerPrefs.GetString("player name")}!";
        }
        else
        {
            HelloText.text = "Нет пользователя";
        }
        //Debug.Log(PlayerPrefs.GetInt("id player"));
        ButtonAuthorization.onClick.AddListener(AutorizacionButtonCklick);
        ButtonNewPlay.onClick.AddListener(Play);
        ButtonSetting.onClick.AddListener(SettingOpen);
        ButtonSettingExit.onClick.AddListener(SettingClose);
        ButtonExit.onClick.AddListener(Exit);
    }
    private void Exit()
    {
        Application.Quit();
    }
    void SettingOpen()
    {
        SettingPanel.gameObject.SetActive(true);
    }
    void SettingClose()
    {
        SettingPanel.gameObject.SetActive(false);
    }
    void AutorizacionButtonCklick()
    {
        SceneManager.LoadScene("RA");
    }
    private void Play()
    {
        soundController.SaveTrack();
        SceneManager.LoadScene("SampleScene");
    }
}