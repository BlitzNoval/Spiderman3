using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 10f;
    public float jumpForce = 5f;
    public bool isGrounded = false;

    private Rigidbody rb;

    void Start()
    {
        // Get the Rigidbody component attached to the character
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Handle rotation based on horizontal input (mouse or arrow keys)
        float horizontal = Input.GetAxis("Horizontal");
        Vector3 rotation = new Vector3(0f, horizontal * turnSpeed * Time.deltaTime, 0f);
        transform.Rotate(rotation);

        // Handle movement based on vertical input (WASD or arrow keys)
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = transform.forward * vertical * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + moveDirection);

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the player is grounded (on the floor)
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Check if the player left the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
