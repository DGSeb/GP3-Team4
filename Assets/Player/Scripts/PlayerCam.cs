using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    public static float sensX;
    public static float sensY;

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
        // Set the X and Y sensitivity floats to the player prefs keys. If no value, set it to 400.
        sensX = PlayerPrefs.GetFloat("XSensitivity", 400f);
        sensY = PlayerPrefs.GetFloat("YSensitivity", 400f);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = GetComponent<Camera>(); // Cache the Camera component
    }

    private void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        // Combine mouse and controller input into one variable, so either can be used.
        float inputX = mouseX + Input.GetAxisRaw("Controller X");
        float inputY = mouseY + Input.GetAxisRaw("Controller Y");

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

    public void DoFov(float endValue)
    {
        cam.DOFieldOfView(endValue, 0.25f); // Use cached camera component
    }

    public void RecoilFire(float recoilX, float recoilY, float recoilZ)
    {
        targetRotation += new Vector3(Random.Range(recoilX * 0.75f, recoilX), Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }
}