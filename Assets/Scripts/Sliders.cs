using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Sliders : MonoBehaviour
{
    // Reference to the crosshair object that is a part of the player UI.
    public RectTransform crosshair;

    void Start()
    {
        // Set the slider value to what the current sensitivity is.
        SetSliderValue();
    }

    // Function to update the X sensitivity based on the slider position and apply it to player preferences key.
    public void AdjustXSensitivity(float newSensX)
    {
        PlayerCam.sensX = newSensX;
        PlayerPrefs.SetFloat("XSensitivity", PlayerCam.sensX);
    }

    // Function to update the Y sensitivity based on the slider position and apply it to player preferences key.
    public void AdjustYSensitivity(float newSensY)
    {
        PlayerCam.sensY = newSensY;
        PlayerPrefs.SetFloat("YSensitivity", PlayerCam.sensY);
    }

    // Function that updates the size of the crosshair based on the slider position and applies it to the player prefs key.
    public void AdjustCrosshairSize(float newCrosshairSize)
    {
        crosshair.transform.localScale = new Vector3(newCrosshairSize, newCrosshairSize, crosshair.transform.localScale.z);
        PlayerPrefs.SetFloat("CrosshairSize", newCrosshairSize);
    }

    // Function to apply the value of the sensitivity to the slider so that the slider shows the correct position between scenes.
    void SetSliderValue()
    {
        // If X Sensitivity slider, set its value to the player prefs key that stores X sensitivity. If no value, set it to 400.
        if (this.gameObject.name == "XSensitivitySlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("XSensitivity", 400f);
        }

        // If Y Sensitivity slider, set its value to the player prefs key that stores Y sensitivity. If no value, set it to 400.
        else if (this.gameObject.name == "YSensitivitySlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("YSensitivity", 400f);
        }

        // If crosshair size slider, set its value to the saved data in the player prefs key. If no value, set default of 0.15.
        else if (this.gameObject.name == "CrosshairSizeSlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("CrosshairSize", 0.15f);
        }
    }
}