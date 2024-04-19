using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    // Lets scripts know when the game is paused.
    public static bool gameIsPaused;

    // Screens on the pause menu.
    [Header("Pause Menu Screens")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject pauseScreenOneUI;
    [SerializeField] private GameObject settingsMenuUI;

    // Variables related to allowing the menus to be navigated with keyboard and controller.
    [Header("Pause Menu Navigation")]
    public GameObject pauseFirstButton;
    public GameObject settingsFirstButton;
    public GameObject settingsClosedButton;

    // Scrollbar movement on controller
    [SerializeField] private Scrollbar scrollbar;
    private float scrollSpeed = 0.0035f;

    [Header("Settings Menu Screens")]
    // Various settings screen for the various kinds of settings.
    // First item in the array is audio, second is sensitivity, third is performance, and fourth is controls. 
    [SerializeField] private GameObject[] settingsScreens;
    
    // The scroll view that contains the screen selection buttons.
    [SerializeField] private GameObject settingsScrollView;

    // Audio settings items.
    [Header("Audio Settings")]
    [SerializeField] private GameObject audioFirstButton;
    [SerializeField] private GameObject audioExitButton;

    // sensitivity settings
    [Header("Sensitivity Settings")]
    [SerializeField] private GameObject sensitivityFirstButton;
    [SerializeField] private GameObject sensitivityExitButton;

    // Performance settings
    [Header("Performance Settings")]
    [SerializeField] private GameObject performanceFirstButton;
    [SerializeField] private GameObject performanceExitButton;

    // Controls settings
    [Header("Controls Settings")]
    [SerializeField] private GameObject controlsFirstButton;
    [SerializeField] private GameObject controlsExitButton;

    [Header("FPS")]
    // FPS variables
    [SerializeField] private TextMeshProUGUI fpsDisplay;
    [SerializeField] private Toggle fpsToggle;
    private float fps;
    private float fpsUpdateFrequency = 0.125f;
    private float fpsUpdateTimer;
    public static bool displayFPS;

    [Header("Main Menu")]
    [SerializeField] private GameObject buttons;

    // Current scene the player is in.
    private string currentScene;

    // List that contains scenes in which the pause menu displays. The list is searched when determining if the pause menu should display.
    private List<string> pauseScenes = new List<string> { "Tutorial", "Level1", "Level2", "Level3" };

    // How to play screen.
    [Header("How To Play")]
    [SerializeField] private GameObject howToPlayScreen;
    [SerializeField] private GameObject howToPlayExitButton;

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;

        // Make sure the pause menu is off.
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // At start, ensure the game is not paused bool is false as the game is not paused.
        gameIsPaused = false;

        // Set the fps timer to the frequency that it will update at.
        fpsUpdateTimer = fpsUpdateFrequency;

        // Check if display fps is true to determine if it should be displayed
        if (displayFPS)
        {
            fpsToggle.isOn = displayFPS;
            displayFPS = true;
            fpsDisplay.enabled = true;
        }
    }

    void Update()
    {
        // Check the input provided to see if a screen or menu should be turned off.
        InputChecks();

        // If the settings menu is active, check for controller input to scroll the scroll bar.
        if (settingsMenuUI.activeSelf)
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

        // Run function that updates the FPS on screen if the bool is true.
        if (displayFPS)
        {
            UpdateFPSDisplay();
        }
    }

    // Function to resume the game.
    public void Resume()
    {
        // Lock the cursor, make it not visible, turn off the settings menu and pause menu UI, set time back to normal, set player active bool to true,
        // and set gameIsPaused to false as game is no longer paused.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        settingsMenuUI.SetActive(false);

        // Check that the pause menu has been assigned before turning it off.
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        gameIsPaused = false;
        GameManager.isPlayerActive = true;
        Time.timeScale = 1.0f;
    }

    // Function to pause the game.
    void Pause()
    {
        // Clear any selected object in the event system and set a new selected object.
        SetSelectedUIButton(pauseFirstButton);

        // Stop time, set player active bool to false, unlock player cursor so they can interact with buttons, turn on pause menu UI and its buttons,
        // and set gameIsPaused to true as game is now paused.
        Time.timeScale = 0f;
        GameManager.isPlayerActive = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenuUI.SetActive(true);
        pauseScreenOneUI.SetActive(true);
        gameIsPaused = true;
    }

    // Function to load the menu scene.
    public void LoadMenu()
    {
        // Set time back to normal and load main menu scene.
        Time.timeScale = 1.0f;
        ChangeScene("MainMenu");
        //Debug.Log("Loading menu...");
    }

    // Function to quit the game. 
    public void QuitGame()
    {
        // Quit the application (only works in builds).
        //Debug.Log("Quitting game...");
        Application.Quit();
    }

    // Function to turn on the settings menu.
    public void ActivateSettings()
    {
        pauseScreenOneUI.SetActive(false);

        // Clear any selected object in the event system and set a new selected object.
        SetSelectedUIButton(settingsFirstButton);
        settingsMenuUI.SetActive(true);
    }

    // Exit settings.
    public void ExitSettings()
    {
        // Set the scrollbar back to 1, so it is at the top of the settings when the player opens the settings menu.
        scrollbar.value = 1f;

        settingsMenuUI.SetActive(false);

        // Clear any selected object in the event system and set a new selected object.
        SetSelectedUIButton(settingsClosedButton);

        // If in the main menu, turn the buttons on. If not, turn on the pause screen buttons.
        if (currentScene == "MainMenu")
        {
            buttons.SetActive(true);
        }
        else
        {
            pauseScreenOneUI.SetActive(true);
        }
    }

    // Function that changes the display fps bool based on input in settings menu.
    public void ShowFPSDisplay()
    {
        displayFPS = !displayFPS;
        fpsDisplay.enabled = displayFPS;
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

    // Display the audio settings menu.
    public void DisplayAudioSettings()
    {
        // Set the selected button to the first button in the audio settings (master audio slider).
        SetSelectedUIButton(audioFirstButton);

        // Turn on position 0 in the array (audio settings).
        TurnOnSettingsMenu(0);
    }

    // Enter the sensitivity settings menu
    public void DisplaySensitivitySettings()
    {
        // Set the selected button to the first button in the sensitivity settings (mouse x slider).
        SetSelectedUIButton(sensitivityFirstButton);

        // Turn on position 1 in the array (sensitivity settings).
        TurnOnSettingsMenu(1);
    }

    // Enter the performance settings menu
    public void DisplayPerformanceSettings()
    {
        // Set the selected button to the first button in the performance settings (V-Sync toggle)
        SetSelectedUIButton(performanceFirstButton);

        // Turn on position 2 in the array (performance settings).
        TurnOnSettingsMenu(2);
    }

    // Enter the controls settings menu
    public void DisplayControlsSettings()
    {
        // Set the selected button to the first button in the controls settings (exit button).
        SetSelectedUIButton(controlsFirstButton);

        // Turn on position 3 in the array (controls settings)
        TurnOnSettingsMenu(3);
    }

    // Display the how to play screen.
    public void DisplayHowToPlayScreen()
    {
        // Turn off the settings menu and turn on the how to play screen.
        settingsMenuUI.SetActive(false);

        // Set the selected button to the exit button.
        SetSelectedUIButton(howToPlayExitButton);
        howToPlayScreen.SetActive(true);
    }

    // Exit the how to play screen.
    public void ExitHowToPlayScreen()
    {
        // Turn off the how to play screen and the settings menu. Then, turn on the buttons.
        howToPlayScreen.SetActive(false);

        // Set the button selected to the settings button.
        SetSelectedUIButton(settingsFirstButton);
        settingsMenuUI.SetActive(true);
    }

    // Turn on a specific settings menu
    void TurnOnSettingsMenu(int screenNum)
    {
        // Turn off the scroll view and turn on the settings screen array number inputted.
        settingsScrollView.SetActive(false);
        settingsScreens[screenNum].SetActive(true);
    }

    // Turn off the settings menu and reactivate the scrollview.
    public void TurnOffSettingsMenu()
    {
        // Turn off all the settings screen and turn on the scrollview.
        foreach (GameObject screen in settingsScreens)
        {
            screen.SetActive(false);
        }

        settingsScrollView.SetActive(true);
    }

    // Change the scene based on the inputted string when the function is called.
    void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Set the selected button based on what gameobject is inputted when the function is called.
    void SetSelectedUIButton(GameObject buttonName)
    {
        // Clear any selected object in the event system and set a new selected object.
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonName);
    }

    // Exit audio settings.
    public void ExitAudioSettings()
    {
        settingsScreens[0].SetActive(false);
        SetSelectedUIButton(audioExitButton);
        settingsScrollView.SetActive(true);
    }

    // Exit sensitivity settings.
    public void ExitSensitivitySettings()
    {
        settingsScreens[1].SetActive(false);
        SetSelectedUIButton(sensitivityExitButton);
        settingsScrollView.SetActive(true);
    }

    // Exit performance settings.
    public void ExitPerformanceSettings()
    {
        settingsScreens[2].SetActive(false);
        SetSelectedUIButton(performanceExitButton);
        settingsScrollView.SetActive(true);
    }

    // Exit controls settings.
    public void ExitControlsSettings()
    {
        settingsScreens[3].SetActive(false);
        SetSelectedUIButton(controlsExitButton);
        settingsScrollView.SetActive(true);
    }

    // Input checks for exiting the various screen and menus
    void InputChecks()
    {
        // If escape key is pressed and the player is in a level, check if the game is paused.
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (pauseScenes.Any(scene => scene == currentScene) && !gameIsPaused)
            {
                Pause();
            }

            // If the how to play screen is active, exit the how to play screen.
            else if (howToPlayScreen.activeSelf)
            {
                ExitHowToPlayScreen();
            }

            // If audio settings screen is active, turn off the settings menu and select the audio exit button on the scrollview.
            else if (settingsScreens[0].activeSelf)
            {
                ExitAudioSettings();
            }

            // If sensitivity settings screen is active, turn off the settings menu and select the sensitivity exit button on the scrollview.
            else if (settingsScreens[1].activeSelf)
            {
                ExitSensitivitySettings();
            }

            // If performance settings screen is active, turn off the settings menu and select the performance exit button on the scrollview.
            else if (settingsScreens[2].activeSelf)
            {
                ExitPerformanceSettings();
            }

            // If controls settings screen is active, turn off the settings menu and select the controls exit button on the scrollview.
            else if (settingsScreens[3].activeSelf)
            {
                ExitControlsSettings();
            }

            // If the settings menu is active, turn it off.
            else if (settingsMenuUI.activeSelf)
            {
                ExitSettings();
            }

            // If the pause menu exists, check if it is active.
            else if (currentScene != "MainMenu" && pauseMenuUI.activeSelf)
            {
                Resume();
            }
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            // If the how to play screen is active, exit the how to play screen.
            if (howToPlayScreen.activeSelf)
            {
                ExitHowToPlayScreen();
            }

            // If audio settings screen is active, turn off the settings menu and select the audio exit button on the scrollview.
            else if (settingsScreens[0].activeSelf)
            {
                ExitAudioSettings();
            }

            // If sensitivity settings screen is active, turn off the settings menu and select the sensitivity exit button on the scrollview.
            else if (settingsScreens[1].activeSelf)
            {
                ExitSensitivitySettings();
            }

            // If performance settings screen is active, turn off the settings menu and select the performance exit button on the scrollview.
            else if (settingsScreens[2].activeSelf)
            {
                ExitPerformanceSettings();
            }

            // If controls settings screen is active, turn off the settings menu and select the controls exit button on the scrollview.
            else if (settingsScreens[3].activeSelf)
            {
                ExitControlsSettings();
            }

            // If the settings menu is active, turn it off.
            else if (settingsMenuUI.activeSelf)
            {
                ExitSettings();
            }

            // If the pause menu exists, check if it is active.
            else if (currentScene != "MainMenu" && pauseMenuUI.activeSelf)
            {
                Resume();
            }
        }
    }
}