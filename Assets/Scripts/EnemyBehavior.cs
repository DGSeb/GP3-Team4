using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemyBehavior : MonoBehaviour
{
    private Transform player; // Reference to player transform.
    private GameManager gM; // Reference to game manager script.

    // Vector3 used for the direction the enemy needs to face to look at the player.
    private Vector3 directionToPlayer;

    // Variables related to firing of weapon.
    private float detectionRange = 25f; // How close player needs to be for detection.
    private float shootDelayTimer; // Timer that uses delay before enemy shoots at player.
    private float shootDelay = 1.8f; // Delay before enemy shoots at player.

    // Speed variables
    //private float bulletSpeed = 1.3f;
    private float enemyRotationSpeed = 20.5f;

    private bool playerInRange; // Bool that tracks if player is in range.
    private bool canFire = false; // Bool that determine whether or not the enemy can fire.

    private int enemyHealth = 1; // Int for enemy health.
    // Variable used for changing enemyHealth through get set so that other scripts do not have direct access to the variable.
    public int EnemyHealth
    {
        get { return enemyHealth; }
        set { enemyHealth = value; } 
    }

    // Variables related to the ray shot by the enemy towards the player.
    [Header("Raycast/Shooting")]
    private float timeAddition = 2.5f; // Time added to the clock when enemy shoots.

    private LineRenderer lineRenderer; // Line that displays when enemy is shooting at player.

    private bool canAddTime; // Bool that says whether or not time can be added to the timer.

    [Header("Audio")]
    [SerializeField] private AudioSource attackChargeSound;
    [SerializeField] private AudioSource attackSound;

    void Start()
    {
        // References
        player = GameObject.FindWithTag("Player").GetComponent<Transform>(); // Set player reference.
        gM = GameObject.Find("GameManager").GetComponent<GameManager>(); // Set game manager script reference.
        lineRenderer = GetComponent<LineRenderer>(); // Refernece to line renderer component.

        // Set the delay timer that controls when the enemy shoots to the delay value that can be adjusted.
        shootDelayTimer = shootDelay;

    }

    void Update()
    {
        // If the distance between the player and the enemy is less than the detection range, the player is in range, so set the bool to true.
        if (Vector3.Distance(transform.position, player.position) < detectionRange)
        {
            playerInRange = true;

            // The Vector3 that indicates the direction to face the player is found through the player position minus the position of the enemy.
            directionToPlayer = (player.position - transform.position).normalized;

            // Create a variable for what the raycast hits.
            RaycastHit hit;

            // Do a raycast in the direction of the player and check to see what it hit.
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
            {
                // If the hit rigidbody exists and if it is part of the player, draw a ray and begin attack sequence.
                if (hit.rigidbody != null && hit.rigidbody.CompareTag("Player"))
                {
                    // Set bool to true here to ensure that time only gets added to the timer when the player has been hit by the ray.
                    // This prevents time being added if the player was hit, but then hid behind a wall.
                    canAddTime = true;

                    // Run the draw ray function that creates a line at the enemy position going towards the player in the color red.
                    DrawRay(transform.position, hit.point, Color.red);

                    // Begin the counting down of the timer.
                    shootDelayTimer -= Time.deltaTime;

                    // Play audio of enemy attack charging up
                    if (!attackChargeSound.isPlaying)
                    {
                        attackChargeSound.Play();
                    }

                    // If the timer has reached or gone below zero, the enemy is now able to fire at the player.
                    if (shootDelayTimer <= 0)
                    {
                        canFire = true;
                        shootDelayTimer = 0; // Ensure timer does not go below zero.
                    }

                    // Create a float that will be the alpha value of the drawn ray.
                    // Set the float equal to the opposite percentage of the timer.
                    // How this works is if the timer is at 8 and the delay is set to 10, the math will be 1 - (8/10), which is 1 - 0.8, which equals 0.2.
                    // The alpha value should be increasing as the timer is decreasing,
                    // so the alpha does not use the percentage of how close the timer is to 0, but rather how far the timer is from it's highest value.
                    float alpha = 1.0f - (shootDelayTimer / shootDelay);

                    // Clamp the alpha value to between 0 and 1 to ensure that it does not go below 0 or above 1.
                    alpha = Mathf.Clamp01(alpha);

                    // Run the set alpha function that sets the drawn ray's alpha value to the alpha calculated above.
                    SetAlpha(alpha);

                    // If the enemy can fire and the player is currently in range, fire the enemy's weapon.
                    if (canFire && playerInRange)
                    {
                        FireWeapon();

                        // Set can fire to false as the enemy has to wait before firing again,
                        // and begin the cooldown by resetting the timer to its starting value.
                        canFire = false;
                        shootDelayTimer = shootDelay;
                    }
                }
                // If the ray hits an obstacle such as a wall, reset the alpha value for the next shot.
                // Also, prevent the addition of time to the timer by setting canAddTime bool to false.
                else
                {
                    // Make sure audio isn't playing.
                    attackChargeSound.Stop();

                    canAddTime = false;

                    // Draw a ray in the direction of the player, but it will hit an obstacle instead of the player.
                    DrawRay(transform.position, hit.point, Color.red);

                    // As the ray is not hitting the player, the timer to shoot should not be going down, so set it to its starting value.
                    shootDelayTimer = shootDelay;

                    // Perform the same math as above to calculate the alpha value of the drawn ray.
                    float alpha = 1.0f - (shootDelayTimer / shootDelay);
                    alpha = Mathf.Clamp01(alpha);
                    SetAlpha(alpha);
                }
            }
            // If the player is too far and the ray does not hit anything, draw it in the direction of the player
            // with the farthest point being the range at which the player can begin to be detected.
            else
            {
                // Make sure audio isn't playing.
                attackChargeSound.Stop();

                // Set this bool to false to ensure that time is not added to the timer when it shouldn't be.
                canAddTime = false;

                DrawRay(transform.position, transform.position + directionToPlayer.normalized * detectionRange, Color.red);
            }
        }
        // If the player is not in range, set playerInRange to false and turn off the ray that was drawn. Also stop audio if it's playing.
        else
        {
            playerInRange = false;
            lineRenderer.enabled = false;

            // Make sure audio isn't playing.
            attackChargeSound.Stop();
        }

        // If the player is in range, rotate the enemy towards the player.
        if (playerInRange)
        {
            // The target rotation of the enemy is rotating towards the direction of the player.
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            // Rotate the enemy over time towards the desired area multiplied by a speed variable that can be changed.
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * enemyRotationSpeed);
        }

        // If the enemy is at 0 health or, if it somehow occurs, below 0 health, destroy the enemy object.
        if (enemyHealth <= 0)
        {
            // Change the enemy count variables by decreasing one from the enemy remaining count and increasing the enemies eliminated count by 1.
            gM.ChangeEnemyCountVariables();
            gM.PlayEnemyDeathSound();
            if (gM.currentScene == "TutorialRemastered")
            {
                enemyHealth = 1;
                this.gameObject.SetActive(false);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }

    // Function that draws a line from a starting point to an ending point in a certain color.
    void DrawRay(Vector3 start, Vector3 end, Color color)
    {
        // Enable the line renderer component, set the first point on the line to the starting position,
        // set the second point on the line to the ending position, and apply a color.
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.material.color = color;
    }

    // Function that sets the alpha value of the material on the drawn ray (rendered line).
    void SetAlpha(float alpha)
    {
        // Create a variable that refers to the line renderers current color.
        Color color = lineRenderer.material.color;
        
        // Set the alpha value of the color to the alpha value inputted when this function was called.
        color.a = alpha;

        // Debug the current alpha value to ensure that it is changing correctly.
        //Debug.Log("Alpha: "+ alpha);

        // Set the line renderer material's color to the newly found color above.
        lineRenderer.material.color = color;
    }

    // Function that fires the enemy's weapon.
    void FireWeapon()
    {
        // If time can be added to the timer and the player is still in range, 
        // add time to the timer through a function in the game manager script using a set value that can be changed.
        if (canAddTime && playerInRange)
        {
            //Debug.Log("Fire");

            // If the sound isn't already playing, play the attack sound.
            if (!attackSound.isPlaying)
            {
                attackSound.Play();
            }

            gM.ChangeTimer(timeAddition);
        }

        // With the weapon fired and time added to the timer, set canAddTime back to false so the cooldown before the next shot begins.
        canAddTime = false;
    }

    // Function called in WeaponHandler script that occurs when the enemy is hit by a player's attack.
    public void LoseHealth()
    {
        EnemyHealth--;
    }
}