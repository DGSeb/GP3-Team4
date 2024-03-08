using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MetricUpdate : MonoBehaviour
{
    public Rigidbody rb;

    void FixedUpdate()
    {
        if (rb.velocity.magnitude > 0.01)
        {
            gameObject.GetComponent<TextMeshProUGUI>().text = "Velocity: " + rb.velocity.magnitude.ToString();
        }
        else
        {
            gameObject.GetComponent<TextMeshProUGUI>().text = ("Velocity: 0");
        }
    }
}
