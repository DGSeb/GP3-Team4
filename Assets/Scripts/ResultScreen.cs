using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ResultScreen : MonoBehaviour
{
    // Objects that determine what button will start as selected.
    [SerializeField] private GameObject lostFirstButton;
    [SerializeField] private GameObject wonFirstButton;

    // String that stores the scene the player was in before they went to the results screen.
    public static string lastScene;

    // String that stores the next scene the player will go to, which is used for the next level button.
    public static string nextScene;

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

            // Clear any selected object in the event system and set a new selected object.
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(lostFirstButton);
        }
        // If the player won, ensure the lost UI is off and turn on the won UI.
        else if (won)
        {
            lostUI.SetActive(false);
            wonUI.SetActive(true);

            // Set the run time and pb time texts to the variable values that correspond with them.
            runTimeSecondsText.text = runTime;
            pBTimeText.text = pBTime;

            // Clear any selected object in the event system and set a new selected object.
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(wonFirstButton);
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

    // Lets the player go to the next level.
    public void PlayNextLevel()
    {
        GameManager.isPlayerActive = true;
        LoadScene(nextScene);
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