using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    private Transform player; // Reference to player transform.

    // Vector3 used for the direction the enemy needs to face to look at the player.
    private Vector3 directionToPlayer;

    // Variables related to firing of weapon.
    private float detectionRange = 25f; // How close player needs to be for detection.
    private float shootDelay = 1.8f; // Delay before enemy shoots at player.

    // Speed variables
    private float bulletSpeed = 1.3f;
    private float enemyRotationSpeed = 2.5f;

    private bool playerInRange; // Bool that tracks if player is in range.
    private bool canFire = true; // Bool that determine whether or not the enemy can fire.

    private int enemyHealth = 1; // Int for enemy health.
    public int EnemyHealth
    {
        get { return enemyHealth; }
        set { enemyHealth = value; } 
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Transform>(); // Set player reference.
    }

    void Update()
    {
        // If the distance between the player and the enemy is less than the detection range, the player is in range, so set the bool to true.
        if (Vector3.Distance(transform.position, player.position) < detectionRange)
        {
            playerInRange = true;
        }
        // If not in range, set playerInRange to false.
        else
        {
            playerInRange = false;
        }

        // If the player is in range, rotate the enemy towards the player and begin the delay before the enemy shoots.
        if (playerInRange && canFire)
        {
            // The Vector3 that indicates the direction to face the player is found through the player position minus the position of the enemy.
            directionToPlayer = player.position - transform.position;

            // The target rotation of the enemy is rotating towards the direction of the player.
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            // Rotate the enemy over time towards the desired area multiplied by a speed variable that can be changed.
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * enemyRotationSpeed);

            // Invoke the firing of the enemy weapon with a delay.
            Invoke("FireWeapon", shootDelay);

            // Set can fire to false and begin the cooldown before enemy can shoot again.
            canFire = false;
            StartCoroutine(ResetShootDelay());
        }

        // If the enemy is at 0 health or, if it somehow occurs, below 0 health, destroy the enemy object.
        if (enemyHealth <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    // Function for firing of enemy weapon.
    void FireWeapon()
    {
        // Create a sphere called bullet, change its size, and put its position at the position of this enemy.
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.transform.localScale = new Vector3(0.23f, 0.23f, 0.23f);
        bullet.transform.position = this.transform.position;

        // Add a rigidbody to the bullet, change its mass, turn off the effect of gravity on the bullet,
        // and ensure collision detection is occurring quickly to prevent bullets going through walls.
        Rigidbody bulletRigidbody = bullet.AddComponent<Rigidbody>();
        bulletRigidbody.mass = 0.7f;
        bulletRigidbody.useGravity = false;
        bulletRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Set the velocity of the bullet to the direction of the player times the bullet speed, which can me modified.
        bulletRigidbody.velocity = directionToPlayer * bulletSpeed;

        // Destroy the bullet after 2 and a half seconds.
        Destroy(bullet, 2.5f);
    }
    
    // Coroutine for allowing the enemy to shoot again. 
    IEnumerator ResetShootDelay()
    {
        yield return new WaitForSeconds(Random.Range(1.2f, 2.6f));
        canFire = true;
    }

    // Function called in WeaponHandler script that occurs when the enemy is hit by a player's attack.
    public void LoseHealth()
    {
        EnemyHealth--;
    }
}