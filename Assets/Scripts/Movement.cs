using System;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    public WebSwinging swingController;
    public Rigidbody rb;
    public float moveSpeed;
    public float aerialMoveSpeed;
    public float rotationSpeed;

    private void Start()
    {
        swingController = GetComponent<WebSwinging>();
    }

    public void DoAerialMovement()
    {
        float verticalInput = Input.GetAxis("Vertical");
        if (verticalInput != 0)
        {
            rb.AddForce(transform.forward*verticalInput*aerialMoveSpeed, ForceMode.Acceleration);
        }
    }

    public void DoGroundedMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        /*rb.AddForce(move*moveSpeed, ForceMode.VelocityChange);
        if (move.x != 0)
        {
            Quaternion targetRotation = Quaternion.Euler(0, move.x>0?currentDirection += rotationSpeed * Time.deltaTime:currentDirection -= rotationSpeed * Time.deltaTime, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }*/
    }

    public void HandlePlayerRotation(float currentDirection)
    {
        if (Input.GetKey(KeyCode.A))
        {
            // Rotate player left
            currentDirection -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            // Rotate player right
            currentDirection += rotationSpeed * Time.deltaTime;
        }

        // Update player orientation based on currentDirection
        Quaternion targetRotation = Quaternion.Euler(0, currentDirection, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
