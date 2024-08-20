using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnhancedSwingJumpController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float swingRadius = 10.0f;
    public float swingDuration = 2.0f;
    public float baseLaunchForce = 5.0f;
    public KeyCode swingKey = KeyCode.Space;
    public KeyCode jumpKey = KeyCode.W;

    private bool isSwinging = false;
    private Vector3 imaginaryPoint;
    private Vector3[] arcPoints;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(swingKey) && !isSwinging)
        {
            StartSwing();
        }

        if (isSwinging && Input.GetKeyDown(jumpKey))
        {
            LaunchFromSwing();
        }
    }

    void StartSwing()
    {
        GenerateImaginaryPoint();
        CalculateSwingArc();
        lineRenderer.positionCount = arcPoints.Length;
        lineRenderer.SetPositions(arcPoints);
        lineRenderer.enabled = true;
        StartCoroutine(SwingAlongArc());
    }

    void GenerateImaginaryPoint()
    {
        imaginaryPoint = transform.position + transform.forward * swingRadius;
        imaginaryPoint.y += swingRadius;
    }

    void CalculateSwingArc()
    {
        int pointCount = 50;
        arcPoints = new Vector3[pointCount];
        Vector3 start = transform.position;
        Vector3 end = imaginaryPoint;
        Vector3 midpoint = (start + end) / 2;
        midpoint.y = Mathf.Min(start.y, end.y) - 5.0f;

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
        isSwinging = true;
        float timeElapsed = 0f;
        int pointCount = arcPoints.Length;

        while (timeElapsed < swingDuration)
        {
            float t = timeElapsed / swingDuration;
            int currentPointIndex = Mathf.FloorToInt(t * (pointCount - 1));

            Vector3 targetPosition = arcPoints[currentPointIndex];
            Vector3 moveDirection = (targetPosition - transform.position).normalized;

            // Apply force to move towards the arc point
            rb.velocity = moveDirection * (swingRadius / swingDuration);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        rb.position = imaginaryPoint;
        LaunchFromSwing();

        lineRenderer.enabled = false;
        isSwinging = false;
    }

    void LaunchFromSwing()
    {
        float jumpForce = baseLaunchForce + rb.velocity.magnitude;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
            Gizmos.DrawSphere(transform.position, 0.5f); // Start point
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(imaginaryPoint, 0.5f); // End point
            Gizmos.color = Color.green;
            Vector3 apex = (transform.position + imaginaryPoint) / 2;
            apex.y = Mathf.Min(transform.position.y, imaginaryPoint.y) - 5.0f;
            Gizmos.DrawSphere(apex, 0.5f); // Apex of the arc
        }
    }
}