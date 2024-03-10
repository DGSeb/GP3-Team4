using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using System;

public class GameManager : MonoBehaviour
{
    // Components that display on UI.
    [Header("UI Components")]
    public TextMeshProUGUI timerText;
    public GameObject timeLimitReachedText;
    public TextMeshProUGUI pBText;
    [SerializeField] private GameObject pauseMenuUI;

    // Settings that control time.
    [Header("Timer Settings")]
    public float currentTime;
    public bool countDown;

    // Settings to control if there is a time limit.
    [Header("Limit Settings")]
    public bool hasLimit;
    public float timerLimit;

    // Settings to control the format of the timer.
    [Header("Format Settings")]
    public bool hasFormat;
    public TimerFormats format;
    private Dictionary<TimerFormats, string> timeFormats = new Dictionary<TimerFormats, string>();

    // Enum for the different formats of the time, such as 1 second or 1.1 seconds or 1.11 seconds or 1.11 seconds.
    public enum TimerFormats
    {
        Whole,
        TenthDecimal,
        HundrethsDecimal,
        ThousandthsDecimal
    }

    // Pause menu bool.
    public static bool gameIsPaused;

    // String to say which scene the player is currently in.
    private string currentScene;

    void Start()
    {
        // Add entries to the dictionary for the timer format.
        timeFormats.Add(TimerFormats.Whole, "0");
        timeFormats.Add(TimerFormats.TenthDecimal, "0.0");
        timeFormats.Add(TimerFormats.HundrethsDecimal, "0.00");
        timeFormats.Add(TimerFormats.ThousandthsDecimal, "0.000");

        // Find the name of the active scene and assign it to the currentScene variable.
        // Make sure this occurs before running UpdatePBText so it knows which scene it is in.
        currentScene = SceneManager.GetActiveScene().name;

        // Run the UpdatePBText that updates the text displaying the player's PB.
        UpdatePBText();

        // Make sure the pause menu is off.
        pauseMenuUI.SetActive(false);

    }

    void Update()
    {
        // ? : operator says do item on left side of : if true and do item on right side of : if false.
        // Set the current time to currentTime - deltaTime if it's counting down.
        // If not counting down, add deltaTime to the currentTime.
        currentTime = countDown ? currentTime -= Time.deltaTime : currentTime += Time.deltaTime;

        // If (there is a time limit and ((there is a count down and the currentTime is less than or equal to the limit) OR
        // (timer is counting up and current time is at or above the limit)))
        // Set the currentTime to the time limit, update the timer text, change its color, disable this component, and start coroutine indicating time limit reached.
        if(hasLimit && ((countDown && currentTime <= timerLimit) || (!countDown && currentTime >= timerLimit)))
        {
            currentTime = timerLimit;
            SetTimerText();
            timerText.color = Color.red;
            enabled = false;
            StartCoroutine(TimeLimitReached());
        }

        // Set timer text here so it occurs even if there isn't a limit.
        SetTimerText();

        // If escape key is pressed, check if the game is paused.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If paused, resume the game.
            if (gameIsPaused)
            {
                Resume();
            }
            // If not paused, pause the game.
            else
            {
                Pause();
            }
        }
    }

    // Function to set the text of the timer.
    void SetTimerText()
    {        
        // The bool in this case is hasFormat and the ? indicates checking for the value of the bool.
        // If there is a format, apply it. Otherwise, do not apply a format.
        timerText.text = hasFormat ? currentTime.ToString(timeFormats[format]) : currentTime.ToString();
    }

    // Function to change the scene based on the name inputted when function is called.
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // When time limit is reached, run coroutine that resets game.
    IEnumerator TimeLimitReached()
    {
        // Turn on text telling player the time limit is reached, stop time, wait about a second,
        // change scene to the one the player is currently in, resume time, and update text displaying player's PB.
        timeLimitReachedText.SetActive(true);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1.2f);
        ChangeScene(currentScene);
        Time.timeScale = 1.0f;
        UpdatePBText();
    }

    // Function to check the fastest time the player beat the level (PB (Personal Best)).
    // This function only runs when the player reaches the exit.
    public void CheckPB()
    {
        // If currently in the first scene and the current time is lower than the player's PB, the player has a new PB.
        // Set the player's PB to the currentTime and update the PB text.
        if (currentScene == "LiamsWackyWonderland" && (currentTime < PlayerPrefs.GetFloat("PB", timerLimit)))
        {
            PlayerPrefs.SetFloat("PB", currentTime);
            UpdatePBText();            
        }
        // If currently in the second scene and the current time is lower than the player's PB, the player has a new PB.
        // Set the player's PB to the currentTime and update the PB text.
        else if (currentScene == "LiamsHighlyPsychoticJoint" && (currentTime < PlayerPrefs.GetFloat("PB2", timerLimit)))
        {
            PlayerPrefs.SetFloat("PB2", currentTime);
            UpdatePBText();
        }
    }

    // Update text displaying the player's PB.
    void UpdatePBText()
    {
        // If in the first scene, set the PB text to the PB for the 1st level.
        if (currentScene == "LiamsWackyWonderland")
        {
            // If there is a format, use the format. Otherwise, don't use a format for PB text.
            pBText.text = hasFormat ? $"PB: {PlayerPrefs.GetFloat("PB", timerLimit).ToString(timeFormats[format])}" : $"PB: {PlayerPrefs.GetFloat("PB", timerLimit)}";
        }
        // If in the second scene, set the PB text to the PB for the 2nd level.
        else if (currentScene == "LiamsHighlyPsychoticJoint")
        {
            // If there is a format, use the format. Otherwise, don't use a format for PB text.
            pBText.text = hasFormat ? $"PB: {PlayerPrefs.GetFloat("PB2", timerLimit).ToString(timeFormats[format])}" : $"PB: {PlayerPrefs.GetFloat("PB2", timerLimit)}";
        }
    }    

    // Function to resume the game.
    public void Resume()
    {
        // Lock the cursor, make it not visible, turn off the pause menu UI, set time back to normal, and set gameIsPaused to false as game is no longer paused.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        gameIsPaused = false;
    }

    // Function to pause the game.
    void Pause()
    {
        // Unlock the cursor so player can click pause menu buttons, turn on the pause menu UI, stop time, and set gameIsPaused to true as game is paused.
        Cursor.lockState = CursorLockMode.None;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    // Function to load the menu scene.
    public void LoadMenu()
    {
        // Set time back to normal and load main menu scene.
        Time.timeScale = 1.0f;
        //ChangeScene("MainMenu");
        Debug.Log("Loading menu...");
    }

    // Function to quit the game. 
    public void QuitGame()
    {
        // Quit the application (only works in builds).
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}