using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiamTempPlayer : MonoBehaviour
{
    private GameManager gM; // game manager script reference.

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
        // If player hits the end object, update PB and go to next scene.
        if (other.name == "End")
        {
            gM.CheckPB();
            gM.ChangeScene("LiamsHighlyPsychoticJoint");
        }
        else if (other.name == "End2")
        {
            gM.CheckPB();
            gM.ChangeScene("LiamsWackyWonderland");
        }
        else if (other.name == "Hulk")
        {
            gM.CheckPB();
            gM.ChangeScene("LiamsWackyWonderland");
        }
    }

    // Function to check if the playe is out of bounds.
    void BoundaryCheck()
    {
        if (this.gameObject.transform.position.y < -50)
        {
            gM.ReloadScene();
        }
    }
}
