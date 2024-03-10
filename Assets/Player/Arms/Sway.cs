using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class Sway : MonoBehaviour
{
    [SerializeField] public Transform weaponTransform;

    [SerializeField] public GameObject player;
    [SerializeField] public Rigidbody rb;

    [Header("Sway Properties")]
    [SerializeField] private float swayAmount = 0.01f;
    [SerializeField] public float maxSwayAmount = 0.5f;
    [SerializeField] public float swaySmooth = 50f;
    [SerializeField] public AnimationCurve swayCurve;

    [Range(0f, 1f)]
    [SerializeField] public float swaySmoothCounteraction = 1f;

    [Header("Rotation")]
    [SerializeField] public float rotationSwayMultiplier = -1f;

    [Header("Position")]
    [SerializeField] public float positionSwayMultiplier = 9f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector2 sway;
    Quaternion lastRot;

    [Header("Bobbing Properties")]
    public bool bobOffset = true;
    public bool bobSway = true;

    [Header("Bobbing")]
    public float speedCurve;
    float curveSin { get => Mathf.Sin(speedCurve); }
    float curveCos { get => Mathf.Cos(speedCurve); }

    public Vector3 travelLimit = Vector3.one * 0.025f; // the maximum limits of travel from move input
    public Vector3 bobLimit = Vector3.one * 0.01f; // limits of travel from bobbing over time 

    Vector3 bobPosition;

    void BobOffset()
    {
        //used to generate sin and cos waves
        if (player.GetComponent<PlayerMovement>().grounded == true)
            speedCurve += Time.deltaTime * rb.velocity.magnitude + 0.01f;
        else
            speedCurve = Time.deltaTime * 1f + 0.01f;

        if (bobOffset == false) { bobPosition = Vector3.zero; return; }

        if (player.GetComponent<PlayerMovement>().grounded == true)
        {
            bobPosition.x =
                (curveCos * bobLimit.x * 1) - (player.GetComponent<PlayerMovement>().horizontalInput * travelLimit.x);
        }
        else
        {
            bobPosition.x =
                (curveCos * bobLimit.x * 0) - (player.GetComponent<PlayerMovement>().horizontalInput * travelLimit.x);
        }

        bobPosition.y =
                (curveSin * bobLimit.y)
                - (rb.velocity.y * travelLimit.y);

        bobPosition.z =
            - (player.GetComponent<PlayerMovement>().verticalInput * travelLimit.z);

    }

    Vector3 bobEulerRotation;
    public Vector3 multiplier;

    void BobRotation()
    {
        bool walking = (player.GetComponent<PlayerMovement>().horizontalInput + player.GetComponent<PlayerMovement>().verticalInput != 0);

        if (walking)
        {
            bobEulerRotation.x = multiplier.x * (Mathf.Sin(2 * speedCurve));

            bobEulerRotation.y = multiplier.y * curveCos;

            bobEulerRotation.z = multiplier.z * curveCos * player.GetComponent<PlayerMovement>().horizontalInput;
        }
        else
        {
            bobEulerRotation.x = multiplier.x * (Mathf.Sin(2 * speedCurve) / 2); //pitch
            bobEulerRotation.y = 0; //yaw
            bobEulerRotation.z = 0; //roll
        }
    }


    float smooth = 10f;
    float smoothRot = 12f;

    void Composite()
    {
        //position
        transform.localPosition =
            Vector3.Lerp(transform.localPosition,
            transform.localPosition + bobPosition,
            Time.deltaTime * smooth );

        //rotation
        transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation * Quaternion.Euler(bobEulerRotation), Time.deltaTime * smoothRot);
    }



    private void Reset()
    {
        Keyframe[] ks = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0) };
        swayCurve = new AnimationCurve(ks);
    }

    private void Start()
    {
        if (!weaponTransform)
            weaponTransform = transform;
        lastRot = transform.localRotation;
        initialPosition = weaponTransform.localPosition;
        initialRotation = weaponTransform.localRotation;
    }

    private void Update()
    {
        var angularVelocity = Quaternion.Inverse(lastRot) * transform.rotation;

        float mouseX = FixAngle(angularVelocity.eulerAngles.y) * swayAmount;
        float mouseY = -FixAngle(angularVelocity.eulerAngles.x) * swayAmount;

        lastRot = transform.rotation;

        sway = Vector2.MoveTowards(sway, Vector2.zero, swayCurve.Evaluate(Time.deltaTime * swaySmoothCounteraction * sway.magnitude * swaySmooth));
        sway = Vector2.ClampMagnitude(new Vector2(mouseX, mouseY) + sway, maxSwayAmount);

        weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, new Vector3(sway.x, sway.y, 0) * positionSwayMultiplier * Mathf.Deg2Rad + initialPosition, swayCurve.Evaluate(Time.deltaTime * swaySmooth));
        weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, initialRotation * Quaternion.Euler(Mathf.Rad2Deg * rotationSwayMultiplier * new Vector3(-sway.y, sway.x, 0)), swayCurve.Evaluate(Time.deltaTime * swaySmooth));

        BobOffset();
        BobRotation();
        Composite();

    }

    private float FixAngle(float angle)
    {
        return Mathf.Repeat(angle + 180f, 360f) - 180f;
    }
}
