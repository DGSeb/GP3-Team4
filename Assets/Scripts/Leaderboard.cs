using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    // Leaderboard things.
    public Transform entryContainer; // Container that stores entries.
    public Transform entryTemplate; // Template that is used to determine how entries display.
    public static string playerPrefsString; // String where leaderboard entries are based on the current scene.

    private float templateHeight = 37f; // Y distance between entries.
    private List<Transform> highscoreEntryTransformList; // List of entry locations.

    // Settings to control if there is a time limit.
    [Header("Limit Settings")]
    public bool hasLimit;

    // Settings to control the format of the timer.
    [Header("Format Settings")]
    public bool hasFormat;
    public TimerFormats format;
    [HideInInspector] public Dictionary<TimerFormats, string> timeFormats = new Dictionary<TimerFormats, string>();

    // Enum for the different formats of the timer, such as 1 second or 1.1 seconds or 1.11 seconds or 1.111 seconds.
    public enum TimerFormats
    {
        None,
        Whole,
        TenthDecimal,
        HundrethsDecimal,
        ThousandthsDecimal
    }

    // Function that load and populates the leaderboard with the entries stored in the player prefs string/json file.
    public void LoadLeaderboard(string playerPrefsStringy)
    {
        // Clear the dictionary so the addition of new keys does not cause any issues.
        timeFormats.Clear();

        // Add entries to the dictionary for the timer format.
        timeFormats.Add(TimerFormats.None, "0.000000");
        timeFormats.Add(TimerFormats.Whole, "0");
        timeFormats.Add(TimerFormats.TenthDecimal, "0.0");
        timeFormats.Add(TimerFormats.HundrethsDecimal, "0.00");
        timeFormats.Add(TimerFormats.ThousandthsDecimal, "0.000");

        // Set the static player prefs string variable to the string taken in when this function is called.
        playerPrefsString = playerPrefsStringy;

        // Clear the leaderboard for new entries
        ClearLeaderboard();

        // Turn off the template of what entries will look like as it's not meant to be seen as an actual entry.
        entryTemplate.gameObject.SetActive(false);

        // Create a string that stores the player prefs PBTimes string. If no string, make an empty JSON object.
        string jsonString = PlayerPrefs.GetString(playerPrefsString, "{\"highscoreEntryList\":[]}");

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
    public void AddHighscoreEntry(float time, string name)
    {
        // Load saved highscores by first getting the data from the player prefs string, then setting a variable to the JSON data we want to access.
        string jsonString = PlayerPrefs.GetString(playerPrefsString);
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);

        // If highscores is null, create a new instance of it to prevent null reference exception error.
        if (highscores == null)
        {
            highscores = new Highscores();
        }

        // If the highscores list is null, intialize the list to prevent null reference exception error.
        if (highscores.highscoreEntryList == null)
        {
            highscores.highscoreEntryList = new List<HighscoreEntry>();
        }

        // Create a highscore entry by setting the time and name of the highscore entry to the value inputted when this function was called.
        HighscoreEntry highscoreEntry = new HighscoreEntry { time = time, name = name };

        // Add the newly created entry to the highscores list.
        highscores.highscoreEntryList.Add(highscoreEntry);

        // Save the new entry by putting it into a JSON string and then storing that in the player prefs key.
        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString(playerPrefsString, json);
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

    // Function that clears out all current entries on the leaderboard, so new entries have a fresh template to draw on.
    void ClearLeaderboard()
    {
        // For each transform in the entry container, remove it.
        foreach (Transform entry in entryContainer)
        {
            if (entry.name != "HighscoreEntryTemplate")
            {
                Destroy(entry.gameObject);
            }
        }
    }
}