using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    // reference to game manager script.
    private GameManager gM;

    // Variables used for things in the tutorial.
    [Header("Tutorial")]

    // Arrays for enemies and doors.
    [SerializeField] private GameObject[] shootingRangeEnemies;
    [SerializeField] private GameObject[] tutorialDoors;

    // Integer that determines how many enemies will be spawned in the shooting range.
    //private int shootingRangeEnemySpawnCount = 10;

    // First door bools.
    private bool movedForward;
    private bool movedBackward;
    private bool movedLeft;
    private bool movedRight;
    private bool firstDoorOpen;
    private bool startSpawningEnemies;

    // Second door variables.
    private float enemySpawnTime = 5.0f; // How often enemies spawn.
    private int enemiesInFiringRange = 6;
    private int enemiesToSpawn = 3; // number of enemies to spawn in the firing range.
    private bool firingRangeDone;
    private bool secondDoorOpen;

    // Third door variables
    [SerializeField] private AudioSource powerGoesOut;
    private bool powerOutAudioStarted;

    // Bools that determine if the player can perform certain movements.
    [HideInInspector] public static bool canDoubleJump;
    [HideInInspector] public static bool canDash;

    // bool to say whether or not debugging should show up in the console.
    private bool debug = false;

    private void Start()
    {
        // Set reference to game manager script.
        gM = this.gameObject.GetComponent<GameManager>();
        
        // Set enemy remaining variable.
        gM.enemiesRemaining = shootingRangeEnemies.Length;

        // Ensure double jump and dash is false when beginning the tutorial.
        canDoubleJump = false;
        canDash = false;
    }

    void Update()
    {
        // Run tutorial checks function that performs actions based on if certain conditions are met.
        TutorialChecks();
    }

    // Function for checks that occur while in the tutorial level.
    void TutorialChecks()
    {
        // Function used for testing things.
        TestingKeys();

        // Run door checks function that checks to see if doors should be opened.
        DoorChecks();

        // Once the audio has started, check to see if it has stopped. If it has, open the door.
        if (powerOutAudioStarted && !powerGoesOut.isPlaying)
        {
            Destroy(tutorialDoors[2]);
            powerOutAudioStarted = false;
        }

        // If the number of enemies eliminated is equal to half of the array length, that means enough enemies have been eliminated to complete the firing range.
        if (gM.enemiesEliminated >= (shootingRangeEnemies.Length / 2))
        {
            firingRangeDone = true;
        }
    }

    // Function that performs checks to see if doors should be opened.
    void DoorChecks()
    {
        // Input axis variables.
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Amount of input needed to set move variables to true.
        float inputAmountRequired = 0.1f;

        // If the first door isn't open, check for movement.
        if (!firstDoorOpen)
        {
            // If vertical movement is higher than the positive input amount, player is going forward, so set that variable to true.
            if (verticalInput >= inputAmountRequired)
            {
                movedForward = true;
            }
            // If vertical input is less than the negative input amount, player is going backwards, so set that variable to true.
            else if (verticalInput <= -inputAmountRequired)
            {
                movedBackward = true;
            }
            // If horizontal input is greater than the positive input amount, player is moving right, so set that variable to true.
            else if (horizontalInput >= inputAmountRequired)
            {
                movedRight = true;
            }
            // If horizontal input is less than the negative input amount, player is moving left, so set that variable to true.
            else if (horizontalInput <= -inputAmountRequired)
            {
                movedLeft = true;
            }

            // If the player has moved in all directions, open the door and set door open variable to true so this function does not run again
            // unless level is restarted/scene is reloaded.
            if (movedForward && movedBackward && movedRight && movedLeft)
            {
                Destroy(tutorialDoors[0]);
                firstDoorOpen = true;
                startSpawningEnemies = true;
            }
        }

        // If the first door is open, spawn the enemies
        if (startSpawningEnemies)
        {
            InvokeRepeating("FiringRangeSpawn", 2.0f, enemySpawnTime);
            startSpawningEnemies = false;
        }

        // If there are no more enemies in the firing range, the firing range is done.
        if (enemiesInFiringRange <= 0)
        {
            firingRangeDone = true;
        }

        // If the second door isn't open, check if enough enemies have been eliminated. If enough have, open the door.
        if (!secondDoorOpen && firingRangeDone)
        {
            Destroy(tutorialDoors[1]);
            secondDoorOpen = true;
            canDoubleJump = true;
        }        
    }

    // Handling of the spawning in the firing range in the tutorial.
    void FiringRangeSpawn()
    {
        // Turn off enemies in firing range to prep for next wave.
        for (int i = 0; i < shootingRangeEnemies.Length; i++)
        {
            // If the enemy exists, set it's active to false.
            if (shootingRangeEnemies[i] != null)
            {
                shootingRangeEnemies[i].SetActive(false);
            }
        }

        // Spawn enemies in firing range.
        for (int i = 1; i <= enemiesToSpawn; i++)
        {
            // Create a random number.
            int randomNum = Random.Range(0, shootingRangeEnemies.Length);

            // If there are more than 2 enemies in the array, keep finding new numbers
            if (shootingRangeEnemies.Length > enemiesToSpawn - 1)
            {
                // If that random number does not have a corresponding enemy in the array, keep finding a new number until it does.
                while (shootingRangeEnemies[randomNum] == null || shootingRangeEnemies[randomNum].activeSelf)
                {
                    randomNum = Random.Range(0, shootingRangeEnemies.Length);
                }
            }

            // Once a random number that correlates with an enemy in the array has been found, set that enemy active.
            shootingRangeEnemies[randomNum].SetActive(true);

            Debugging(shootingRangeEnemies[randomNum]);
        }
    }

    // Function that runs the power outage items.
    public void PowerOutage()
    {
        powerGoesOut.Play();
        powerOutAudioStarted = true;
    }

    // Function used for testing to ensure things are working.
    void TestingKeys()
    {
        // If the number 2 is pressed, open door 2.
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            firingRangeDone = true;
        }

        // If the number 3 is pressed, open door 3.
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Destroy(tutorialDoors[2]);
        }

        // If the backslash key is pressed, set debug to opposite of what it currently is.
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            debug = !debug;
        }
    }

    // Generic function that prints info inputted when function is called into the console if debugging is active.
    void Debugging<T>(T info)
    {
        if (debug)
        {
            Debug.Log(info);
        }
    }
}