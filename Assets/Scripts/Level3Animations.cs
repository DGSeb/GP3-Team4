using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level3Animations : MonoBehaviour
{
    // Reference to player transform.
    private Transform player;

    // range at which animations can play
    private float detectionRange = 25;

    // Reference to this object's animator
    private Animator animator;

    void Start()
    {
        // Set references.
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        animator = GetComponent<Animator>();
        
        // Set the animation speed to 0, this stops all animations.
        animator.speed = 0f;

        // If this is on the LnRRoom make the detection range a bit higher so that it turns on at the right time.
        if (this.gameObject.name == "LnRRoom")
        {
            detectionRange = 32;
        }
    }

    void FixedUpdate()
    {
        // If the distance from this object to the player is less than the detection range, set the animator speed to 1, which resumes the animation.
        if (Vector3.Distance(transform.position, player.position) < detectionRange)
        {
            animator.speed = 1f;
        }

        // If the distance from this object to the player is greater than the detection range, set the animator speed back to 0, which pauses the animation.
        if (Vector3.Distance(transform.position, player.position) > detectionRange)
        {
            animator.speed = 0f;
        }
    }
}
