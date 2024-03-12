using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Function to load scenes.
    void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Function to play the game.
    public void PlayGame()
    {
        GameManager.isPlayerActive = true;
        LoadScene("LiamsWackyWonderland");
    }

    // Function to exit the game when they are super angry.
    public void RageQuit()
    {
        Application.Quit();
        print("SO ANGYYYYY");
    }
    
    // Function to play the tutorial.
    public void PlayTutorial()
    {
        GameManager.isPlayerActive = true;
        LoadScene("AveryScene");
    }

    // Function to play the old and smaller tutorial.
    public void PlaySmallTutorial()
    {
        GameManager.isPlayerActive = true;
        LoadScene("Tutorial (Small Version)");
    }
}