using UnityEngine;

public class WebSwing : MonoBehaviour
{
    [Header("Web Settings")]
    public Transform rightHand; // The right hand transform (assign in the Inspector)
    public Transform leftHand; // The left hand transform (assign in the Inspector)
    public float maxSwingDistance = 20.0f; // Maximum distance for swinging
    public GameObject swingTargetIndicator; // Visual indicator for valid swing targets
    public LayerMask swingableLayer; // Layer mask to define what is "swingable"

    private LineRenderer lineRenderer; // LineRenderer to visualize the web
    private bool useRightHand = true; // Start with the right hand
    private bool isAiming = false; // Are we currently aiming?
    private Transform currentSwingTarget; // The current swing target object

    private void Start()
    {
        // Initialize and configure the LineRenderer component
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.positionCount = 2;

        // Ensure the swing target indicator is initially inactive
        if (swingTargetIndicator != null)
        {
            swingTargetIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        // Handle aiming and targeting
        HandleAiming();

        // Check for the web spawn key (e.g., "E") to attach to the target
        if (Input.GetKeyDown(KeyCode.E) && isAiming && currentSwingTarget != null)
        {
            AttachWebToTarget();
        }
    }

    private void HandleAiming()
    {
        // Perform a sphere cast to find the nearest valid swingable object within range
        RaycastHit hit;
        Vector3 direction = transform.forward;

        if (Physics.SphereCast(transform.position, 1f, direction, out hit, maxSwingDistance, swingableLayer))
        {
            // Valid target found on the swingable layer
            isAiming = true;
            currentSwingTarget = hit.transform;

            // Visualize the swing target indicator
            if (swingTargetIndicator != null)
            {
                swingTargetIndicator.SetActive(true);
                swingTargetIndicator.transform.position = hit.point;
            }

            return;
        }

        // If no valid target found, disable aiming and indicator
        isAiming = false;
        currentSwingTarget = null;
        if (swingTargetIndicator != null)
        {
            swingTargetIndicator.SetActive(false);
        }
    }

    private void AttachWebToTarget()
    {
        // Calculate the web point position based on the selected hand
        Vector3 startPoint = useRightHand ? rightHand.position : leftHand.position;
        Vector3 endPoint = currentSwingTarget.position;

        // Set the positions for the line renderer
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        // Switch hands for the next web
        useRightHand = !useRightHand;

        // Here, you can implement the logic for the actual swinging movement, 
        // like applying forces or constraining the player's movement to the web's direction.
    }

    private void OnDrawGizmos()
    {
        // Draw the web lines in the Scene view for better visualization
        if (currentSwingTarget != null)
        {
            Vector3 startPoint = useRightHand ? rightHand.position : leftHand.position;
            Vector3 endPoint = currentSwingTarget.position;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(startPoint, endPoint);
        }
    }
}