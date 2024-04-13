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
        doorAnimator = GetComponentInParent<Animator>();    
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            // If player tag object enters the trigger, set open door to true to play the open door animation.
            case "Player":
                Debug.Log("HEre");
                if (doorAnimator != null)
                {
                    doorAnimator.SetBool("OpenDoor", true);
                }
                else
                {
                    Debug.LogWarning("doorAnimator not found.");
                }
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            // If player tag object exits the trigger, set open door to false to play the close door animation.
            case "Player":
                if (doorAnimator != null)
                {
                    doorAnimator.SetBool("OpenDoor", false);
                }
                else
                {
                    Debug.LogWarning("doorAnimator not found.");
                }
                break;
        }
    }
}
