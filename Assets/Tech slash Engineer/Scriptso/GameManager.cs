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
    [SerializeField] private GameObject pauseScreenOneUI;
    [SerializeField] private GameObject settingsMenuUI;

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

    // Enum for the different formats of the timer, such as 1 second or 1.1 seconds or 1.11 seconds or 1.111 seconds.
    public enum TimerFormats
    {
        None,
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

    // Leaderboard things.
    public Transform entryContainer; // Container that stores entries.
    public Transform entryTemplate; // Template that is used to determine how entries display.
    public GameObject leaderboard;

    private float templateHeight = 37f; // Y distance between entries.
    private List<Transform> highscoreEntryTransformList; // List of entry locations.

    // Declared here as it fixes null reference error, but list entries do not persist through scene change, so testing other things.
    //public List<HighscoreEntry> highscoreEntryList;

    // Comment
    void Awake()
    {
        // Add entries to the dictionary for the timer format.
        timeFormats.Add(TimerFormats.None, "0.000000");
        timeFormats.Add(TimerFormats.Whole, "0");
        timeFormats.Add(TimerFormats.TenthDecimal, "0.0");
        timeFormats.Add(TimerFormats.HundrethsDecimal, "0.00");
        timeFormats.Add(TimerFormats.ThousandthsDecimal, "0.000");

        // Turn off the template of what entries will look like as it's not meant to be seen as an actual entry.
        entryTemplate.gameObject.SetActive(false);

        // Create a string that stores the player prefs PBTimes string. If no string, make an empty JSON object.
        string jsonString = PlayerPrefs.GetString("PBTimes", "{\"highscoreEntryList\":[]}");
        
        // Set highscores variable equal to JSON file found at jsonString indicated above.
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);

        // If highscores and the highscoreEntryList exist, do things to display entries.
        if (highscores != null && highscores.highscoreEntryList != null)
        {
            // Sort highscore entries by lowest time on top.
            for (int i = 0; i < highscores.highscoreEntryList.Count; i++)
            {
                for (int j = i + 1; j < highscores.highscoreEntryList.Count; j++)
                {
                    if (highscores.highscoreEntryList[j].time < highscores.highscoreEntryList[i].time)
                    {
                        // Swap by storing i on tmp variable, then setting i equal to j, then setting j to tmp variable.
                        // For example, if i equaled 5.6, tmp = 5.6. If j equaled 8.7, i now equals 8.7. Finally, since tmp = 5.6, j is now 5.6.
                        // This leaves you with i = 8.7 and j = 5.6, the opposite of what it was before.
                        HighscoreEntry tmp = highscores.highscoreEntryList[i];
                        highscores.highscoreEntryList[i] = highscores.highscoreEntryList[j];
                        highscores.highscoreEntryList[j] = tmp;
                    }
                }
            }

            highscoreEntryTransformList = new List<Transform>(); // Set value to list that stores entry transforms.

            // For each entry in the highscoreEntryList, create an entry on the leaderboard based on the parameters put in the function below.
            foreach (HighscoreEntry highscoreEntry in highscores.highscoreEntryList)
            {
                CreateHighscoreEntryTransform(highscoreEntry, entryContainer, highscoreEntryTransformList);
            }
        }
        // If there is an issue with no highscores data, display this message.
        else
        {
            Debug.LogWarning("No highscores found.");
        }
    }
    
    // Function used to determine the location of a highscore entry on the leaderboard.
    void CreateHighscoreEntryTransform(HighscoreEntry highscoreEntry, Transform container, List<Transform> transformList)
    {
        // Create an entry on the leaderboard utilizing the template, which is a child of the container.
        Transform entryTransform = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>(); // Get the rect transform of newly created entry.
        
        // Adjust the Y position of the entry based on the template height variable and number in the list.
        // For example, if template height is 40, entry 3 will be 40 units below entry 3, and 120 units from the top.
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        
        // Once adjustments are made to the positioning of the entry, make it active.
        entryTransform.gameObject.SetActive(true);

        // Rank integer that is the count of the list + 1 as the current entry has not been added to the list yet.
        int rank = transformList.Count + 1;
        string rankString = rank.ToString(); // Convert int to a string to display on leaderboard.

/*        // Rank, but with suffixes like 1st, 2nd, 3rd, 4th, 5th, and so on.
        switch (rank)
        {
            default:
                rankString = rank + "TH";
                break;
            case 1:
                rankString = "1ST";
                break;
            case 2:
                rankString = "2ND";
                break;
            case 3:
                rankString = "3RD";
                break;
        }*/

        // Find the position text and set its text element to the rank found above.
        entryTransform.Find("PositionTextEntry").GetComponent<TextMeshProUGUI>().text = rankString;

        // Find the time text in the entry template and set it to the current time on the timer utilizing the timer format chosen.
        // (highscoreEntry.time is equal to time parameter when adding a new highscore entry, which is set to the currentTime on the timer when a new highscore entry is created.)
        entryTransform.Find("TimeTextEntry").GetComponent<TextMeshProUGUI>().text = hasFormat ? $"{highscoreEntry.time.ToString(timeFormats[format])}" : $"{highscoreEntry.time}";

        string name = highscoreEntry.name; // String for player name

        // Find the name text in the template and set it to the name of the player.
        //** Player will be able to set their name later on, not currently implemented. **
        entryTransform.Find("NameTextEntry").GetComponent<TextMeshProUGUI>().text = name;

        // Set the parent object of the newly created highscore entry transform to the container that stores all the entries.
        entryTransform.SetParent(entryContainer.transform);

        // Set background of entry visible based on whether current rank is odd or even.
        // If rank is 1, 1 goes into 2 zero times with 1 left over. 1 equals 1 is a true statement, so the backround is set to active for only odd rank entries.
        entryTransform.Find("Background").gameObject.SetActive(rank % 2 == 1);

        // If the rank of the entry is 1, make its color green so it stands out from the rest.
        if (rank == 1)
        {
            entryTransform.Find("PositionTextEntry").GetComponent<TextMeshProUGUI>().color = Color.green;
            entryTransform.Find("TimeTextEntry").GetComponent<TextMeshProUGUI>().color = Color.green;
            entryTransform.Find("NameTextEntry").GetComponent<TextMeshProUGUI>().color = Color.green;
        }

        // Add the newly created transform of the highscore entry to the transform list.
        transformList.Add(entryTransform);
    }

    // Function that adds a new highscore entry to the list.
    private void AddHighscoreEntry(float time, string name)
    {
        // Load saved highscores by first getting the data from the player prefs string, then setting a variable to the JSON data we want to access.
        string jsonString = PlayerPrefs.GetString("PBTimes");
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);

        // If highscores is null, create a new instance of it to prevent null reference exception error.
        if (highscores == null)
        {
            highscores = new Highscores();
        }
        
        // If the highscores list is null, intialize the list to prevent null reference exception error.
        if(highscores.highscoreEntryList == null)
        {
            highscores.highscoreEntryList = new List<HighscoreEntry>();
        }

        // Create a highscore entry by setting the time and name of the highscore entry to the value inputted when this function was called.
        HighscoreEntry highscoreEntry = new HighscoreEntry { time = time, name = name };

        // Add the newly created entry to the highscores list.
        highscores.highscoreEntryList.Add(highscoreEntry);

        // Save the new entry by putting it into a JSON string and then storing that in the player prefs key.
        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString("PBTimes", json);
        PlayerPrefs.Save();
    }

    // Class where the the highscore entries list is created because a list cannot be directly converted to into JSON.
    private class Highscores
    {
        public List<HighscoreEntry> highscoreEntryList;
    }

    // This represents a single highscore entry and must be serializable for JSON to be able to access the object.
    [System.Serializable]
    private class HighscoreEntry
    {
        public float time;
        public string name;
    }

    // Function called from other scripts that adds an entry to the highscores list.
    public void AddLeaderboardEntry()
    {
        AddHighscoreEntry(currentTime, "Liam");
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

        // While equals is held down, each press of the T key adds 10 seconds to the timer.
        if (Input.GetKey(KeyCode.Equals) && Input.GetKeyDown(KeyCode.T))
        {
            timerLimit += 10;
        }
        // While minus is held down, each press of the T key subtracts 10 seconds from the timer.
        else if(Input.GetKey(KeyCode.Minus) && Input.GetKeyDown(KeyCode.T))
        {
            timerLimit -= 10;
        }

        // If the L key is pressed, do the opposite of what the leaderboard's current active state is. If not active, set to active. If active, set to not active.
        //** Leaderboard will become accessible through the menu later on.**
        if (Input.GetKeyDown(KeyCode.L))
        {
            leaderboard.SetActive(!leaderboard.activeSelf);
        }

        // Add an entry to the highscores list to test if the leaderboard is working correctly.
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            AddHighscoreEntry(currentTime, "Liam");
            Debug.Log("Entry added");
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
        // Lock the cursor, make it not visible, turn off the settings menu and pause menu UI, set time back to normal, set player active bool to true,
        // and set gameIsPaused to false as game is no longer paused.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        isPlayerActive = true;
        gameIsPaused = false;
    }

    // Function to pause the game.
    void Pause()
    {
        // Stop time, set player active bool to false, unlock player cursor so they can interact with buttons, turn on pause menu UI and its buttons,
        // and set gameIsPaused to true as game is now paused.
        Time.timeScale = 0f;
        isPlayerActive = false;
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
        Debug.Log("Loading menu...");
    }

    // Function to quit the game. 
    public void QuitGame()
    {
        // Quit the application (only works in builds).
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    // Function to turn on the settings menu.
    public void ActivateSettings()
    {
        pauseScreenOneUI.SetActive(false);
        settingsMenuUI.SetActive(true);
    }    
}