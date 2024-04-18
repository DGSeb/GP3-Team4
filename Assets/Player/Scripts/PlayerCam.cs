using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    // Mouse sensitivity
    public static float sensMouseX;
    public static float sensMouseY;

    // Controller sensitivity
    public static float sensControllerX = 225f;
    public static float sensControllerY = 100f;

    public Transform orientation;

    private Camera cam; // Cached camera component

    float xRotation;
    float yRotation;

    // Recoil \\

    //Rotations
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [Header("Settings")]
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    private void Awake()
    {
        // Set the mouse X and Y sensitivity floats to the player prefs keys. If no value, set it to 400.
        sensMouseX = PlayerPrefs.GetFloat("MouseXSensitivity", 400f);
        sensMouseY = PlayerPrefs.GetFloat("MouseYSensitivity", 400f);

        // Set the controller X and Y sensitivity floats to the player prefs keys. If no value, x is 200 and y is 90.
        sensControllerX = PlayerPrefs.GetFloat("ControllerXSensitivity", 200f);
        sensControllerY = PlayerPrefs.GetFloat("ControllerYSensitivity", 90f);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = GetComponent<Camera>(); // Cache the Camera component
    }

    private void Update()
    {
        if (GameManager.isPlayerActive)
        {
            // Get mouse input
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensMouseX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensMouseY;

            // Get controller input
            float controllerX = Input.GetAxisRaw("Controller X") * Time.deltaTime * sensControllerX;
            float controllerY = Input.GetAxisRaw("Controller Y") * Time.deltaTime * sensControllerY;

            // Set the input for x and y to the mouse plus controller input. This ensures that the subsequent code can remain the same, yet use two kinds of input (mouse and controller).
            // Only issue is that if you push on controller joystick plus move mouse at the same time, they both move the camera,
            // but who is gonna be playing with one hand on controller and one hand on mouse fr.
            float inputX = mouseX + controllerX;
            float inputY = mouseY + controllerY;


            yRotation += inputX;
            xRotation -= inputY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // Rotate cam and orientation
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);

            targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
            currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
            transform.localRotation *= Quaternion.Euler(currentRotation);
        }
    }

    public void DoFov(float endValue)
    {
        cam.DOFieldOfView(endValue, 0.25f); // Use cached camera component
    }

    public void RecoilFire(float recoilX, float recoilY, float recoilZ)
    {
        targetRotation += new Vector3(Random.Range(recoilX * 0.75f, recoilX), Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }
}