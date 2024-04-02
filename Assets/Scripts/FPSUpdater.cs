using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FPSUpdater : MonoBehaviour
{
    float fps;
    float updateFrequency = 0.01f;
    float updateTimer;

    [SerializeField] TextMeshProUGUI fpsTitle;

    void UpdateFPSDisplay()
    {
        updateTimer -= Time.unscaledDeltaTime;

        if (updateTimer <= 0f)
        {
            fps = 1f / Time.unscaledDeltaTime;
            fpsTitle.text = "FPS: " + Mathf.Round(fps);
            updateTimer = 0.2f;
        }
    }

    private void Update()
    {
        UpdateFPSDisplay();
    }

    private void Start()
    {
        updateTimer = updateFrequency;
        fpsTitle = GetComponentInChildren<TextMeshProUGUI>();
    }
    /*public TextMeshProUGUI fpsText;

    private float pollingTime = 1f;
    private float time;
    private int frameCount;

    // Update is called once per frame
    void Update()
    {
        time += Time.unscaledDeltaTime;
        
        frameCount++;

        if (time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            fpsText.text = frameCount.ToString() + " FPS";

            time -= pollingTime;
            frameCount = 0;
        }
    }*/
}
