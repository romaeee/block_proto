using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void StartGameTimerMode()
    {
        SceneManager.LoadScene("Game");
    }

    public void Chooselvl()
    {
        SceneManager.LoadScene("LevelMenu");
    }

    public void Menu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Clean()
    {
        PlayerPrefs.DeleteAll();
    }

    public void Exit()
    {
        Application.Quit();
    }
}