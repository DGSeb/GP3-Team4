using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI")]
    // Reference to menu buttons
    [SerializeField] private GameObject buttons;
    [SerializeField] private GameObject settingsMenu;

    // Variables related to allowing the menus to be navigated with keyboard and controller.
    [Header("Menu Navigation")]
    public GameObject pauseFirstButton;
    public GameObject settingsFirstButton;
    public GameObject settingsClosedButton;
    public GameObject leaderboardClosedButton;

    // Leaderboard variables.
    [Header("Leaderboard")]
    [SerializeField] private GameObject leaderboard; // Leaderboard object.
    [SerializeField] private GameObject leaderboardSelection; // Screen that has selection for which leaderboard to display.
    private Leaderboard leaderboardScript; // reference to the leaderboard script.
    [SerializeField] private GameObject leaderboardSelectionFirstButton; // first button selected when leaderboard selection screen is active.
    [SerializeField] private GameObject leaderboardExitButton; // Exit button in the leaderboard.

    [Header("Audio")]
    [SerializeField] private AudioMixer master;

    [Header("Level Selection")]
    [SerializeField] private GameObject levelSelectionScreen;
    [SerializeField] private GameObject levelSelectionFirstButton;
    [SerializeField] private GameObject levelSelectedExitButton;

    void Start()
    {
        // Set frame rate to the limit chosen by the player. If none set, default to 100.
        Application.targetFrameRate = PlayerPrefs.GetInt("FPSLimit", 100);

        // Ensure that the player can use their mouse cursor on the main menu.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Set reference to leaderboard script.
        leaderboardScript = leaderboard.GetComponent<Leaderboard>();

        // Set audio levels for the game based on the player prefs audio settings.
        master.SetFloat("MasterVolume", Mathf.Log10(PlayerPrefs.GetFloat("MasterVolume", 0.5f)) * 20);
        master.SetFloat("MusicVolume", Mathf.Log10(PlayerPrefs.GetFloat("MusicVolume", 0.5f)) * 20);
        master.SetFloat("SFXVolume", Mathf.Log10(PlayerPrefs.GetFloat("SFXVolume", 0.5f)) * 20);
    }

    void Update()
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

    // Function to load scenes.
    void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Brings up the level selection menu.
    public void PlayGame()
    {
        // Turn off buttons UI and turn on the level selection screen.
        buttons.SetActive(false);
        levelSelectionScreen.SetActive(true);

        // Set the first selected button of the level selection screen for it to be navigable on controller and keyboard.
        SetSelectedUIButton(levelSelectionFirstButton);
    }

    // Function to exit the game when they are super angry.
    public void RageQuit()
    {
        Application.Quit();
        //print("SO ANGYYYYY");
    }
    
    // Load tutorial scene.
    public void PlayTutorial()
    {
        LoadScene("Tutorial");
    }

    // Load level 1 scene.
    public void PlayLevel1()
    {
        LoadScene("Level1");
    }

    // Load level 2 scene.
    public void PlayLevel2()
    {
        LoadScene("Level2");
    }

    // Load level 3 scene.
    public void PlayLevel3()
    {
        LoadScene("Level3");
    }

    // Exit the level selection screen
    public void ExitLevelSelection()
    {
        levelSelectionScreen.SetActive(false);
        buttons.SetActive(true);

        SetSelectedUIButton(levelSelectedExitButton);
    }

    // Function to play the old and smaller tutorial.
    public void PlaySmallTutorial()
    {
        LoadScene("Tutorial (Small Version)");
    }

    // Function to turn on the settings menu.
    public void ActivateSettings()
    {
        // Turn off buttons UI and turn on settings UI.
        buttons.SetActive(false);
        settingsMenu.SetActive(true);

        // Set the first selected button in the settings menu to make it navigable on controller and keyboard.
        SetSelectedUIButton(settingsFirstButton);
    }

    // Display leaderboard selection screen
    public void LeaderboardSelectionScreen()
    {
        // Turn off the buttons and turn on the leaderboard selection screen.
        buttons.SetActive(false);
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
        buttons.SetActive(true);

        SetSelectedUIButton(leaderboardClosedButton);
    }

    // Sets the currentlt selected UI button
    void SetSelectedUIButton(GameObject selectedButton)
    {
        // Clear any selected object in the event system and set a new selected object.
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectedButton);
    }
}