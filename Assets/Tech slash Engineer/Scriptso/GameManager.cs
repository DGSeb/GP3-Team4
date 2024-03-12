using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using System;
using UnityEngine.UI;

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

    // Bool that says whether the player is actively playing or not.
    // Used to stop certain actions from ocurring when the player isn't active.
    // For example, when paused or when time is stopped.
    public static bool isPlayerActive = true;

    //private Transform entryContainer;
    //private Transform entryTemplate;

    //private int numberOfEntries = 10;
    //private float templateHeight = 30f;
    //private List<Transform> highscoreEntryTransformList;

    // Comment
    void Awake()
    {
        // Add entries to the dictionary for the timer format.
        timeFormats.Add(TimerFormats.Whole, "0");
        timeFormats.Add(TimerFormats.TenthDecimal, "0.0");
        timeFormats.Add(TimerFormats.HundrethsDecimal, "0.00");
        timeFormats.Add(TimerFormats.ThousandthsDecimal, "0.000");


    //    entryContainer = GameObject.Find("HighscoreEntryContainer").GetComponent<Transform>();
    //    entryTemplate = GameObject.Find("HighscoreEntryTemplate").GetComponent<Transform>();

    //    entryTemplate.gameObject.SetActive(false);

    //    string jsonString = PlayerPrefs.GetString("PBTimes");
    //    Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);

    //    // Sort list by lowest time on top.
    //    for (int i = 0; i < highscores.highscoreEntryList.Count; i++)
    //    {
    //        for (int j = i + 1; j < highscores.highscoreEntryList.Count; j++)
    //        {
    //            if (highscores.highscoreEntryList[j].time < highscores.highscoreEntryList[i].time)
    //            {
    //                // Swap
    //                HighscoreEntry tmp = highscores.highscoreEntryList[i];
    //                highscores.highscoreEntryList[i] = highscores.highscoreEntryList[j];
    //                highscores.highscoreEntryList[j] = tmp;
    //            }
    //        }
    //    }

    //    highscoreEntryTransformList = new List<Transform>();
    //    foreach (HighscoreEntry highscoreEntry in highscores.highscoreEntryList)
    //    {
    //        CreateHighscoreEntryTransform(highscoreEntry, entryContainer, highscoreEntryTransformList);
    //    }
    //}

    //void CreateHighscoreEntryTransform(HighscoreEntry highscoreEntry, Transform container, List<Transform> transformList)
    //{
    //    Transform entryTransform = Instantiate(entryTemplate, container);
    //    RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
    //    entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
    //    entryTransform.gameObject.SetActive(true);

    //    int rank = transformList.Count + 1;
    //    string rankString = rank.ToString();
    //    entryTransform.Find("PositionTextEntry").GetComponent<TextMeshProUGUI>().text = rankString;

    //    entryTransform.Find("TimeTextEntry").GetComponent<TextMeshProUGUI>().text = highscoreEntry.time.ToString();
    //        //hasFormat ? $"{PlayerPrefs.GetFloat("PB", timerLimit).ToString(timeFormats[format])}" : $"{PlayerPrefs.GetFloat("PB", timerLimit)}";

    //    string name = highscoreEntry.name;
    //    entryTransform.Find("NameTextEntry").GetComponent<TextMeshProUGUI>().text = name;

    //    entryTransform.SetParent(entryContainer.transform);

    //    // Set background visible odds and evens.
    //    entryTransform.Find("Background").gameObject.SetActive(rank % 2 == 1);

    //    if (rank == 1)
    //    {
    //        // Highlight first entry
    //        entryTransform.Find("PositionTextEntry").GetComponent<TextMeshProUGUI>().color = Color.green;
    //        entryTransform.Find("TimeTextEntry").GetComponent<TextMeshProUGUI>().color = Color.green;
    //        entryTransform.Find("NameTextEntry").GetComponent<TextMeshProUGUI>().color = Color.green;
    //    }

    //    transformList.Add(entryTransform);
    //}

    //private void AddHighscoreEntry(float time, string name)
    //{
    //    // Create highscore entry
    //    HighscoreEntry highscoreEntry = new HighscoreEntry { time = time, name = name };

    //    // Load svaed highscores
    //    string jsonString = PlayerPrefs.GetString("PBTimes");
    //    Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);

    //    // Add new entry to Highscores.
    //    highscores.highscoreEntryList.Add(highscoreEntry);

    //    // Save updated Highscores.
    //    string json = JsonUtility.ToJson(highscores);
    //    PlayerPrefs.SetString("PBTime", json);
    //    PlayerPrefs.Save();
    //}

    //private class Highscores
    //{
    //    public List<HighscoreEntry> highscoreEntryList;
    //}

    //// This represents a single highscore entry.
    //[System.Serializable]
    //private class HighscoreEntry
    //{
    //    public float time;
    //    public string name;
    }

    void Start()
    {
        // Find the name of the active scene and assign it to the currentScene variable.
        // Make sure this occurs before running UpdatePBText so it knows which scene it is in.
        currentScene = SceneManager.GetActiveScene().name;

        // Run the UpdatePBText that updates the text displaying the player's PB.
        UpdatePBText();

        // Make sure the pause menu is off.
        pauseMenuUI.SetActive(false);

        // At start, ensure the game is not paused bool is false as the game is not paused.
        gameIsPaused = false;

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

        // Run the check input function to see what is being pressed.
        CheckInput();
        
    }

    // Function that checks for different inputs.
    void CheckInput()
    {
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

        // If the R key is pressed, restart the player in the scene they are currently in.
        // This is very helpful if you want to restart a run or if you fall off into the abyss.
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }

        // If all these keys are pressed, delete the things saved to player prefs. Don't want this to happen unintentionally.
        if (Input.GetKey(KeyCode.RightAlt) && Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.RightShift)
            && Input.GetKey(KeyCode.B) && Input.GetKeyDown(KeyCode.P))
        {
            PlayerPrefs.DeleteAll();
        }

        if (Input.GetKey(KeyCode.Equals) && Input.GetKeyDown(KeyCode.T))
        {
            timerLimit += 10;
        }
        else if(Input.GetKey(KeyCode.Minus) && Input.GetKeyDown(KeyCode.T))
        {
            timerLimit -= 10;
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

    // Function to reload the scene the player is currently in / respawn them at the start.
    public void ReloadScene()
    {
        ChangeScene(currentScene);
    }

    // When time limit is reached, run coroutine that resets game.
    IEnumerator TimeLimitReached()
    {
        // Set bool saying player isn't active to false, turn on text telling player the time limit is reached, stop time, wait about a second,
        // change scene to the one the player is currently in, resume time, set player active bool back to true, and update text displaying player's PB.
        isPlayerActive = false;
        timeLimitReachedText.SetActive(true);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1.2f);
        ReloadScene();
        Time.timeScale = 1.0f;
        isPlayerActive = true;
        UpdatePBText();
    }

    // Function to check the fastest time the player beat the level (PB (Personal Best)).
    // This function only runs when the player reaches the exit.
    public void CheckPB()
    {
        /*
            If you want to delete a PB, which is stored in PlayerPrefs, there are two ways. The first can be done through script.
            To delete through script, type PlayerPrefs.DeleteAll() to delete all PlayerPrefs. Here is a Unity doc on it: https://docs.unity3d.com/ScriptReference/PlayerPrefs.DeleteAll.html.
            Obviously, this can only be done through script so it's good if you want to test from ground zero, but won't work if you have a build and want to reset PB.
            You can set a function with very specific paramters to do it and to make sure it is very intentionally done.

            The second option requires doing stuff on the computer. 
            Click windows key, type regedit and open registry editor. Within in here go to file path HKEY_CURRENT_USER\SOFTWARE\Unity\UnityEditor\CompanyName\ProjectName
            Then choose the player pref string names in there and delete the file.
        */

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
        // If currently in the tutorial scene and the current time is lower than the player's PB, the player has a new PB.
        // Set the player's PB to the currentTime and update the PB text.
        else if (currentScene == "AveryScene" && (currentTime < PlayerPrefs.GetFloat("PBTutorial", timerLimit)))
        {
            PlayerPrefs.SetFloat("PBTutorial", currentTime);
            UpdatePBText();
        }
        // If currently in the small/old tutorial scene and the current time is lower than the player's PB, the player has a new PB.
        // Set the player's PB to the currentTime and update the PB text.
        else if (currentScene == "Tutorial (Small Version)" && (currentTime < PlayerPrefs.GetFloat("PBSmallTutorial", timerLimit)))
        {
            PlayerPrefs.SetFloat("PBSmallTutorial", currentTime);
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
        // If in the tutorial scene, set the PB text to the PB for the tutorial level.
        else if (currentScene == "AveryScene")
        {
            // If there is a format, use the format. Otherwise, don't use a format for PB text.
            pBText.text = hasFormat ? $"PB: {PlayerPrefs.GetFloat("PBTutorial", timerLimit).ToString(timeFormats[format])}" : $"PB: {PlayerPrefs.GetFloat("PBTutorial", timerLimit)}";
        }
        // If in the small/old tutorial scene, set the PB text to the PB for the small/old tutorial level.
        else if (currentScene == "Tutorial (Small Version)")
        {
            // If there is a format, use the format. Otherwise, don't use a format for PB text.
            pBText.text = hasFormat ? $"PB: {PlayerPrefs.GetFloat("PBSmallTutorial", timerLimit).ToString(timeFormats[format])}" : $"PB: {PlayerPrefs.GetFloat("PBSmallTutorial", timerLimit)}";
        }
    }    

    // Function to resume the game.
    public void Resume()
    {
        // Lock the cursor, make it not visible, turn off the pause menu UI, set time back to normal, set player active bool to true,
        // and set gameIsPaused to false as game is no longer paused.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        isPlayerActive = true;
        gameIsPaused = false;
    }

    // Function to pause the game.
    void Pause()
    {
        // Stop time, set player active bool to false, unlock player cursor so they can interact with buttons, turn on pause menu UI,
        // and set gameIsPaused to true as game is now paused.
        Time.timeScale = 0f;
        isPlayerActive = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenuUI.SetActive(true);
        gameIsPaused = true;
    }

    // Function to load the menu scene.
    public void LoadMenu()
    {
        // Set time back to normal and load main menu scene.
        Time.timeScale = 1.0f;
        ChangeScene("MainMenu");
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