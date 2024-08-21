using UnityEngine;
using System.Collections;

public class EnhancedSwingJumpController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public LineRenderer visualLineRenderer; // Visual representation LineRenderer
    public Transform handTransform; // Reference to the player's hand
    public float swingSpeed = 5.0f;
    public float baseLaunchForce = 5.0f;
    public float gravityScale = 1.0f;
    public float swingPointDistance = 10.0f; // Distance to spawn swing points
    public float arcSize = 5.0f;  // Controls the height of the arc
    public float arcCurveFactor = 0.5f;  // Controls how curved the arc is
    public float autoLaunchTime = 1.0f;  // Time before auto-launching

    private bool isSwinging = false;
    private bool isLaunching = false;
    private Transform currentSwingPoint;
    private Rigidbody rb;
    private Vector3[] arcPoints;
    private int currentPointIndex = 0;
    private Vector3 swingVelocity = Vector3.zero; // Variable to store swing velocity
    private float holdTime = 0f; // Time the swing button has been held

    public float rotationSpeed = 10.0f; // Speed of player rotation
    private float currentDirection = 0.0f; // Current facing direction in degrees

    private Coroutine autoLaunchCoroutine; // Reference to the auto-launch coroutine

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandlePlayerRotation();

        if (Input.GetMouseButtonDown(1) && !isSwinging && !isLaunching) // Right Click to start swinging
        {
            FindAndStartSwing();
        }

        if (Input.GetMouseButton(1) && isSwinging) // Continue swinging while right click is held
        {
            holdTime += Time.deltaTime;
        }

        if (Input.GetMouseButtonUp(1) && isSwinging) // Left Click to launch
        {
            LaunchFromSwing();
        }
    }

    void HandlePlayerRotation()
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

    void FindAndStartSwing()
    {
        // Clean up the previous swing point if it exists
        if (currentSwingPoint != null)
        {
            Destroy(currentSwingPoint.gameObject);
        }

        currentSwingPoint = GenerateSwingPointInDirection();

        if (currentSwingPoint != null)
        {
            Debug.Log("Swing point created at: " + currentSwingPoint.position);
            StartSwing();
        }
    }

    Transform GenerateSwingPointInDirection()
    {
        // The forward direction is based on the player's current facing direction
        Vector3 direction = transform.forward;
        Vector3 spawnPosition = transform.position + direction * swingPointDistance;

        // Create a new GameObject for the swing point
        GameObject swingPointObj = new GameObject("SwingPoint");
        swingPointObj.transform.position = spawnPosition;

        // Optionally, you can add a visual indicator like a sphere
        SphereCollider collider = swingPointObj.AddComponent<SphereCollider>();
        collider.radius = 0.5f;
        collider.isTrigger = true;

        return swingPointObj.transform;
    }

    void StartSwing()
    {
        CalculateSwingArc();

        lineRenderer.positionCount = arcPoints.Length;
        lineRenderer.SetPositions(arcPoints);
        lineRenderer.enabled = true;

        visualLineRenderer.positionCount = 2; // Set position count for visual line
        visualLineRenderer.SetPosition(0, handTransform.position); // Start position at hand
        visualLineRenderer.SetPosition(1, currentSwingPoint.position); // End position at swing point
        visualLineRenderer.enabled = true;

        currentPointIndex = 0;
        isSwinging = true;

        // Preserve velocity from the previous phase of swinging
        rb.velocity = swingVelocity;
        StartCoroutine(SwingAlongArc());

        // Start the auto-launch coroutine
        if (autoLaunchCoroutine != null)
        {
            StopCoroutine(autoLaunchCoroutine);
        }
        autoLaunchCoroutine = StartCoroutine(AutoLaunchCoroutine());
    }

    void CalculateSwingArc()
    {
        int pointCount = 50;
        arcPoints = new Vector3[pointCount];
        Vector3 start = transform.position;
        Vector3 end = currentSwingPoint.position;
        Vector3 midpoint = (start + end) / 2;

        // Adjust the arc height and curve
        midpoint.y = Mathf.Min(start.y, end.y) - arcSize;
        float controlPointHeight = arcSize * arcCurveFactor;
        midpoint.y += controlPointHeight;

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            arcPoints[i] = CalculateQuadraticBezierPoint(t, start, midpoint, end);
            Debug.Log("Arc Point " + i + ": " + arcPoints[i]);
        }
    }

    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return Mathf.Pow(1 - t, 2) * p0 +
               2 * (1 - t) * t * p1 +
               Mathf.Pow(t, 2) * p2;
    }

    IEnumerator SwingAlongArc()
    {
        while (isSwinging)
        {
            Debug.Log("Swinging along arc");
            // Update the point index based on hold time
            currentPointIndex = Mathf.Clamp((int)(holdTime * (arcPoints.Length - 1) / autoLaunchTime), 0, arcPoints.Length - 1);

            if (currentPointIndex < arcPoints.Length - 1)
            {
                Vector3 targetPosition = arcPoints[currentPointIndex];
                Vector3 moveDirection = (targetPosition - transform.position).normalized;

                rb.velocity = moveDirection * swingSpeed;
                swingVelocity = rb.velocity; // Store the swing velocity

                rb.AddForce(Vector3.down * gravityScale, ForceMode.Acceleration);

                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, currentSwingPoint.position);

                // Update the visual line renderer
                visualLineRenderer.SetPosition(0, handTransform.position);
                visualLineRenderer.SetPosition(1, currentSwingPoint.position);

                yield return null;
            }
            else
            {
                isSwinging = false;
                rb.velocity = swingVelocity; // Ensure velocity is maintained after swing
                rb.velocity = Vector3.zero; // Optional: reset to zero if needed

                // Clean up the swing point object
                Destroy(currentSwingPoint.gameObject);

                // Hide the visual line renderer
                visualLineRenderer.enabled = false;
            }
        }
    }

    void LaunchFromSwing()
    {
        if (isSwinging)
        {
            isSwinging = false;
            isLaunching = true;
            lineRenderer.enabled = false;
            visualLineRenderer.enabled = false; // Hide the visual line renderer

            // Determine the launch force based on swing position
            float t = (float)currentPointIndex / (arcPoints.Length - 1); // Progress in the arc
            Vector3 launchDirection = rb.velocity.normalized;
            float forwardFactor = Mathf.Lerp(0.5f, 1.0f, t); // More forward force at the start
            float upwardFactor = Mathf.Lerp(0.5f, 1.0f, 1 - t); // More upward force at the end

            float launchForce = baseLaunchForce + rb.velocity.magnitude;
            Vector3 force = launchDirection * launchForce * forwardFactor + Vector3.up * launchForce * upwardFactor;

            rb.AddForce(force, ForceMode.Impulse);

            StartCoroutine(ResetLaunchState());
        }
    }

    IEnumerator AutoLaunchCoroutine()
    {
        yield return new WaitForSeconds(autoLaunchTime);

        if (isSwinging)
        {
            LaunchFromSwing();
        }
    }

    IEnumerator ResetLaunchState()
    {
        yield return new WaitForSeconds(0.1f);
        isLaunching = false;
    }

    void OnDrawGizmos()
    {
        if (arcPoints != null && arcPoints.Length > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < arcPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(arcPoints[i], arcPoints[i + 1]);
            }
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.5f);
            if (currentSwingPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(currentSwingPoint.position, 0.5f);
                Gizmos.color = Color.green;
                Vector3 apex = (transform.position + currentSwingPoint.position) / 2;
                apex.y = Mathf.Min(transform.position.y, currentSwingPoint.position.y) - arcSize;
                Gizmos.DrawSphere(apex, 0.5f);
            }
        }
    }
}
