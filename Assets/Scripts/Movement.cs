using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public WebSwinging swingController;
    public Rigidbody rb;
    public float moveSpeed;
    public float aerialMoveSpeed;
    public float rotationSpeed;
    public bool isMoving;  // Make this public
    private float currentDirection = 0.0f;
    private Animator animator;
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Jump = Animator.StringToHash("Jump");

    private void Start()
    {
        swingController = GetComponent<WebSwinging>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleMovement(); // Update call
    }

    public void HandleMovement()  // Make this public
    {
        if (swingController.currentStatePhysics == WebSwinging.PlayerStatePhysics.Aerial)
        {
            DoAerialMovement();
        }
        else if (swingController.currentStatePhysics == WebSwinging.PlayerStatePhysics.Grounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                animator.SetTrigger(Jump); // Trigger jump animation
                rb.AddForce(transform.up * moveSpeed, ForceMode.VelocityChange); // Apply jump force
            }
            DoGroundedMovement();
        }

        isMoving = rb.velocity.magnitude > 0.1f;
    }

    public void DoAerialMovement()  // Make this public
    {
        float verticalInput = Input.GetAxis("Vertical");
        if (verticalInput != 0)
        {
            rb.AddForce(transform.forward * verticalInput * aerialMoveSpeed, ForceMode.Acceleration);
        }
    }

    public void DoGroundedMovement()  // Make this public
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        rb.AddForce(move * moveSpeed, ForceMode.VelocityChange);

        float adjustedSpeed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? moveSpeed * 2 : moveSpeed;
        rb.AddForce(move * adjustedSpeed, ForceMode.VelocityChange);

        isMoving = move.magnitude > 0.1f;
    }

    public void HandlePlayerRotation() // Make this public
    {
        if (Input.GetKey(KeyCode.A))
        {
            currentDirection -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            currentDirection += rotationSpeed * Time.deltaTime;
        }

        Quaternion targetRotation = Quaternion.Euler(0, currentDirection, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void UpdateAnimation()
    {
        float speed = rb.velocity.magnitude;
        animator.SetFloat(Speed, isMoving ? speed : 0f);

        // Handle the jump trigger
        if (Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger(Jump);
        }
    }
}








  /*
        if (move.x != 0)
        {
            Quaternion targetRotation = Quaternion.Euler(0, move.x>0?currentDirection += rotationSpeed * Time.deltaTime:currentDirection -= rotationSpeed * Time.deltaTime, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }*/