using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreen : MonoBehaviour
{
    // String that stores the scene the player was in before they went to the results screen.
    public static string lastScene;

    // Bools that say whether the player lost or won the game. These will then determine what displays on the results screen.
    public static bool lost;
    public static bool won;

    // UI for the lose and win states. The UI that displays is based on whether the player lost or won the game.
    public GameObject lostUI;
    public GameObject wonUI;

    // Text element that will display the player's time on their last run and their PB for that level.
    public TextMeshProUGUI runTimeSecondsText;
    public TextMeshProUGUI pBTimeText;

    public static string runTime;
    public static string pBTime;
    void Start()
    {
        // If the player lost, ensure won UI is off and turn on the lost UI.
        if (lost)
        {
            wonUI.SetActive(false);
            lostUI.SetActive(true);
        }
        // If the player won, ensure the lost UI is off and turn on the won UI.
        else if (won)
        {
            lostUI.SetActive(false);
            wonUI.SetActive(true);

            // Set the run time and pb time texts to the variable values that correspond with them.
            runTimeSecondsText.text = runTime;
            pBTimeText.text = pBTime;
        }

        // Ensure that the player can use their mouse cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Function to load scenes.
    void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Function that reloads the last scene the player was in when the play again button is pressed.
    public void PlayAgain()
    {
        GameManager.isPlayerActive = true;
        LoadScene(lastScene);
    }

    // Function that loads the main menu when the main menu button is pressed.
    public void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }

    // Function that quits the game when the quit game button is pressed.
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("I just can't.");
    }
}