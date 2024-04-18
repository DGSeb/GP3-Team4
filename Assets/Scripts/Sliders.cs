using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Sliders : MonoBehaviour
{
    // Reference to the crosshair object that is a part of the player UI.
    public RectTransform crosshair;

    // References to the audio mixers for audio settings.
    [Header("Audio")]
    [SerializeField] private AudioMixer master;

    void Start()
    {
        // Set the slider value to what the current sensitivity is.
        SetSliderValue();
    }

    // Function to update the mouse X sensitivity based on the slider position and apply it to player preferences key.
    public void MouseXSensitivityAdjustment(float newSensX)
    {
        PlayerCam.sensMouseX = newSensX;
        PlayerPrefs.SetFloat("MouseXSensitivity", PlayerCam.sensMouseX);
    }

    // Function to update the mouse Y sensitivity based on the slider position and apply it to player preferences key.
    public void MouseYSensitivityAdjustment(float newSensY)
    {
        PlayerCam.sensMouseY = newSensY;
        PlayerPrefs.SetFloat("MouseYSensitivity", PlayerCam.sensMouseY);
    }

    // Function to update the controller X sensitivity based on the slider position and apply it to player preferences key.
    public void ControllerXSensitivityAdjustment(float newSensX)
    {
        PlayerCam.sensControllerX = newSensX;
        PlayerPrefs.SetFloat("ControllerXSensitivity", PlayerCam.sensControllerX);
    }

    // Function to update the controller Y sensitivity based on the slider position and apply it to player preferences key.
    public void ControllerYSensitivityAdjustment(float newSensY)
    {
        PlayerCam.sensControllerY = newSensY;
        PlayerPrefs.SetFloat("ControllerYSensitivity", PlayerCam.sensControllerY);
    }

    // Function that updates the size of the crosshair based on the slider position and applies it to the player prefs key.
    public void AdjustCrosshairSize(float newCrosshairSize)
    {
        crosshair.transform.localScale = new Vector3(newCrosshairSize, newCrosshairSize, crosshair.transform.localScale.z);
        PlayerPrefs.SetFloat("CrosshairSize", newCrosshairSize);
    }

    // Adjust the master volume (volume of everything in the game).
    public void AudioMasterAdjustment(float newMasterVolume)
    {
        master.SetFloat("MasterVolume", Mathf.Log10(newMasterVolume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", newMasterVolume);
    }

    // Adjust the music volume in the game.
    public void AudioMusicAdjustment(float newMusicVolume)
    {
        master.SetFloat("MusicVolume", Mathf.Log10(newMusicVolume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", newMusicVolume);
    }

    // Adjust the SFX (sound effects) volume in the game.
    public void AudioSFXAdjustment(float newSFXVolume)
    {
        master.SetFloat("SFXVolume", Mathf.Log10(newSFXVolume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", newSFXVolume);
    }

    // Function to apply the value of the sensitivity to the slider so that the slider shows the correct position between scenes.
    void SetSliderValue()
    {
        // If mouse X Sensitivity slider, set its value to the player prefs key that stores mouse X sensitivity. If no value, set it to 400.
        if (this.gameObject.name == "MouseXSensitivitySlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("MouseXSensitivity", 400f);
        }

        // If mouse Y Sensitivity slider, set its value to the player prefs key that stores mouse Y sensitivity. If no value, set it to 400.
        else if (this.gameObject.name == "MouseYSensitivitySlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("MouseYSensitivity", 400f);
        }

        // If controller X Sensitivity slider, set its value to the player prefs key that stores controller X sensitivity. If no value, set it to 200.
        if (this.gameObject.name == "ControllerXSensitivitySlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("ControllerXSensitivity", 200f);
        }

        // If controller Y Sensitivity slider, set its value to the player prefs key that stores controller Y sensitivity. If no value, set it to 90.
        else if (this.gameObject.name == "ControllerYSensitivitySlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("ControllerYSensitivity", 90f);
        }

        // If crosshair size slider, set its value to the saved data in the player prefs key. If no value, set default of 0.15.
        else if (this.gameObject.name == "CrosshairSizeSlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("CrosshairSize", 0.15f);
        }

        // If master volume slider, set its value to the saved data in the player prefs key. If no value, default to 0.5.
        else if (this.gameObject.name == "MasterVolumeSlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        }

        // If music volume slider, set its value to the saved data in the player prefs key. If no value, default to 0.5.
        else if (this.gameObject.name == "MusicVolumeSlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        }

        // If SFX volume slider, set its value to the saved data in the player prefs key. If no value, default to 0.5.
        else if (this.gameObject.name == "SFXVolumeSlider")
        {
            this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        }
    }
}