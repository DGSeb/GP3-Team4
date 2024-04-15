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
    public float jumpCooldown = 0.35f;
    public float airMultiplier;
    public Transform orientation;
    public bool readyToJump;

    // Int that determines how many times player can jump before touching the ground again.
    private int jumpsRemaining = 0;

    // Variables for coyote time and better jumping.
    private float coyoteTime = 0.3f;
    private float coyoteTimeCounter;

    // Jump buffering (allows for jumping slightly before touching the ground.
    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

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
    public static KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    // Controller binds.
    public static KeyCode jumpController = KeyCode.JoystickButton0;
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

    // Variables used for moving platform interaction.
    private Transform originalParent; // Player's parent object.
    private Vector3 platformVelocity;

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

        if (coyoteTimeCounter > 0f && TutorialManager.canDoubleJump)
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

        // If grounded, set the coyote time counter equal to the coyote time (extra time before jump is not registered)
        if (grounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        // If not grounded, let the coyote time counter count down.
        // If the player clicks the jump button before the coyote time counter reaches zero, the player will jump, even if they are slightly in the air.
        // This allows for the player to press the jump key slightly late (slightly after they have already left the ground) and still jump.
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // If the jump key is pressed, set the jump buffer counter to the jumper buffer time
        if (Input.GetKeyDown(jumpKey) || Input.GetKeyDown(jumpController))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        // If the jump key isn't pressed, let the jump buffer counter count down.
        // If the player is close enough to the ground to reset the coyote time counter before the jump buffer counter runs out of time, the player will jump.
        // This allows for the player to press the jump button slightly before they actually touch the ground.
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // when to jump
        if (jumpBufferCounter > 0f && readyToJump)
        {
            // If the player can double jump, set the jump cooldown very low. This prevents double jumping when the player shouldn't be able to.
            // If the player spammed jump quick enough, the readyToJump bool did not turn false quick enough, so they could jump again.
            // This does not matter if the player can double jump, so the cooldown is higher when the player cannot double jump.
            if (TutorialManager.canDoubleJump)
            {
                jumpCooldown = 0.05f;
            }
            else if (!TutorialManager.canDoubleJump)
            {
                jumpCooldown = 0.35f;
            }

            if (jumpsRemaining > 0)
            {
                //Debug.Log("Double: " + jumpCooldown);
                readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }
            else if (coyoteTimeCounter > 0f)
            {
                //Debug.Log("Single: " + jumpCooldown);
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

        // Set jump buffer counter to 0 so player cannot spam jump to jump again.
        jumpBufferCounter = 0f;
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
        if (this.gameObject.transform.position.y < -40)
        {
            gM.ReloadScene();
        }
    }

    // Check what trigger the player walked into.
    private void OnTriggerEnter(Collider other)
    {
        // Check for collided with object's tag.
        switch (other.tag)
        {
            // If the player hit the level 1 portal, send them to level 1.
            case "Level1Portal":
                TutorialComplete("Level1");
                break;

            // If the player hit the level 2 portal, send them to level 2.
            case "Level2Portal":
                TutorialComplete("Level2");
                break;

            // If the player hit the level 3 portal, send them to level 3.
            case "Level3Portal":
                TutorialComplete("Level3");
                break;

            // If level ending is hit, update pb and change scene.
            case "Exit":
                // If the player has eliminated enough enemies, add a leaderboard entry, set the run time for the results screen,
                // check the player's PB, the player won so set that bool to true and make sure lost is false, and change scene.
                if (gM.enoughEnemiesEliminated)
                {
                    // Disable the collider to ensure the function does not run twice.
                    this.gameObject.GetComponent<Collider>().enabled = false;

                    // Add leaderboard entry and update PB if it's better than it was.
                    gM.AddLeaderboardEntry();
                    gM.CheckPB();

                    // Check the current scene to determine which scene to go to.
                    if (gM.currentScene == "Level1")
                    {
                        gM.ChangeScene("Level2");
                    }
                    else if (gM.currentScene == "Level2")
                    {
                        gM.ChangeScene("Level3");
                    }
                    else if (gM.currentScene == "Level3")
                    {
                        gM.SetRunTime();
                        ResultScreen.won = true;
                        ResultScreen.lost = false;
                        gM.ChangeScene("ResultsScreen");
                    }
                }
                break;
        }

        // Check for collided with object's name.
        switch (other.name)
        {
            // If player hits the end object, update PB and go to next scene.
            case "End":
                // Disable the collider to ensure the function does not run twice.
                this.gameObject.GetComponent<Collider>().enabled = false;
                gM.AddLeaderboardEntry();
                gM.CheckPB();
                gM.ChangeScene("LiamsHighlyPsychoticJoint");
                break;

            // If second ending is hit, update pb and change scene.
            case "End2":
                // Disable the collider to ensure the function does not run twice.
                this.gameObject.GetComponent<Collider>().enabled = false;
                gM.AddLeaderboardEntry();
                gM.CheckPB();
                gM.ChangeScene("LiamsWackyWonderland");
                break;

            // If the player runs into the dash trigger, tell them to dash, let them dash, and destroy the trigger.
            // Also, turn off double jump so player can only dash at the next part.
            case "DashTrigger":
                StartCoroutine(tM.SetTextPromptActive(4));
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

            // If player lands in the DashFallZone trigger, transform them back to the point before the dash.
            case "DashFallZone":
                this.gameObject.transform.position = new Vector3(dashCheckpoint.position.x, dashCheckpoint.position.y, dashCheckpoint.position.z);
                break;

            // If player walks into the double jump text trigger, display the text that tells them to use their double jump across the gap and get rid of the trigger.
            case "DoubleJumpTextTrigger":
                StartCoroutine(tM.SetTextPromptActive(3));
                Destroy(other.gameObject);
                break;

            // If the player walks into the trigger that is where they walk into the firing range, turn on the text for entering the firing range. 
            case "WalkIntoFiringRangeTrigger":
                StartCoroutine(tM.SetTextPromptActive(6));
                Destroy(other.gameObject);
                break;
        }
    }

    // Same code is used for tutorial portals, so running a function instead of writing the same lines of code.
    void TutorialComplete(string sceneName)
    {
        // Disable the collider to ensure that the function does not trigger multiple times.
        // There was an issue with this function running twice, so this resolves that issue.
        this.gameObject.GetComponent<Collider>().enabled = false;

        // Set player prefs int. Can't store a bool, so 0 is false and 1 is true. Since player interacted with portal, tutorial complete is true.
        PlayerPrefs.SetInt("TutorialComplete", 1);

        // If the player can double jump and dash, they have cleared most of the tutorial, so
        // add an entry to the leaderboard because why not have one for speedrunning the tutorial. Also, set tutorial PB, then change scene.
        // The if check is added here to ensure that only if the player completes most of the tutorial can they log an entry in the leaderboard or get a better PB.
        // This is necessary because the player can just use the portals to change scenes if they have completed the tutorial before.
        // If they haven't completed the tutorial again, then they shouldn't be able to change their PB or add a leaderboard entry.
        if (TutorialManager.canDoubleJump && TutorialManager.canDash)
        {
            gM.AddLeaderboardEntry();
            gM.CheckPB();
        }
        
        gM.ChangeScene(sceneName);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If the player collides with a moving platform, store the player's parent object, then set the player's new parent to the transform of the moving platform.
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            // Store the player's current parent object.
            originalParent = transform.parent;

            // Store the platforms velocity.
            platformVelocity = collision.gameObject.GetComponent<Rigidbody>().velocity;

            // Add force to the player using the platforms velocity. Also, increase move speed so player can move on platform.
            rb.AddForce(platformVelocity);
            //moveSpeed = 25;

            // Set the platform as the player's parent.
            transform.parent = collision.transform;
            Debug.Log("Attached");
        }
        // If it is a moving platform, only child the player to the platform rather than change the player's velocity.
        else if (collision.gameObject.CompareTag("HorizontalMovingPlatform"))
        {
            Debug.Log("Horizontal moving platform");
            originalParent = transform.parent;
            transform.parent = collision.transform;
            //moveSpeed = 25;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // If the player has exited the moving platform's collider, set the player's parent back to what it was originally.
        if (collision.gameObject.CompareTag("MovingPlatform") || collision.gameObject.CompareTag("HorizontalMovingPlatform"))
        {
            // Set the player's parent make to what it was before touching the moving platform and set the move speed back to what it should be.
            transform.parent = originalParent;
            moveSpeed = desiredMoveSpeed;
            
            Debug.Log("He GONE!");
        }
    }
}