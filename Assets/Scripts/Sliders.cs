using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class Sliders : MonoBehaviour
{
    // Reference to the crosshair object that is a part of the player UI.
    public RectTransform crosshair;

    // References to the audio mixers for audio settings.
    [Header("Audio")]
    [SerializeField] private AudioMixer master;

    // Text variables that display the value of a slider.
    [Header("Slider Value Text")]
    [SerializeField] private TextMeshProUGUI mouseXValue;
    [SerializeField] private TextMeshProUGUI mouseYValue;
    [SerializeField] private TextMeshProUGUI controllerXValue;
    [SerializeField] private TextMeshProUGUI controllerYValue;
    [SerializeField] private TextMeshProUGUI fpsValue;
    [SerializeField] private TextMeshProUGUI masterVolumeValue;
    [SerializeField] private TextMeshProUGUI musicVolumeValue;
    [SerializeField] private TextMeshProUGUI sFXVolumeValue;
    private int decimalPlaces = 1;

    [Header("VSync")]
    [SerializeField] private Toggle vSyncToggle;
    private bool vSyncOn;

    void Start()
    {
        // Set the slider value to what the current sensitivity is.
        SetSliderValue();

        if (vSyncToggle != null)
        {
            if (vSyncOn)
            {
                vSyncToggle.isOn = vSyncOn;
            }
            else
            {
                vSyncToggle.isOn = vSyncOn;
                Application.targetFrameRate = PlayerPrefs.GetInt("FPSLimit", 100);
            }
        }
    }

    // Function to update the mouse X sensitivity based on the slider position and apply it to player preferences key.
    public void MouseXSensitivityAdjustment(float newSensX)
    {
        PlayerCam.sensMouseX = newSensX;
        PlayerPrefs.SetFloat("MouseXSensitivity", PlayerCam.sensMouseX);

        // Set x sens text to the value of the slider.
        SetSliderValueText();
    }

    // Function to update the mouse Y sensitivity based on the slider position and apply it to player preferences key.
    public void MouseYSensitivityAdjustment(float newSensY)
    {
        PlayerCam.sensMouseY = newSensY;
        PlayerPrefs.SetFloat("MouseYSensitivity", PlayerCam.sensMouseY);

        // Set y sens text to the value of the slider.
        SetSliderValueText();
    }

    // Function to update the controller X sensitivity based on the slider position and apply it to player preferences key.
    public void ControllerXSensitivityAdjustment(float newSensX)
    {
        PlayerCam.sensControllerX = newSensX;
        PlayerPrefs.SetFloat("ControllerXSensitivity", PlayerCam.sensControllerX);

        // Set x sens text to the value of the slider.
        SetSliderValueText();
    }

    // Function to update the controller Y sensitivity based on the slider position and apply it to player preferences key.
    public void ControllerYSensitivityAdjustment(float newSensY)
    {
        PlayerCam.sensControllerY = newSensY;
        PlayerPrefs.SetFloat("ControllerYSensitivity", PlayerCam.sensControllerY);

        // Set y sens text to the value of the slider.
        SetSliderValueText();
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

        // Set the master audio text to the value of the slider.
        SetSliderValue();
    }

    // Adjust the music volume in the game.
    public void AudioMusicAdjustment(float newMusicVolume)
    {
        master.SetFloat("MusicVolume", Mathf.Log10(newMusicVolume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", newMusicVolume);

        // Set the music audio text to the value of the slider.
        SetSliderValue();
    }

    // Adjust the SFX (sound effects) volume in the game.
    public void AudioSFXAdjustment(float newSFXVolume)
    {
        master.SetFloat("SFXVolume", Mathf.Log10(newSFXVolume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", newSFXVolume);

        // Set the SFX audio text to the value of the slider.
        SetSliderValue();
    }

    // Adjust the FPS limit.
    public void FPSLimitAdjustment(float newFPSLimit)
    {
        // Turn off V-Sync to allow adjustable frame rate.
        QualitySettings.vSyncCount = 0;
        vSyncOn = false;
        vSyncToggle.isOn = vSyncOn;

        Application.targetFrameRate = Mathf.RoundToInt(newFPSLimit);
        PlayerPrefs.SetInt("FPSLimit", Mathf.RoundToInt(newFPSLimit));

        // Set the value of the slider text to the fps value.
        SetSliderValueText();
    }

    // Turns on V-Sync to limit frame rate to the refresh rate of the monitor.
    public void ActivateVSync()
    {
        QualitySettings.vSyncCount = 1;
        vSyncOn = true;
    }

    // Function to apply the value of the sensitivity to the slider so that the slider shows the correct position between scenes.
    void SetSliderValue()
    {
        switch (this.gameObject.name)
        {
            // If mouse X Sensitivity slider, set its value to the player prefs key that stores mouse X sensitivity. If no value, set it to 400.
            case "MouseXSensitivitySlider":
                this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("MouseXSensitivity", 400f);
                break;

            // If mouse Y Sensitivity slider, set its value to the player prefs key that stores mouse Y sensitivity. If no value, set it to 400.
            case "MouseYSensitivitySlider":
                this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("MouseYSensitivity", 400f);
                break;

            // If controller X Sensitivity slider, set its value to the player prefs key that stores controller X sensitivity. If no value, set it to 200.
            case "ControllerXSensitivitySlider":
                this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("ControllerXSensitivity", 200f);
                break;

            // If controller Y Sensitivity slider, set its value to the player prefs key that stores controller Y sensitivity. If no value, set it to 90.
            case "ControllerYSensitivitySlider":
                this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("ControllerYSensitivity", 90f);
                break;

            // If crosshair size slider, set its value to the saved data in the player prefs key. If no value, set default of 0.15.
            case "CrosshairSizeSlider":
                this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("CrosshairSize", 0.15f);
                break;

            // If master volume slider, set its value to the saved data in the player prefs key. If no value, default to 0.5.
            case "MasterVolumeSlider":
                this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
                break;

            // If music volume slider, set its value to the saved data in the player prefs key. If no value, default to 0.5.
            case "MusicVolumeSlider":
                this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
                break;

            // If SFX volume slider, set its value to the saved data in the player prefs key. If no value, default to 0.5.
            case "SFXVolumeSlider":
                this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
                break;

            // If FPS slider, set its value to the saved data in the player prefs key. If no value, default to 100.
            case "FPSSlider":
                this.GameObject().GetComponent<Slider>().value = PlayerPrefs.GetInt("FPSLimit", 100);
                break;
        }

        // Set the text next to the sliders to the value of the slider.
        SetSliderValueText();
    }

    // Set the text next to the sliders to the value of the slider.
    void SetSliderValueText()
    {
        switch (this.gameObject.name)
        {
            // Set mouse sensitivity text.
            case "MouseXSensitivitySlider":
                mouseXValue.text = PlayerPrefs.GetFloat("MouseXSensitivity", 400f).ToString("F" + decimalPlaces);
                break;

            // Set mouse sensitivity text.
            case "MouseYSensitivitySlider":
                mouseYValue.text = PlayerPrefs.GetFloat("MouseYSensitivity", 400f).ToString("F" + decimalPlaces);
                break;

            // Set controller sensitivity text.
            case "ControllerXSensitivitySlider":
                controllerXValue.text = PlayerPrefs.GetFloat("ControllerXSensitivity", 200f).ToString("F" + decimalPlaces);
                break;

            // Set controller sensitivity text.
            case "ControllerYSensitivitySlider":
                controllerYValue.text = PlayerPrefs.GetFloat("ControllerYSensitivity", 90f).ToString("F" + decimalPlaces);
                break;

            // Set fps value text
            case "FPSSlider":
                fpsValue.text = PlayerPrefs.GetInt("FPSLimit", 100).ToString();
                break;

            // Set master volume value text
            case "MasterVolumeSlider":
                masterVolumeValue.text = (Mathf.Log10(PlayerPrefs.GetFloat("MasterVolume", 0.5f)) * 20).ToString("F" + decimalPlaces);
                break;

            // Set music volume value text
            case "MusicVolumeSlider":
                musicVolumeValue.text = (Mathf.Log10(PlayerPrefs.GetFloat("MusicVolume", 0.5f)) * 20).ToString("F" + decimalPlaces);
                break;

            // Set SFX volume value text
            case "SFXVolumeSlider":
                sFXVolumeValue.text = (Mathf.Log10(PlayerPrefs.GetFloat("SFXVolume", 0.5f)) * 20).ToString("F" + decimalPlaces);
                break;
        } 
    }
}