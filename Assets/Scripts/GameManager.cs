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
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    // Components that display on UI.
    [Header("UI Components")]
    public TextMeshProUGUI timerText;
    public GameObject timeLimitReachedText;
    public TextMeshProUGUI pBText;

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

    // String to say which scene the player is currently in.
    [HideInInspector] public string currentScene;

    // Bool that says whether the player is actively playing or not.
    // Used to stop certain actions from ocurring when the player isn't active.
    // For example, when paused or when time is stopped.
    public static bool isPlayerActive = true;

    [Header("Enemy UI")]
    public TextMeshProUGUI enemiesRemainingUI; // Reference to enemies remaining text element.
    public TextMeshProUGUI enemiesEliminatedUI; // Reference to enemies eliminated text element.

    // Level enemy count variables
    private int levelOneEnemyCount = 25;
    private int levelTwoEnemyCount = 28;
    private int levelThreeEnemyCount = 30;

    [HideInInspector] public int enemiesRemaining;
    [HideInInspector] public int enemiesEliminated = 0;

    // Number of enemies needed to be eliminated to interact with the end object and win.
    private int enemiesToWinLevel;
    private int enemiesToWinTutorial = 12;
    private int enemiesToWinLevel1 = 15;
    private int enemiesToWinLevel2 = 15;
    private int enemiesToWinLevel3 = 22;


    // Bool that says whether or not enough enemies have been eliminated for the player to be able to exit the level.
    [HideInInspector] public bool enoughEnemiesEliminated = false;

    // Amount of time taken off the clock per elimination when the number of enemies needed to win the level has been achieved.
    private float timeSaved = 1.8f;

    private float colorFlashTime = 0.2f;

    // Reference to the crosshair object that is a part of the player UI.
    private RectTransform crosshair;

    [Header("Audio")]
    [SerializeField] private AudioMixer master;
    [SerializeField] private AudioSource enemyDeathSound;
    [SerializeField] private AudioSource playerLoseSound;

    [Header("Exit")]
    // Variables related to the changing of exit material color based on whether or not the player has enough elims.
    public Material exitMaterialGreen;
    public Material exitMaterialRed;
    private Material exitMaterial;
    public GameObject exit;

    [Header("Leaderboard")]
    public GameObject leaderboard;
    private Leaderboard leaderboardScript;

    private static int frameRate = 30;

    void Awake()
    {
        leaderboardScript = leaderboard.GetComponent<Leaderboard>();

        crosshair = GameObject.Find("Crosshair").GetComponent<RectTransform>(); // Set reference to crosshair.

        // Create a variable that will store the size of the crosshair that is obtained from the player prefs key.
        float crosshairSize = PlayerPrefs.GetFloat("CrosshairSize", 0.15f);

        // Set the x and y scale of the crosshair to the player prefs saved data.
        crosshair.transform.localScale = new Vector3(crosshairSize, crosshairSize, crosshair.transform.localScale.z);

        // Add entries to the dictionary for the timer format.
        timeFormats.Add(TimerFormats.None, "0.000000");
        timeFormats.Add(TimerFormats.Whole, "0");
        timeFormats.Add(TimerFormats.TenthDecimal, "0.0");
        timeFormats.Add(TimerFormats.HundrethsDecimal, "0.00");
        timeFormats.Add(TimerFormats.ThousandthsDecimal, "0.000");

        // Find the name of the active scene and assign it to the currentScene variable.
        currentScene = SceneManager.GetActiveScene().name;

        // Switch statment that sets the playerprefs string to store a leaderboard entry in based on which scene the player is in.
        switch (currentScene)
        {
            case "Tutorial":
                Leaderboard.playerPrefsString = "PBLeaderboardTutorial";
                break;
            case "Level1":
                Leaderboard.playerPrefsString = "PBLeaderboardLevel1";
                break;

            case "Level2":
                Leaderboard.playerPrefsString = "PBLeaderboardLevel2";
                break;

            case "Level3":
                Leaderboard.playerPrefsString = "PBLeaderboardLevel3";
                break;
        }
    }

    

    // Function called from other scripts that adds an entry to the highscores list.
    public void AddLeaderboardEntry()
    {
        leaderboardScript.AddHighscoreEntry(currentTime, "Liam");
    }

    void Start()
    {
        // Set audio levels for the game based on the player prefs audio settings.
        master.SetFloat("MasterVolume", Mathf.Log10(PlayerPrefs.GetFloat("MasterVolume", 0.5f)) * 20);
        master.SetFloat("MusicVolume", Mathf.Log10(PlayerPrefs.GetFloat("MusicVolume", 0.5f)) * 20);
        master.SetFloat("SFXVolume", Mathf.Log10(PlayerPrefs.GetFloat("SFXVolume", 0.5f)) * 20);

        // If in the level scenes, set the exit material so that it is red when not enough eliminations, and green when enough eliminations.
        if (currentScene == "Level1" || currentScene == "Level2" || currentScene == "Level3")
        {
            exitMaterial = exitMaterialRed;
            SwapExitMaterial();
        }

        // If the player is not in the tutorial, make sure they can double jump and dash.
        if (currentScene != "Tutorial")
        {
            TutorialManager.canDoubleJump = true;
            TutorialManager.canDash = true;
        }

        // Set the scene that the result screen play again button will load.
        ResultScreen.lastScene = currentScene;

        // Run the UpdatePBText that updates the text displaying the player's PB.
        UpdatePBText();

        // At start, ensure the game is not paused bool is false as the game is not paused.
        SettingsMenu.gameIsPaused = false;

        // Set the start value of the enemy count values. Enemies remaining is the enemy count of the level.
        // Enemies eliminated is 0 as no eliminations have occurred yet.
        EnemyCountVariablesStartValues();
    }

    void Update()
    {
        if (currentTime > 0.2f && !isPlayerActive && !SettingsMenu.gameIsPaused)
        {
            isPlayerActive = true;
        }

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
        // Cat laughing at you.
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Application.OpenURL("https://www.youtube.com/watch?v=L8XbI9aJOXk");
        }

        // Plays the Kalimba song from Windows 7.
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            Application.OpenURL("https://www.youtube.com/watch?v=tCO4i2t-Aso");
        }

        // Half of what your monitor can do.
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            QualitySettings.vSyncCount = 2;
        }
        // Display number of frames that your monitor can display.
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            // Quality settings: https://docs.unity3d.com/ScriptReference/QualitySettings-vSyncCount.html
            QualitySettings.vSyncCount = 1;
        }
        // Uncapped frames.
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
        }
        
        // Set the frame rate manually starting from 30 and subtract 5 on each press.
        else if (Input.GetKey(KeyCode.Minus) && Input.GetKeyDown(KeyCode.F))
        {
            QualitySettings.vSyncCount = 0;
            frameRate -= 5;
            Application.targetFrameRate = frameRate;
        }
        // Set frame rate manually starting from 30 or different value if changed and add 5 on each press.
        else if (Input.GetKey(KeyCode.Equals) && Input.GetKeyDown(KeyCode.F))
        {
            QualitySettings.vSyncCount = 0;
            frameRate += 5;
            Application.targetFrameRate = frameRate;
        }  

        // If the R key is pressed, restart the player in the scene they are currently in.
        // This is very helpful if you want to restart a run or if you fall off into the abyss.
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton6))
        {
            ReloadScene();
        }

        // If all these keys are pressed, delete the things saved to player prefs. Don't want this to happen unintentionally.
        if (Input.GetKey(KeyCode.RightAlt) && Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.RightShift))
        {
            // If B and P are also pressed, delete all the player prefs keys.
            if (Input.GetKey(KeyCode.B) && Input.GetKeyDown(KeyCode.P))
            {
                PlayerPrefs.DeleteAll();
                Debug.Log("Deleted all player prefs keys!");
            }
            // If only V is pressed, delete all the volume keys.
            else if (Input.GetKeyDown(KeyCode.V))
            {
                PlayerPrefs.DeleteKey("MasterVolume");
                PlayerPrefs.DeleteKey("MusicVolume");
                PlayerPrefs.DeleteKey("SFXVolume");
            }
            
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
            leaderboardScript.LoadLeaderboard(Leaderboard.playerPrefsString);
            leaderboard.SetActive(!leaderboard.activeSelf);
        }

        // Add an entry to the highscores list to test if the leaderboard is working correctly.
        if (Input.GetKey(KeyCode.Backslash) && Input.GetKeyDown(KeyCode.Slash))
        {
            leaderboardScript.AddHighscoreEntry(currentTime, "Liam");
            Debug.Log("Entry added");
        }

        //CheckControllerInput();
    }

    // Function to set the text of the timer.
    void SetTimerText()
    {        
        // The bool in this case is hasFormat and the ? indicates checking for the value of the bool.
        // If there is a format, apply it. Otherwise, do not apply a format.
        timerText.text = hasFormat ? currentTime.ToString(timeFormats[format]) : currentTime.ToString();
    }

    // Function called when player is hit by an enemy attack that adds time to the clock.
    public void ChangeTimer(float timeChange)
    {
        currentTime += timeChange;
        StopCoroutine(SavedTime());
        StartCoroutine(TakeTimerDamage());
    }

    // Function to change the scene based on the name inputted when function is called.
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Function to reload the scene the player is currently in / respawn them at the start.
    public void ReloadScene()
    {
        isPlayerActive = false;
        ChangeScene(currentScene);
    }

    // When time limit is reached, run coroutine that resets game.
    IEnumerator TimeLimitReached()
    {
        // Set bool saying player isn't active to false, turn on text telling player the time limit is reached, stop time, and wait about a second.
        // The player has lost, so set that bool to true and make sure won is false. Resume time and changing scenes to the results screen.

        // if it isn't already playing, play it
        if (!playerLoseSound.isPlaying)
        {
            playerLoseSound.Play();
        }

        isPlayerActive = false;
        timeLimitReachedText.SetActive(true);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1.2f);
        ResultScreen.lost = true;
        ResultScreen.won = false;
        Time.timeScale = 1.0f;
        ChangeScene("ResultsScreen");
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
        else if (currentScene == "Tutorial" && (currentTime < PlayerPrefs.GetFloat("PBTutorial", timerLimit)))
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
        // If currently in level 1 and the current time is lower than the player's PB, the player has a new PB.
        // Set the player's PB to the currentTime and update the PB text.
        else if (currentScene == "Level1" && (currentTime < PlayerPrefs.GetFloat("PBLevel1", timerLimit)))
        {
            PlayerPrefs.SetFloat("PBLevel1", currentTime);
            UpdatePBText();
        }
        // If currently in level 1 and the current time is lower than the player's PB, the player has a new PB.
        // Set the player's PB to the currentTime and update the PB text.
        else if (currentScene == "Level2" && (currentTime < PlayerPrefs.GetFloat("PBLevel2", timerLimit)))
        {
            PlayerPrefs.SetFloat("PBLevel2", currentTime);
            UpdatePBText();
        }
        // If currently in level 2 and the current time is lower than the player's PB, the player has a new PB.
        // Set the player's PB to the currentTime and update the PB text.
        else if (currentScene == "Level2" && (currentTime < PlayerPrefs.GetFloat("PBLevel2", timerLimit)))
        {
            PlayerPrefs.SetFloat("PBLevel2", currentTime);
            UpdatePBText();
        }
        // If currently in level 3 and the current time is lower than the player's PB, the player has a new PB.
        // Set the player's PB to the currentTime and update the PB text.
        else if (currentScene == "Level3" && (currentTime < PlayerPrefs.GetFloat("PBLevel3", timerLimit)))
        {
            PlayerPrefs.SetFloat("PBLevel3", currentTime);
            UpdatePBText();
        }
    }

    // Update text displaying the player's PB.
    void UpdatePBText()
    {
        switch(currentScene)
        {
            // If in the first scene, set the PB text to the PB for the 1st random level.
            case "LiamsWackyWonderland":
                // If there is a format, use the format. Otherwise, don't use a format for PB text.
                pBText.text = hasFormat ? $"PB: {PlayerPrefs.GetFloat("PB", timerLimit).ToString(timeFormats[format])}" : $"PB: {PlayerPrefs.GetFloat("PB", timerLimit)}";
                break;

            // If in the second scene, set the PB text to the PB for the 2nd level.
            case "LiamsHighlyPsychoticJoint":
                // If there is a format, use the format. Otherwise, don't use a format for PB text.
                pBText.text = hasFormat ? $"PB: {PlayerPrefs.GetFloat("PB2", timerLimit).ToString(timeFormats[format])}" : $"PB: {PlayerPrefs.GetFloat("PB2", timerLimit)}";
                break;

            // If in the tutorial scene, set the PB text to the PB for the tutorial level.
            case "Tutorial":
                // If there is a format, use the format. Otherwise, don't use a format for PB text.
                pBText.text = hasFormat ? $"PB: {PlayerPrefs.GetFloat("PBTutorial", timerLimit).ToString(timeFormats[format])}" : $"PB: {PlayerPrefs.GetFloat("PBTutorial", timerLimit)}";
                break;

            // If in the small/old tutorial scene, set the PB text to the PB for the small/old tutorial level.
            case "Tutorial (Small Version)":
                // If there is a format, use the format. Otherwise, don't use a format for PB text.
                pBText.text = hasFormat ? $"PB: {PlayerPrefs.GetFloat("PBSmallTutorial", timerLimit).ToString(timeFormats[format])}" : $"PB: {PlayerPrefs.GetFloat("PBSmallTutorial", timerLimit)}";
                break;

            // If in level 1, set the PB text to the PB for level 1.
            case "Level1":
                // If there is a format, use the format. Otherwise, don't use a format for PB text.
                pBText.text = hasFormat ? $"PB: {PlayerPrefs.GetFloat("PBLevel1", timerLimit).ToString(timeFormats[format])}" : $"PB: {PlayerPrefs.GetFloat("PBLevel1", timerLimit)}";

                // Set the text for what the player's PB will be displayed as on the results screen.
                ResultScreen.pBTime = hasFormat ? PlayerPrefs.GetFloat("PBLevel1", timerLimit).ToString(timeFormats[format]) : PlayerPrefs.GetFloat("PBLevel1", timerLimit).ToString();
                break;

            // If in level 2, set the PB text to the PB for level 2.
            case "level2":
                // If there is a format, use the format. Otherwise, don't use a format for PB text.
                pBText.text = hasFormat ? $"PB: {PlayerPrefs.GetFloat("PBLevel2", timerLimit).ToString(timeFormats[format])}" : $"PB: {PlayerPrefs.GetFloat("PBLevel2", timerLimit)}";

                // Set the text for what the player's PB will be displayed as on the results screen.
                ResultScreen.pBTime = hasFormat ? PlayerPrefs.GetFloat("PBLevel2", timerLimit).ToString(timeFormats[format]) : PlayerPrefs.GetFloat("PBLevel2", timerLimit).ToString();
                break;

            // If in level 3, set the PB text to the PB for level 3.
            case "Level3":
                // If there is a format, use the format. Otherwise, don't use a format for PB text.
                pBText.text = hasFormat ? $"PB: {PlayerPrefs.GetFloat("PBLevel3", timerLimit).ToString(timeFormats[format])}" : $"PB: {PlayerPrefs.GetFloat("PBLevel3", timerLimit)}";

                // Set the text for what the player's PB will be displayed as on the results screen.
                ResultScreen.pBTime = hasFormat ? PlayerPrefs.GetFloat("PBLevel3", timerLimit).ToString(timeFormats[format]) : PlayerPrefs.GetFloat("PBLevel3", timerLimit).ToString();
                break;
        }
    }  
    
    // Function that sets the run time on the results screen
    public void SetRunTime()
    {
        ResultScreen.runTime = hasFormat ? currentTime.ToString(timeFormats[format]) : currentTime.ToString();
    }
    
    // Set the UI for how many enemies are left and have been eliminated.
    void SetEnemyUI()
    {
        enemiesRemainingUI.text = "Enemies Remaining: " + enemiesRemaining;
        enemiesEliminatedUI.text = "Enemies Eliminated: " + enemiesEliminated + " / " + enemiesToWinLevel;
    }

    // Function called in the EnemyBehavior script when the enemy runs out of health.
    // Decreases the enemies remaining by 1, increases the enemies eliminated by 1, and updates the UI.
    public void ChangeEnemyCountVariables()
    {
        enemiesRemaining--;
        enemiesEliminated++;
        SetEnemyUI();

        // If the number of required eliminations has been reached, subtract time from the timer, so the player receives a benefit for eliminating more enemies.
        if (enemiesEliminated > enemiesToWinLevel)
        {
            currentTime -= timeSaved;
            StopCoroutine(TakeTimerDamage());
            StartCoroutine(SavedTime());
        }

        // If the number of required eliminations has been reached, set the bool that indicates that to true. 
        // This is a separate if statement as this only needs to occur once, rather than every time a kill is obtained when the number of required eliminations has already been reached.
        if (enemiesEliminated == enemiesToWinLevel)
        {
            if (currentScene == "Level1" || currentScene == "Level2" || currentScene == "Level3")
            {
                exitMaterial = exitMaterialGreen;
                SwapExitMaterial();
            }

            enoughEnemiesEliminated = true;
        }
    }

    // Sets the beginning values of the enemy count variables.
    void EnemyCountVariablesStartValues()
    {
        // As levels may differ in the number of enemies, set the beginning number of remaining enemies to the number of enemies based on the level the player is in.
        // Also set the number of enemies needed to win the level as this might change for each level.
        switch(currentScene)
        {
            case "Tutorial":
                enemiesToWinLevel = enemiesToWinTutorial;
                break;

            case "Level1":
                enemiesRemaining = levelOneEnemyCount;
                enemiesToWinLevel = enemiesToWinLevel1;
                break;

            case "Level2":
                enemiesRemaining = levelTwoEnemyCount;
                enemiesToWinLevel = enemiesToWinLevel2;
                break;

            case "Level3":
                enemiesRemaining = levelThreeEnemyCount;
                enemiesToWinLevel = enemiesToWinLevel3;
                break;        
        }

        // The below items always occur no matter the level.

        // Player hasn't made any elims yet, so make sure it's at 0.
        enemiesEliminated = 0;

        // As no elims, player hasn't eliminated enough enemies to win, so make sure this is false.
        enoughEnemiesEliminated = false;

        // Set the UI so it displays the number of enemies in the level and 0 for the number of enemies eliminated.
        SetEnemyUI();
    }

    // Function to play the enemy's death sound as if it's played in the enemy scipt, the enemy object no longer exists, so the sound only plays for a split second.
    public void PlayEnemyDeathSound()
    {
        // If sound isn't playing, play it.
        if (!enemyDeathSound.isPlaying)
        {
            enemyDeathSound.Play();
        }
    }

    // Coroutine that flashes the timer red when the player gets shot by an enemy.
    IEnumerator TakeTimerDamage()
    {
        timerText.color = Color.red;
        yield return new WaitForSeconds(colorFlashTime);
        timerText.color = Color.white;
        yield return new WaitForSeconds(colorFlashTime);
        timerText.color = Color.red;
        yield return new WaitForSeconds(colorFlashTime);
        timerText.color = Color.white;
    }

    // Coroutine that flashes the timer green when the player takes time off the clock (positive action).
    IEnumerator SavedTime()
    {
        timerText.color = Color.green;
        yield return new WaitForSeconds(colorFlashTime);
        timerText.color = Color.white;
        yield return new WaitForSeconds(colorFlashTime);
        timerText.color = Color.green;
        yield return new WaitForSeconds(colorFlashTime);
        timerText.color = Color.white;
    }

    void SwapExitMaterial()
    {
        // Check if the object to swap and the new material are not null
        if (exit != null && exitMaterial != null)
        {
            // Get the renderer component of the object
            Renderer renderer = exit.GetComponent<Renderer>();

            // Check if the renderer component is not null
            if (renderer != null)
            {
                // Assign the new material to the object
                renderer.material = exitMaterial;
            }
            else
            {
                Debug.LogWarning("Renderer component not found on the object to swap.");
            }
        }
        else
        {
            Debug.LogWarning("Object to swap or new material is null.");
        }
    }

    // Function that checks which button on the controller is pressed.
    /*void CheckControllerInput()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            // A button
            Debug.Log("0 is A button");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            // B button
            Debug.Log("1 is B button");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            // X button
            Debug.Log("2 is X button");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            // Y button
            Debug.Log("3 is Y button");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            // Left bumper (LB)
            Debug.Log("4 is left bumper (LB)");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            // Right bumper (RB)
            Debug.Log("5 is right bumper (RB)");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton6))
        {
            // Select button (two squares)
            Debug.Log("6 is the select button (two squares)");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            // Pause button with the three horizontal parallel lines (hamburger button or start button).
            Debug.Log("7 is start button/pause button/hamburger button");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton8))
        {
            // Clicking left stick.
            Debug.Log("8 is click left stick");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton9))
        {
            // Clicking right stick.
            Debug.Log("9 is click right stick");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton10))
        {
            Debug.Log("10");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton11))
        {
            Debug.Log("11");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton12))
        {
            Debug.Log("12");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton13))
        {
            Debug.Log("13");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton14))
        {
            Debug.Log("14");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton15))
        {
            Debug.Log("15");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton16))
        {
            Debug.Log("16");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton17))
        {
            Debug.Log("17");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton18))
        {
            Debug.Log("18");
        }
        else if (Input.GetKeyDown(KeyCode.JoystickButton19))
        {
            Debug.Log("19");
        }
    }*/
}