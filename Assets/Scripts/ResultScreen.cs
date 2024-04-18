using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;

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

    // Leaderboard variables.
    [Header("Leaderboard")]
    [SerializeField] private GameObject leaderboard; // Leaderboard object.
    [SerializeField] private GameObject leaderboardSelection; // Screen that has selection for which leaderboard to display.
    private Leaderboard leaderboardScript; // reference to the leaderboard script.
    [SerializeField] private GameObject leaderboardSelectionFirstButton; // first button selected when leaderboard selection screen is active.
    [SerializeField] private GameObject leaderboardExitButton; // Exit button in the leaderboard.

    // Button selected when the leaderboard is closed. As there is one button for win UI and lose UI, set leaderboardCloseButton when checking win state.
    private GameObject leaderboardClosedButton;
    [SerializeField] private GameObject wonLeaderboardClosedButton;
    [SerializeField] private GameObject lostLeaderboardClosedButton;


    void Start()
    {
        // Set reference to leaderboard script.
        leaderboardScript = leaderboard.GetComponent<Leaderboard>();

        // Check if the player won or lost.
        CheckWinState();

        // If the player won, only need to set the times at the very start.
        if (won)
        {
            // Set the run time and pb time texts to the variable values that correspond with them.
            runTimeSecondsText.text = runTime;
            pBTimeText.text = pBTime;
        }

        // Ensure that the player can use their mouse cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Check the input given by the player.
        CheckInput();    
    }

    // Check whether the player won or lost and display the corresponding UI.
    void CheckWinState()
    {
        // If the player lost, ensure won UI is off and turn on the lost UI.
        if (lost)
        {
            wonUI.SetActive(false);
            lostUI.SetActive(true);

            // Set the selected UI button to the lost first button.
            SetSelectedUIButton(lostFirstButton);

            // Set the leaderboard close button to the one for losing the game.
            leaderboardClosedButton = lostLeaderboardClosedButton;
        }
        // If the player won, ensure the lost UI is off and turn on the won UI.
        else if (won)
        {
            lostUI.SetActive(false);
            wonUI.SetActive(true);

            // Set the selected UI button to the won first button.
            SetSelectedUIButton(wonFirstButton);

            // Set the closed leaderboard button to the one for winning the game.
            leaderboardClosedButton = wonLeaderboardClosedButton;
        }
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

    // Display leaderboard selection screen
    public void LeaderboardSelectionScreen()
    {
        // Turn off the won and lost UIs and turn on the leaderboard selection screen.
        wonUI.SetActive(false);
        lostUI.SetActive(false);
        leaderboardSelection.SetActive(true);

        // Set the first selected leaderboard selection button to make the leaderboard selection screen navigable on controller and keyboard.
        SetSelectedUIButton(leaderboardSelectionFirstButton);
    }

    // Display tutorial leaderboard
    public void LeaderboardTutorial()
    {
        // Load the tutorial leaderboard entries, turn off the selection screen, and turn the leaderboard on.
        leaderboardScript.LoadLeaderboard("PBLeaderboardTutorial");
        leaderboardSelection.SetActive(false);
        leaderboard.SetActive(true);

        // Set the first selected button to the exit button so the leaderboard can be exited on controller and keyboard.
        SetSelectedUIButton(leaderboardExitButton);
    }

    // Display level 1 leaderboard
    public void LeaderboardLevel1()
    {
        // Load the level 1 leaderboard entries, turn off the selection screen, and turn the leaderboard on.
        leaderboardScript.LoadLeaderboard("PBLeaderboardLevel1");
        leaderboardSelection.SetActive(false);
        leaderboard.SetActive(true);

        // Set the first selected button to the exit button so the leaderboard can be exited on controller and keyboard.
        SetSelectedUIButton(leaderboardExitButton);
    }

    // Display level 2 leaderboard
    public void LeaderboardLevel2()
    {
        // Load the level 2 leaderboard entries, turn off the selection screen, and turn the leaderboard on.
        leaderboardScript.LoadLeaderboard("PBLeaderboardLevel2");
        leaderboardSelection.SetActive(false);
        leaderboard.SetActive(true);

        // Set the first selected button to the exit button so the leaderboard can be exited on controller and keyboard.
        SetSelectedUIButton(leaderboardExitButton);
    }

    // Display level 3 leaderboard
    public void LeaderboardLevel3()
    {
        // Load the level 3 leaderboard entries, turn off the selection screen, and turn the leaderboard on.
        leaderboardScript.LoadLeaderboard("PBLeaderboardLevel3");
        leaderboardSelection.SetActive(false);
        leaderboard.SetActive(true);

        // Set the first selected button to the exit button so the leaderboard can be exited on controller and keyboard.
        SetSelectedUIButton(leaderboardExitButton);
    }

    // Exit the leaderboard and go back to the leaderboard selection screen.
    public void ExitLeaderboard()
    {
        // Turn off the leaderboard and turn on the leaderboard selection menu.
        leaderboard.SetActive(false);
        leaderboardSelection.SetActive(true);

        // Set the selected UI button to the settings closed button.
        SetSelectedUIButton(leaderboardSelectionFirstButton);
    }

    // Exit the leaderboard selection screen and go back the the menu.
    public void ExitLeaderboardSelection()
    {
        // Turn off the leaderboard selection screen and turn on the win or loss UI depending on whether player won or loss.
        leaderboardSelection.SetActive(false);
        CheckWinState();

        SetSelectedUIButton(leaderboardClosedButton);
    }

    // Sets the currentlt selected UI button
    void SetSelectedUIButton(GameObject selectedButton)
    {
        // Clear any selected object in the event system and set a new selected object.
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectedButton);
    }

    // Check for player input.
    void CheckInput()
    {
        // If escape is pressed, see what UI is currently active and set it to false.
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (leaderboardSelection.activeSelf)
            {
                ExitLeaderboardSelection();
            }
            else if (leaderboard.activeSelf)
            {
                ExitLeaderboard();
            }
        }
    }
}