using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiamTempPlayer : MonoBehaviour
{
    private GameManager gM; // game manager script reference.
    private TutorialManager tM; // Tutorial manager script reference.

    // Array for enemies that will spawn
    [SerializeField] private GameObject[] enemyGroups;

    void Start()
    {
        // Set reference to scripts.
        gM = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        tM = GameObject.FindWithTag("GameManager").GetComponent<TutorialManager>();

    }

    void Update()
    {
        // Check if player is out of bounds
        BoundaryCheck();
    }

    // Check what trigger the player walked into.
    private void OnTriggerEnter(Collider other)
    {
        switch(other.name)
        {
            // If player hits the end object, update PB and go to next scene.
            case "End":
                gM.AddLeaderboardEntry();
                gM.CheckPB();
                gM.ChangeScene("LiamsHighlyPsychoticJoint");
                break;

            // If second ending is hit, update pb and change scene.
            case "End2":
                gM.AddLeaderboardEntry();
                gM.CheckPB();
                gM.ChangeScene("LiamsWackyWonderland");
                break;

            // If tutorial ending is hit, update pb and change scene.
            case "Glitch":
                // If the player has eliminated enough enemies, add a leaderboard entry, set the run time for the results screen,
                // check the player's PB, the player won so set that bool to true and make sure lost is false, and change scene.
                if (gM.enoughEnemiesEliminated)
                {
                    gM.AddLeaderboardEntry();
                    gM.SetRunTime();
                    gM.CheckPB();
                    ResultScreen.won = true;
                    ResultScreen.lost = false;
                    gM.ChangeScene("ResultsScreen");
                }
                break;

            // If the tutorial pressure plate is walked on, run function from tutorial manager script that turns off all current enemies
            // and then spawns all new enemies. Also, destroy the pressure plate.
            case "TutorialPressurePlate":
                tM.FiringRangeSpawn();
                Destroy(other.gameObject);
                break;

            // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
            case "PressurePlate0":
                enemyGroups[0].SetActive(true);
                Destroy(other.gameObject);
                break;

            // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
            case "PressurePlate1":
                enemyGroups[1].SetActive(true);
                Destroy(other.gameObject);
                break;

            // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
            case "PressurePlate2":
                enemyGroups[2].SetActive(true);
                Destroy(other.gameObject);
                break;

            // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
            case "PressurePlate3":
                enemyGroups[3].SetActive(true);
                Destroy(other.gameObject);
                break;

            // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
            case "PressurePlate4":
                enemyGroups[4].SetActive(true);
                Destroy(other.gameObject);
                break;

            // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
            case "PressurePlate5":
                enemyGroups[5].SetActive(true);
                Destroy(other.gameObject);
                break;

            // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
            case "PressurePlate6":
                enemyGroups[6].SetActive(true);
                Destroy(other.gameObject);
                break;
        }
    }

    // Function to check if the player is out of bounds.
    void BoundaryCheck()
    {
        // If player is below -50 on the y axis, respawn them.
        if (this.gameObject.transform.position.y < -50)
        {
            gM.ReloadScene();
        }
    }
}