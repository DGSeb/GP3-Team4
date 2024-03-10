using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void PlayGame()
    {
        GameManager.isPlayerActive = true;
        LoadScene("LiamsWackyWonderland");
    }

    public void RageQuit()
    {
        Application.Quit();
        print("SO ANGYYYYY");
    }
    
    public void PlayTutorial()
    {
        GameManager.isPlayerActive = true;
        LoadScene("AveryScene");
    }
}