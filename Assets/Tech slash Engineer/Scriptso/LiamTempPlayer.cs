using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiamTempPlayer : MonoBehaviour
{
    private GameManager gM; // game manager script reference.

    // Array for enemies that will spawn
    [SerializeField] private GameObject[] enemyGroups;

    void Start()
    {
        // Set reference to game manager script.
        gM = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {
        // Check if player is out of bounds
        BoundaryCheck();
    }

    // Add this to player script so when they reach the end, their PB is updated.
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
                gM.AddLeaderboardEntry();
                gM.CheckPB();
                gM.ChangeScene("LiamsWackyWonderland");
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