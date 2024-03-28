using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    private Transform player; // Reference to player transform.

    private Vector3 direction;

    // Variables related to firing of weapon.
    private float detectionRange = 10f; // How close player needs to be for detection.
    private float shootDelay = 1.0f; // Delay before enemy shoots at player.

    public float bulletSpeed = 0.8f;

    private bool playerInRange; // Bool that tracks if player is in range.
    private bool canFire = true;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
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
            direction = (player.position - transform.position).normalized;

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            FireWeapon();

            canFire = false;

            StartCoroutine(ResetShootDelay());
        }
    }

    // Temp function for firing of enemy weapon.
    void FireWeapon()
    {
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        bullet.transform.position = this.transform.position;

        Rigidbody bulletRigidbody = bullet.AddComponent<Rigidbody>();

        bulletRigidbody.velocity = direction * bulletSpeed;

    }
    IEnumerator ResetShootDelay()
    {
        yield return new WaitForSeconds(shootDelay);
        canFire = true;
    }
}
