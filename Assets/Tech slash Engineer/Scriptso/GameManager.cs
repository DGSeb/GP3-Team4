using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

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

    void Start()
    {
        // Add entries to the dictionary for the timer format.
        timeFormats.Add(TimerFormats.Whole, "0");
        timeFormats.Add(TimerFormats.TenthDecimal, "0.0");
        timeFormats.Add(TimerFormats.HundrethsDecimal, "0.00");
        timeFormats.Add(TimerFormats.ThousandthsDecimal, "0.000");

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

        SetTimerText();

        // Pause menu check
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
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
        // Turn on text telling player the time limit is reached, stop time, wait about a second, reload the scene, and resume time.
        timeLimitReachedText.SetActive(true);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1.2f);
        ChangeScene("LiamsWackyWonderland");
        Time.timeScale = 1.0f;
        UpdatePBText();
    }

    // Function to check the best time the player achieved.
    public void CheckPB()
    {
        if (currentTime < PlayerPrefs.GetFloat("PB", timerLimit))
        {
            PlayerPrefs.SetFloat("PB", currentTime);
            UpdatePBText();
        }
    }

    void UpdatePBText()
    {
        pBText.text = hasFormat ? $"PB: {PlayerPrefs.GetFloat("PB", timerLimit).ToString(timeFormats[format])}" : $"PB: {PlayerPrefs.GetFloat("PB", timerLimit)}";
    }    

    // Pause menu things
    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        gameIsPaused = false;
    }

    void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1.0f;
        Debug.Log("Loading menu...");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}