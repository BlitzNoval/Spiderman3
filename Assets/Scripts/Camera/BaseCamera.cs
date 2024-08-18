using UnityEngine;

public abstract class BaseCamera : MonoBehaviour
{
    [Header("General Settings")]
    public Transform target;  // Reference to the player (Spider-Man)
    public float heightOffset = 2.0f;  // Height offset from the player
    public float smoothSpeed = 0.125f;  // Smoothing speed for transitions
    public float rotationSmoothTime = 0.1f;  // Smoothing time for rotation
    public LayerMask obstacleLayers;  // Layers to consider as obstacles

    [Header("Customization")]
    public float distance = 5.0f;  // Distance from the target
    public float rotationAngleX = 15f;  // Angle for rotation on the X-axis (up/down)
    public float rotationAngleY = 0f;  // Angle for rotation on the Y-axis (left/right)
    public float fieldOfView = 60f;  // Field of View (FOV)

    protected Vector3 offset;
    protected Vector3 rotationSmoothVelocity;

    protected virtual void Start()
    {
        offset = new Vector3(0, heightOffset, -distance);
        Camera.main.fieldOfView = fieldOfView;
    }

    protected virtual void LateUpdate()
    {
        UpdateCameraPositionAndRotation();
    }

    protected void UpdateCameraPositionAndRotation()
    {
        // Calculate target position based on offset and rotation angle
        Vector3 targetPosition = target.position + Quaternion.Euler(rotationAngleX, rotationAngleY, 0) * offset;

        // Smoothly transition to the new position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Smoothly adjust rotation to follow the target
        Vector3 direction = target.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime);

        // Handle obstacle avoidance
        HandleObstacleAvoidance(Vector3.Distance(transform.position, target.position));
    }

    protected void HandleObstacleAvoidance(float cameraDistance)
    {
        RaycastHit hit;
        if (Physics.Raycast(target.position, -transform.forward, out hit, cameraDistance, obstacleLayers))
        {
            float hitDistance = hit.distance;
            Vector3 correctedPosition = target.position - transform.forward * hitDistance;
            transform.position = Vector3.Lerp(transform.position, correctedPosition, smoothSpeed);
        }
    }

    // Method to handle height adjustments based on environment
    protected void AdjustHeight(float desiredHeight)
    {
        offset.y = Mathf.Lerp(offset.y, desiredHeight, Time.deltaTime * smoothSpeed);
    }
}
