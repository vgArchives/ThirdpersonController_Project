using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController playerController;
    private Animator animator;
    private Transform cam;

    [Header("Move Settings")]
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float turnSmoothTime = 0.2f;
    private float moveSpeed = 2f;
    private float runSpeed = 6f;      
    private float turnSmoothVelocity;
    private float targetAngle = 0.0f;

    [Header("Gravity Settings")]
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    private float verticalVelocity;
    

    [Header("Ground Settings")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool grounded = true;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask groundLayers;
    private float groundedRadius = 0.3f; // this value should be the same as the character controller radius



    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void Move()
    {
        // collects the player input
        float horizontalX = Input.GetAxisRaw("Horizontal");
        float verticalZ = Input.GetAxisRaw("Vertical");
        // the direction value is normalized to maintain the same speed if the player is going diagonal
        Vector3 direction = new Vector3(horizontalX, 0f, verticalZ).normalized;

        // checks if the player is holding the run button
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;

        if (direction == Vector3.zero) targetSpeed = 0.0f;

        if (direction.magnitude >= 0.1f)
        {
            // directs the player to where the camera is being pointed
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        // move the player
        Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        playerController.Move(moveDirection.normalized * (targetSpeed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

        // set the animator parameter equals to the player's current speed
        animator.SetFloat("InputMagnitude", targetSpeed, 0.15f, Time.deltaTime);
    }

    private void GroundedCheck()
    {
        // position of the sphere who checks the ground
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers);
    }

    private void JumpAndGravity()
    {
        if(grounded)
        {
            // stop our velocity dropping infinitely when grounded
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
            }

            // Jump
            if(Input.GetButtonDown("Jump"))
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                animator.SetTrigger("Jump");

            }
        }

        //apply gravity over time (multiply by delta time twice to linearly speed up over time)
        verticalVelocity += gravity * Time.deltaTime;
    }
}

