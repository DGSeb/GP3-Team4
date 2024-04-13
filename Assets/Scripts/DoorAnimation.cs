using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnimation : MonoBehaviour
{
    // Reference to animator on door.
    private Animator doorAnimator;

    void Start()
    {
        // Set reference.
        doorAnimator = GetComponent<Animator>();    
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            // If player tag object enters the trigger, set open door to true to play the open door animation.
            case "Player":
                doorAnimator.SetBool("OpenDoor", true);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            // If player tag object exits the trigger, set open door to false to play the close door animation.
            case "Player":
                doorAnimator.SetBool("OpenDoor", false);
                break;
        }
    }
}
