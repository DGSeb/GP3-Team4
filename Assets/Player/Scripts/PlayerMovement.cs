using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float dashSpeed;
    public float dashSpeedChangeFactor;

    public float groundDrag;

    public float maxYSpeed;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public Transform orientation;
    public bool readyToJump;

    // Int that determines how many times player can jump before touching the ground again.
    private int jumpsRemaining = 0;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYscale;

    public GameObject arms;
    public Transform camTransform;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    // Controller binds.
    private KeyCode jumpController = KeyCode.JoystickButton0;
    private KeyCode sprintController = KeyCode.JoystickButton8;
    private KeyCode crouchController = KeyCode.JoystickButton9;

    [Header("Slope")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;


    public float horizontalInput;
    public float verticalInput;

    private float velocityCap;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        dashing,
        air
    }

    public bool dashing;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource dashSound;

    private GameManager gM; // game manager script reference.
    private TutorialManager tM; // Tutorial manager script reference.

    /*[Header("Enemy Array")]
    // Array for enemies that will spawn
    [SerializeField] private GameObject[] enemyGroups;

    // Array for pressure plates in tutorial
    [SerializeField] private GameObject[] tutorialPressurePlates;*/

    [Header("Tutorial Items")]
    public Transform dashCheckpoint;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYscale = transform.localScale.y;

        // Set reference to scripts.
        gM = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        tM = GameObject.FindWithTag("GameManager").GetComponent<TutorialManager>();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;

    private void StateHandler()
    {

        // Mode - Dashing
        if (dashing)
        {
            // If dash sound isn't already playing, play it.
            if (!dashSound.isPlaying)
            {
                dashSound.Play();
            }

            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }

        // Mode - Crouching
        else if (Input.GetKey(crouchKey) || Input.GetKey(crouchController))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
            velocityCap = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && (Input.GetKey(sprintKey) || Input.GetKey(sprintController)))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
            velocityCap = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
            velocityCap = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;

            if (desiredMoveSpeed < sprintSpeed)
            {
                desiredMoveSpeed = walkSpeed;
                velocityCap = walkSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
                velocityCap = sprintSpeed;
            }
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) keepMomentum = true;

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);        

        if (grounded && TutorialManager.canDoubleJump)
            // Give player an extra jump once they touch the ground.
            jumpsRemaining = 1;

        // If the player is active, check for their input.
        if (GameManager.isPlayerActive)
        {
            MyInput();
        }
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded && state != MovementState.dashing)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        // Run function that checks whether or not the player is within the map boundaries.
        BoundaryCheck();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if((Input.GetKeyDown(jumpKey) || Input.GetKeyDown(jumpController)) && readyToJump)
        {
            if (jumpsRemaining > 0)
            {
                readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }
            else if (grounded)
            {
                readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }

        // when to crouch
        if ((Input.GetKeyDown(crouchKey) || Input.GetKeyDown(crouchController)))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            if (grounded)
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }
        }

        if ((Input.GetKeyUp(crouchKey) || Input.GetKeyUp(crouchController)))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);
        }
    }

    private void MovePlayer()
    {
        // disable when dashing to prevent gravity conflict
        if (state == MovementState.dashing) return;

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // if on ground
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        // on slope
        if(OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if(rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // in air
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // turn off gravity while on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // limited speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
               Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

        // limit y vel
        if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed)
            rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        // Subtract 1 from the remaining jumps count.
        jumpsRemaining--;
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private float speedChangeFactor;
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            if (horizontalVelocity.magnitude < velocityCap)
            {
                //print("X " + Mathf.Abs(rb.velocity.x)); 
                //print("Y " + Mathf.Abs(rb.velocity.y));
                boostFactor = 1000;
            }

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum= false;    
    }

    // Function to check if the player is out of bounds.
    void BoundaryCheck()
    {
        // If player is below -50 on the y axis, respawn them.
        if (this.gameObject.transform.position.y < -50)
        {
            gM.ReloadScene();
        }
    }

    // Check what trigger the player walked into.
    private void OnTriggerEnter(Collider other)
    {
        switch (other.name)
        {
            // If player hits the end object, update PB and go to next scene.
            case "End":
                gM.AddLeaderboardEntry();
                gM.CheckPB();
                gM.ChangeScene("LiamsHighlyPsychoticJoint");
                break;

            // If second ending is hit, update pb and change scene.
            case "End2":
                gM.AddLeaderboardEntry();
                gM.CheckPB();
                gM.ChangeScene("LiamsWackyWonderland");
                break;

            // If tutorial ending is hit, update pb and change scene.
            case "Glitch":
                // If the player has eliminated enough enemies, add a leaderboard entry, set the run time for the results screen,
                // check the player's PB, the player won so set that bool to true and make sure lost is false, and change scene.
                if (gM.enoughEnemiesEliminated)
                {
                    gM.AddLeaderboardEntry();
                    gM.SetRunTime();
                    gM.CheckPB();
                    ResultScreen.won = true;
                    ResultScreen.lost = false;
                    gM.ChangeScene("ResultsScreen");
                }
                break;

            // If the player runs into the dash trigger, let them dash and destroy the trigger.
            // Also, turn off double jump so player can only dash at the next part.
            case "DashTrigger":
                TutorialManager.canDash = true;
                TutorialManager.canDoubleJump = false;
                jumpsRemaining = 0;
                Destroy(other.gameObject);
                break;

            // If player walks into the power outage trigger, shut off power and start glitching the game.
            // Also, allow the player to double jump and destroy the trigger.
            case "PowerOutageTrigger":
                tM.PowerOutage();
                TutorialManager.canDoubleJump = true;
                Destroy(other.gameObject);
                break;

            // If player walks into the door, play the door's open animation.
            case "Door4":
                other.GetComponent<Animator>().Play("doorOpen");
                break;

            // If player lands in the DashFallZone trigger, transform them back to the point before the dash.
            case "DashFallZone":
                this.gameObject.transform.position = new Vector3(dashCheckpoint.position.x, dashCheckpoint.position.y, dashCheckpoint.position.z);
                break;

            /*// If the tutorial pressure plate is walked on, run function from tutorial manager script that turns off all current enemies
            // and then spawns all new enemies. Also, turn off the trigger on the current pressure plate and turn on the other pressure plate triggers.
            case "TutorialPressurePlate1":
                tM.FiringRangeSpawn();
                other.gameObject.GetComponent<BoxCollider>().enabled = false;
                tutorialPressurePlates[1].GetComponent<BoxCollider>().enabled = true;
                tutorialPressurePlates[2].GetComponent<BoxCollider>().enabled = true;
                break;

            // If the tutorial pressure plate is walked on, run function from tutorial manager script that turns off all current enemies
            // and then spawns all new enemies. Also, turn off the trigger on the current pressure plate and turn on the other pressure plate triggers.
            case "TutorialPressurePlate2":
                tM.FiringRangeSpawn();
                other.gameObject.GetComponent<BoxCollider>().enabled = false;
                tutorialPressurePlates[0].GetComponent<BoxCollider>().enabled = true;
                tutorialPressurePlates[2].GetComponent<BoxCollider>().enabled = true;
                break;

            // If the tutorial pressure plate is walked on, run function from tutorial manager script that turns off all current enemies
            // and then spawns all new enemies. Also, turn off the trigger on the current pressure plate and turn on the other pressure plate triggers.
            case "TutorialPressurePlate3":
                tM.FiringRangeSpawn();
                other.gameObject.GetComponent<BoxCollider>().enabled = false;
                tutorialPressurePlates[0].GetComponent<BoxCollider>().enabled = true;
                tutorialPressurePlates[1].GetComponent<BoxCollider>().enabled = true;
                break;*/

                /*// If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
                case "PressurePlate0":
                    enemyGroups[0].SetActive(true);
                    Destroy(other.gameObject);
                    break;

                // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
                case "PressurePlate1":
                    enemyGroups[1].SetActive(true);
                    Destroy(other.gameObject);
                    break;

                // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
                case "PressurePlate2":
                    enemyGroups[2].SetActive(true);
                    Destroy(other.gameObject);
                    break;

                // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
                case "PressurePlate3":
                    enemyGroups[3].SetActive(true);
                    Destroy(other.gameObject);
                    break;

                // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
                case "PressurePlate4":
                    enemyGroups[4].SetActive(true);
                    Destroy(other.gameObject);
                    break;

                // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
                case "PressurePlate5":
                    enemyGroups[5].SetActive(true);
                    Destroy(other.gameObject);
                    break;

                // If pressure plate is hit, spawn corresponding enemy group and destroy the pressure plate.
                case "PressurePlate6":
                    enemyGroups[6].SetActive(true);
                    Destroy(other.gameObject);
                    break;*/
        }
    }
}