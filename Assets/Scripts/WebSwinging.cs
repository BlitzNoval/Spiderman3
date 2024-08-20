using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnhancedSwingJumpController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float swingSpeed = 5.0f;
    public float baseLaunchForce = 5.0f;
    public float gravityScale = 1.0f;
    public KeyCode swingKey = KeyCode.Space;
    public List<Transform> swingPoints;

    // New public variables for arc control
    public float arcSize = 5.0f;  // Controls the height of the arc
    public float arcCurveFactor = 0.5f;  // Controls how curved the arc is

    private bool isSwinging = false;
    private bool isLaunching = false;
    private bool swingForward = true; // Direction flag
    private int currentSwingIndex = 0;
    private Transform currentSwingPoint;
    private Rigidbody rb;
    private Vector3[] arcPoints;
    private int currentPointIndex = 0;
    private Vector3 previousVelocity = Vector3.zero;
    private Vector3 swingVelocity = Vector3.zero; // New variable to store swing velocity

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(swingKey) && !isSwinging && !isLaunching)
        {
            FindAndStartSwing();
        }

        if (isSwinging && Input.GetKeyUp(swingKey))
        {
            LaunchFromSwing();
        }
    }

    void FindAndStartSwing()
    {
        currentSwingPoint = FindNextSwingPoint();

        if (currentSwingPoint != null)
        {
            StartSwing();
        }
    }

    Transform FindNextSwingPoint()
    {
        if (swingPoints.Count == 0)
            return null;

        // Determine next swing point based on direction
        if (swingForward)
        {
            if (currentSwingIndex >= swingPoints.Count)
            {
                swingForward = false;
                currentSwingIndex = swingPoints.Count - 2; // Start moving backward
            }
        }
        else
        {
            if (currentSwingIndex < 0)
            {
                swingForward = true;
                currentSwingIndex = 1; // Start moving forward
            }
        }

        Transform nextPoint = swingPoints[currentSwingIndex];
        currentSwingIndex += swingForward ? 1 : -1;

        return nextPoint;
    }

    void StartSwing()
    {
        CalculateSwingArc();

        lineRenderer.positionCount = arcPoints.Length;
        lineRenderer.SetPositions(arcPoints);
        lineRenderer.enabled = true;

        currentPointIndex = 0;
        isSwinging = true;

        // Preserve velocity from the previous phase of swinging
        rb.velocity = swingVelocity;
        StartCoroutine(SwingAlongArc());
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
            if (currentPointIndex < arcPoints.Length - 1)
            {
                Vector3 targetPosition = arcPoints[currentPointIndex];
                Vector3 moveDirection = (targetPosition - transform.position).normalized;

                rb.velocity = moveDirection * swingSpeed;
                swingVelocity = rb.velocity; // Store the swing velocity

                currentPointIndex = Mathf.Min(currentPointIndex + 1, arcPoints.Length - 1);

                rb.AddForce(Vector3.down * gravityScale, ForceMode.Acceleration);

                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, currentSwingPoint.position);

                yield return null;
            }
            else
            {
                isSwinging = false;
                rb.velocity = swingVelocity; // Ensure velocity is maintained after swing
                rb.velocity = Vector3.zero; // Optional: reset to zero if needed
            }
        }
    }

    void LaunchFromSwing()
    {
        isSwinging = false;
        isLaunching = true;
        lineRenderer.enabled = false;

        Vector3 launchDirection = rb.velocity.normalized;
        float launchForce = baseLaunchForce + rb.velocity.magnitude;

        rb.AddForce(launchDirection * launchForce, ForceMode.Impulse);

        previousVelocity = rb.velocity;

        StartCoroutine(ResetLaunchState());
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
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(currentSwingPoint.position, 0.5f);
            Gizmos.color = Color.green;
            Vector3 apex = (transform.position + currentSwingPoint.position) / 2;
            apex.y = Mathf.Min(transform.position.y, currentSwingPoint.position.y) - arcSize;
            Gizmos.DrawSphere(apex, 0.5f);
        }

        Gizmos.color = Color.blue;
        foreach (Transform point in swingPoints)
        {
            Gizmos.DrawSphere(point.position, 0.3f);
        }
    }
}
