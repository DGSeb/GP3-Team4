using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Sliders : MonoBehaviour
{
    void Start()
    {
        // Set the slider value to what the current sensitivity is.
        SetSliderValue();
    }

    // Function to update the X sensitivity based on the slider position.
    public void AdjustXSensitivity(float newSensX)
    {
        PlayerCam.sensX = newSensX;
    }

    // Function to update the Y sensitivity based on the slider position.
    public void AdjustYSensitivity(float newSensY)
    {
        PlayerCam.sensY = newSensY;
    }

    // Function to apply the value of the sensitivity to the slider so that the slider shows the correct position between scenes.
    void SetSliderValue()
    {
        if (this.gameObject.name == "XSensitivitySlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerCam.sensX;
        }
        else if (this.gameObject.name == "YSensitivitySlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerCam.sensY;
        }
    }
}
