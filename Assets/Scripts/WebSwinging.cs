using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeSwingMovement : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform handTransform;  // Reference to the hand transform
    public float swingRadius = 30.0f;  // The radius of the swing arc (increased to 30)
    public float duration = 2.0f;  // Total time to follow the arc
    public float launchForce = 5.0f;  // The force to apply after swinging
    public float upwardLaunchForce = 3.0f;  // The upward force to apply after swinging
    public KeyCode activationKey = KeyCode.Space;  // Key to activate the swing

    private bool isSwinging = false;  // To check if the player is currently swinging
    private Vector3 imaginaryPoint;   // The temporary swing point
    private Vector3[] arcPoints;
    private float t;
    private CharacterController characterController;
    private Vector3 momentum;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Check if the activation key is pressed and the player isn't already swinging
        if (Input.GetKeyDown(activationKey) && !isSwinging)
        {
            // Generate the imaginary swing point using the hand transform
            GenerateImaginaryPoint();

            // Calculate the arc and prepare for the swing
            CalculateLowArc();

            // Draw the line from hand to the imaginary point (for attachment visualization)
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, handTransform.position);
            lineRenderer.SetPosition(1, imaginaryPoint);
            lineRenderer.enabled = true;

            // Start the swinging process
            StartCoroutine(SwingAlongArc());
        }

        // Update the LineRenderer to follow the hand position during the swing
        if (isSwinging)
        {
            lineRenderer.SetPosition(0, handTransform.position);
        }

        // Apply momentum
        if (!isSwinging && momentum.magnitude > 0.1f)
        {
            characterController.Move(momentum * Time.deltaTime);
            momentum *= 0.98f; // Gradually reduce momentum
        }
    }

    void GenerateImaginaryPoint()
    {
        // Generate a point at a fixed distance directly in front of the hand
        // You can customize this to generate the point based on different criteria
        imaginaryPoint = handTransform.position + handTransform.forward * swingRadius;
        imaginaryPoint.y += swingRadius; // Adjust the height to create an arc
    }

    void CalculateLowArc()
    {
        // Number of points in the arc
        int pointCount = 50;
        arcPoints = new Vector3[pointCount];

        Vector3 start = handTransform.position;
        Vector3 end = imaginaryPoint;

        // Midpoint directly below the line between start and end
        Vector3 midpoint = (start + end) / 2;
        midpoint.y = Mathf.Min(start.y, end.y) - 5.0f;  // Adjust the height for the "low" arc

        // Calculate the arc points
        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            arcPoints[i] = CalculateQuadraticBezierPoint(t, start, midpoint, end);
        }
    }

    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // Quadratic Bezier formula: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
        return Mathf.Pow(1 - t, 2) * p0 +
               2 * (1 - t) * t * p1 +
               Mathf.Pow(t, 2) * p2;
    }

    IEnumerator SwingAlongArc()
    {
        isSwinging = true;
        float timeElapsed = 0f;
        int pointCount = arcPoints.Length;

        // Follow the arc for the specified duration
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            int currentPointIndex = Mathf.FloorToInt(t * (pointCount - 1));

            // Move the player along the arc
            transform.position = arcPoints[currentPointIndex];

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position is exactly the endpoint
        transform.position = imaginaryPoint;

        // Calculate launch direction and apply momentum
        Vector3 launchDirection = (imaginaryPoint - arcPoints[arcPoints.Length - 2]).normalized;
        LaunchPlayerForward(launchDirection);

        // Hide the LineRenderer after the movement is done
        lineRenderer.enabled = false;
        isSwinging = false;
    }

    void LaunchPlayerForward(Vector3 direction)
    {
        // Calculate the launch vector with an upward component
        Vector3 launchVector = direction * launchForce + Vector3.up * upwardLaunchForce;
        
        // Set the initial momentum
        momentum = launchVector;
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
        }
    }
}
