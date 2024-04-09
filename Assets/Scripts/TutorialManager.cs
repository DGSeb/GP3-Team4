using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    // Variables used for things in the tutorial.
    [Header("Tutorial")]

    // Arrays for enemies and doors.
    [SerializeField] private GameObject[] shootingRangeEnemies;
    [SerializeField] private GameObject[] tutorialDoors;

    // Integer that determines how many enemies will be spawned in the shooting range.
    private int shootingRangeEnemySpawnCount = 10;

    // First door bools.
    private bool movedForward;
    private bool movedBackward;
    private bool movedLeft;
    private bool movedRight;
    private bool firstDoorOpen;

    // Second door bools.
    private int enemiesInFiringRange = 5;
    private bool firingRangeDone;
    public bool secondDoorOpen;

    // Bools that determine if the player can perform certain movements.
    [HideInInspector] public static bool canDoubleJump;

    void Update()
    {
        // Run tutorial checks function that performs actions based on if certain conditions are met.
        TutorialChecks();
    }

    // Function for checks that occur while in the tutorial level.
    void TutorialChecks()
    {
        // Run door checks function that checks to see if doors should be opened.
        DoorChecks();

        // If the second door is open, the player can now double jump.
        if (secondDoorOpen)
        {
            canDoubleJump = true;
        }

        if (Input.GetKeyDown(KeyCode.I))
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
            }
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
        }
    }

    // Handling of the spawning and other items of the firing range in the tutorial.
    public void FiringRangeSpawn()
    {
        // Turn off enemies in firing range to prep for next wave.
        for (int i = 0; i < shootingRangeEnemies.Length; i++)
        {
            shootingRangeEnemies[i].SetActive(false);
        }

        // Spawn enemies in firing range.
        for (int i = 0; i < shootingRangeEnemySpawnCount; i++)
        {
            shootingRangeEnemies[Random.Range(0, shootingRangeEnemies.Length)].SetActive(true);
        }
    }
}