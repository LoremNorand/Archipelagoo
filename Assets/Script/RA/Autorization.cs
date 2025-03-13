using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Autorization : MonoBehaviour
{
    public DbController dbController;
    public Button autoButton;
    public InputField nameInput;
    public InputField passwordInput;
    public Text OutText;

    void Start()
    {
        autoButton.onClick.AddListener(Auto);
    }

    // Update is called once per frame
    void Auto()
    {
        if(string.IsNullOrEmpty(nameInput.text)||string.IsNullOrEmpty(passwordInput.text))
        {
            OutText.text = "Пустые поля ввода!";
            return;
        }
        if (dbController.ExaminationUser(nameInput.text))
        {
            OutText.text = "Такого пользователя нет!";
            return;
        }
        if(!dbController.PasswordChek(nameInput.text,passwordInput.text))
        {
            OutText.text = "Пароль не верный!";
            return;
        }
        OutText.text = "Вход";
        PlayerPrefs.SetString("player name", nameInput.text);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }
}
