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
    // Reference to settings menu object and menu buttons
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject buttons;

    [Header("FPS")]
    // FPS variables
    [SerializeField] private TextMeshProUGUI fpsDisplay;
    [SerializeField] private Toggle fpsToggle;
    private float fps;
    private float fpsUpdateFrequency = 0.125f;
    private float fpsUpdateTimer;

    // Variables related to allowing the menus to be navigated with keyboard and controller.
    [Header("Menu Navigation")]
    public GameObject pauseFirstButton;
    public GameObject settingsFirstButton;
    public GameObject settingsClosedButton;
    public GameObject leaderboardClosedButton;

    // Scrollbar movement on controller
    [SerializeField] private Scrollbar scrollbar;
    private float scrollSpeed = 0.0035f;

    // Leaderboard variables.
    [Header("Leaderboard")]
    [SerializeField] private GameObject leaderboard; // Leaderboard object.
    [SerializeField] private GameObject leaderboardSelection; // Screen that has selection for which leaderboard to display.
    private Leaderboard leaderboardScript; // reference to the leaderboard script.
    [SerializeField] private GameObject leaderboardSelectionFirstButton; // first button selected when leaderboard selection screen is active.
    [SerializeField] private GameObject leaderboardExitButton; // Exit button in the leaderboard.

    [Header("Audio")]
    // Audio settings items.
    [SerializeField] private GameObject audioSettings;
    [SerializeField] private GameObject settingsScrollView;
    [SerializeField] private AudioMixer master;
    [SerializeField] private GameObject audioFirstButton;

    [Header("How To Play")]
    [SerializeField] private GameObject howToPlayScreen;
    [SerializeField] private GameObject howToPlayExitButton;

    [Header("Level Selection")]
    [SerializeField] private GameObject levelSelectionScreen;
    [SerializeField] private GameObject levelSelectionFirstButton;
    [SerializeField] private GameObject levelSelectedExitButton;

    void Start()
    {
        // Set starting frame rate to half of what monitor can do
        QualitySettings.vSyncCount = 2;

        // Ensure that the player can use their mouse cursor on the main menu.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Set the fps timer to the frequency that it will update at.
        fpsUpdateTimer = fpsUpdateFrequency;

        // Check if display fps is true to determine if it should be displayed
        if (GameManager.displayFPS)
        {
            fpsToggle.isOn = GameManager.displayFPS;
            GameManager.displayFPS = true;
            fpsDisplay.enabled = true;
        }

        // Set reference to leaderboard script.
        leaderboardScript = leaderboard.GetComponent<Leaderboard>();

        // Set audio levels for the game based on the player prefs audio settings.
        master.SetFloat("MasterVolume", Mathf.Log10(PlayerPrefs.GetFloat("MasterVolume", 0.5f)) * 20);
        master.SetFloat("MusicVolume", Mathf.Log10(PlayerPrefs.GetFloat("MusicVolume", 0.5f)) * 20);
        master.SetFloat("SFXVolume", Mathf.Log10(PlayerPrefs.GetFloat("SFXVolume", 0.5f)) * 20);
    }

    void Update()
    {
        // Run function that updates the FPS on screen if the bool is true.
        if (GameManager.displayFPS)
        {
            UpdateFPSDisplay();
        }

        // If the settings menu is active, check for controller input to scroll the scroll bar.
        if (settingsMenu.activeSelf)
        {
            // Use this for right joystick
            float controllerInputRight = Input.GetAxis("Controller Y");

            // use this for left joystick.
            float controllerInputLeft = Input.GetAxis("Vertical");

            // Combine both left and right joystick input so the player can use either on the settings menu.
            float controllerInput = controllerInputLeft + controllerInputRight;

            // Adjust the speed at which the scorllbar scrolls with input. Use this value for vertical axis.
            scrollSpeed = 0.0068f;

            // Multiply the controllerInput with the scroll speed and set the scrollbar's value to that.
            scrollbar.value += controllerInput * scrollSpeed;

            // Clamp the scrollbar's value so it can't go above 1 or below 0.
            scrollbar.value = Mathf.Clamp01(scrollbar.value);
        }

        // If escape is pressed, see what UI is currently active and set it to false.
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (settingsMenu.activeSelf)
            {
                ExitSettings();
            }
            else if (leaderboardSelection.activeSelf)
            {
                ExitLeaderboard();
            }
            else if (howToPlayScreen.activeSelf)
            {
                ExitHowToPlayScreen();
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

    // Function to turn off the settings menu.
    public void ExitSettings()
    {
        // Turn off settings UI, and turn on buttons UI.
        settingsMenu.SetActive(false);
        buttons.SetActive(true);

        // If the audio settings are currently active, turn off the audio settings and turn the settings scroll view back on.
        if (audioSettings.activeSelf)
        {
            audioSettings.SetActive(false);
            settingsScrollView.SetActive(true);
        }

        // Set the first button selected when closing the settings menu to make the menu navigable on controller and keyboard.
        SetSelectedUIButton(settingsClosedButton);
    }

    // Function that changes the display fps bool based on input in settings menu.
    public void ShowFPSDisplay()
    {
        // Set the bool to the opposite of what it currently is and set the enabled status of the fps display to the value of the bool.
        GameManager.displayFPS = !GameManager.displayFPS;
        fpsDisplay.enabled = GameManager.displayFPS;
    }

    // Function that updates the FPS counter on screen.
    void UpdateFPSDisplay()
    {
        // fpsUpdateTimer will equal 0 at the end of the current frame in seconds.
        fpsUpdateTimer -= Time.unscaledDeltaTime;

        // If fpsUpdateTimer is less than or equal to 0 seconds, display the number of frames. 
        if (fpsUpdateTimer <= 0f)
        {
            // fps 
            fps = 1f / Time.unscaledDeltaTime;

            // Display the rounded to a whole number fps value on screen.
            fpsDisplay.text = "FPS: " + Mathf.Round(fps);

            // Set the fpsUpdateTimer back to its original value to begin the cycle again.
            fpsUpdateTimer = fpsUpdateFrequency;
        }
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

    // Go back to menu
    public void ExitLeaderboard()
    {
        // Turn off the leaderboard selection menu.
        leaderboardSelection.SetActive(false);
        
        // If the leaderboard is active, turn it off.
        if (leaderboard.activeSelf)
        {
            leaderboard.SetActive(false);
        }

        // Turn the buttons back on.
        buttons.SetActive(true);

        // Set the selected UI button to the settings closed button.
        SetSelectedUIButton(leaderboardClosedButton);
    }

    // Display the audio settings menu.
    public void DisplayAudioSettings()
    {
        // Turn off the scrollview and turn on the audio settings menu.
        settingsScrollView.SetActive(false);
        audioSettings.SetActive(true);

        // Set the selected button to the first button in the audio menu.
        SetSelectedUIButton(audioFirstButton);
    }

    // Display the how to play screen.
    public void DisplayHowToPlayScreen()
    {
        // Turn off the buttons and turn on the how to play screen.
        buttons.SetActive(false);
        howToPlayScreen.SetActive(true);

        // Set the selected button to the exit button.
        SetSelectedUIButton(howToPlayExitButton);
    }

    // Exit the how to play screen.
    public void ExitHowToPlayScreen()
    {
        // Turn off the how to play screen and the settings menu. Then, turn on the buttons.
        howToPlayScreen.SetActive(false);
        settingsMenu.SetActive(false);
        buttons.SetActive(true);

        // Set the button selected to the settings button.
        SetSelectedUIButton(settingsClosedButton);
    }

    // Sets the currentlt selected UI button
    void SetSelectedUIButton(GameObject selectedButton)
    {
        // Clear any selected object in the event system and set a new selected object.
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(selectedButton);
    }
}