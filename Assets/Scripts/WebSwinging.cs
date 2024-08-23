using System;
using System.Collections;
using UnityEngine;

public enum PlayerStateAnim
{
    SwingingDownArc,
    SwingingUpArc,
    InAir,
    Falling,
    Walking,
    Idle,
    Run,
    Grounded
}

public class WebSwinging : MonoBehaviour
{
    [Serializable]
    public enum PlayerStatePhysics
    {
        Grounded,
        Aerial,
        Swinging,
        Launching
    }

    public PlayerMovement movement;
    public PlayerStatePhysics currentStatePhysics;
    public bool doGroundedChecks;
    public float jumpHeight;
    public float maxSwingSpeed;
    
    public LineRenderer lineRenderer;
    public LineRenderer visualLineRenderer;
    public Transform handTransform;
    public float swingSpeed = 5.0f;
    public float baseLaunchForce = 5.0f;
    public float gravityScale = 1.0f;
    public float swingPointDistance = 10.0f;
    public float arcSize = 5.0f;
    public float arcCurveFactor = 0.5f;
    public float autoLaunchTime = 1.0f;

    private bool isSwinging = false;
    private bool isLaunching = false;
    private Transform currentSwingPoint;
    private Rigidbody rb;
    private Vector3[] arcPoints;
    private int currentPointIndex = 0;
    private Vector3 swingVelocity = Vector3.zero;
    private Coroutine autoLaunchCoroutine;

    public PlayerStateAnim currentState;
    private Animator animator;
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int inAir = Animator.StringToHash("inAir");
    private static readonly int falling = Animator.StringToHash("falling");
    private static readonly int hasLanded = Animator.StringToHash("hasLanded");

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovement>();
        movement.rb = rb;
        animator = GetComponent<Animator>();
    }

    void Update()
{
    switch (currentStatePhysics)
    {
        case PlayerStatePhysics.Aerial:
            movement.HandlePlayerRotation(); // Should be accessible now
            movement.DoAerialMovement(); // Should be accessible now
            break;
        case PlayerStatePhysics.Grounded:
            if (Input.GetButtonDown("Jump"))
            {
                rb.AddForce(transform.up * jumpHeight, ForceMode.VelocityChange);
            }
            movement.DoGroundedMovement(); // Should be accessible now
            break;
    }

    UpdateState();

    if (Input.GetMouseButtonDown(1) && !isSwinging && !isLaunching)
    {
        FindAndStartSwing();
    }

    if (Input.GetMouseButtonDown(0) && isSwinging && !isLaunching)
    {
        LaunchFromSwing();
    }
}


    private void FixedUpdate()
    {
        switch (currentStatePhysics)
        {
            case PlayerStatePhysics.Aerial:
                rb.velocity = new Vector3(rb.velocity.x * 0.99f, rb.velocity.y, rb.velocity.z * 0.99f);
                break;
            case PlayerStatePhysics.Grounded:
                rb.velocity = new Vector3(rb.velocity.x * 0.9f, rb.velocity.y, rb.velocity.z * 0.9f);
                break;
        }
        if (doGroundedChecks)
        {
            float groundCheckDistance = 0.3f;
            int groundLayer = 1 << LayerMask.NameToLayer("whatIsGround");
            groundLayer |= 1 << LayerMask.NameToLayer("whatIsWall");
            if (Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer))
            {
                currentStatePhysics = PlayerStatePhysics.Grounded;
            }
            else
            {
                currentStatePhysics = PlayerStatePhysics.Aerial;
            }
        }
        
        //Clamping max speed
        rb.velocity = ClampVelocity(rb.velocity, 50f);
    }
    
    Vector3 ClampVelocity(Vector3 velocity, float maxSpeed)
    {
        // If the velocity exceeds the maxSpeed, clamp it
        if (velocity.magnitude > maxSpeed)
        {
            return velocity.normalized * maxSpeed;
        }

        // If the velocity is within the maxSpeed, return it unchanged
        return velocity;
    }

    void FindAndStartSwing()
    {
        currentSwingPoint = GenerateSwingPointInDirection();

        if (currentSwingPoint)
        {
            swingVelocity = rb.velocity;
            StartSwing();
        }
    }

    Transform GenerateSwingPointInDirection()
    {
        Vector3 direction = transform.forward;
        Vector3 spawnPosition = transform.position + direction * swingPointDistance;
        
        //Do a raycast sweep in a cone around the area
        int wallLayer = 1 << LayerMask.NameToLayer("whatIsGround");
        int sweepGranularityX = 5;
        int sweepGranularityY = 4;
        float furthestDistance = Single.MinValue;
        for (int i = 0; i < sweepGranularityY; i++)
        {
            for (int j = 0; j < sweepGranularityX; j++)
            {
                Vector3 xzInfluence = Vector3.Lerp(transform.forward, transform.right, j/(float)(sweepGranularityX*2)).normalized;
                Vector3 yInfluence = transform.up * Mathf.Lerp(0.5f, 2, i / (float)sweepGranularityY - 1);
                Vector3 rayDir = (xzInfluence + yInfluence).normalized;
                RaycastHit hit;
                if (Physics.SphereCast(transform.position, 1f, rayDir, out hit, swingPointDistance*2, wallLayer))
                {
                    float dist = Vector3.Distance(transform.position, hit.point);
                    if (dist > furthestDistance)
                    {
                        spawnPosition = hit.point;
                        furthestDistance = dist;
                    }
                }
            }
        }

        GameObject swingPointObj = new GameObject("SwingPoint");
        swingPointObj.transform.position = spawnPosition;

        SphereCollider collider = swingPointObj.AddComponent<SphereCollider>();
        collider.radius = 0.5f;
        collider.isTrigger = true;

        return swingPointObj.transform;
    }

    void StartSwing()
    {
        //perform swing animation
        int pickSwing = UnityEngine.Random.Range(1, 3);
        animator.SetInteger("Swing", pickSwing);

        CalculateSwingArc();

        lineRenderer.positionCount = arcPoints.Length;
        lineRenderer.SetPositions(arcPoints);
        lineRenderer.enabled = true;

        visualLineRenderer.positionCount = 2;
        visualLineRenderer.SetPosition(0, handTransform.position);
        visualLineRenderer.SetPosition(1, currentSwingPoint.position);
        visualLineRenderer.enabled = true;

        currentPointIndex = 0;
        isSwinging = true;
        
        rb.velocity = swingVelocity;

        StartCoroutine(SwingAlongArc());
    }

    void CalculateSwingArc()
    {
        int pointCount = 50;
        arcPoints = new Vector3[pointCount];
        Vector3 swingPointDir = (currentSwingPoint.position - transform.position).normalized;
        Vector3 start = transform.position;
        Vector3 end = transform.position + swingPointDir * (Vector3.Distance(transform.position, currentSwingPoint.position) * 2);
        Vector3 midpoint = CalculateBezierControlPoint(start, end, rb.velocity);

        /*float velocityMagnitude = rb.velocity.magnitude;
        float velocityFactor = Mathf.Clamp(velocityMagnitude / swingSpeed, 0.5f, 2.0f);
        midpoint.y = Mathf.Min(start.y, end.y) - arcSize * velocityFactor;
        float controlPointHeight = arcSize * arcCurveFactor * velocityFactor;
        midpoint.y += controlPointHeight;*/

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            arcPoints[i] = CalculateQuadraticBezierPoint(t, start, midpoint, end);
        }
    }
    
    Vector3 CalculateBezierControlPoint(Vector3 startPoint, Vector3 endPoint, Vector3 playerVelocity)
    {
        // Find the midpoint between the start and end points
        Vector3 midpoint = (startPoint + endPoint) / 2;

        // Determine the direction from start to end
        Vector3 lineDirection = (endPoint - startPoint).normalized;

        // Project the player's velocity onto the plane perpendicular to the line direction
        Vector3 velocityProjection = Vector3.ProjectOnPlane(playerVelocity, lineDirection).normalized;

        // Determine the control point distance from the midpoint
        float controlPointDistance = Vector3.Distance(startPoint, midpoint);

        // Calculate the control point position
        Vector3 controlPoint = midpoint + velocityProjection * controlPointDistance;

        return controlPoint;
    }

    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return Mathf.Pow(1 - t, 2) * p0 +
               2 * (1 - t) * t * p1 +
               Mathf.Pow(t, 2) * p2;
    }
    
    IEnumerator SwingAlongArc()
    {
        swingSpeed = rb.velocity.magnitude;
        while (isSwinging)
        {
            yield return null;

            if (currentPointIndex < arcPoints.Length - 1)
            {
                float tolerance = 1f;
                Vector3 targetPosition = arcPoints[currentPointIndex];
                Vector3 moveDirection = (targetPosition - transform.position).normalized;

                Vector3 predictedPosition = transform.position + moveDirection * (swingSpeed * Time.deltaTime);

                if (Vector3.Distance(predictedPosition, targetPosition) < Vector3.Distance(transform.position, targetPosition))
                {
                    rb.velocity = moveDirection * swingSpeed;
                }
                else
                {
                    transform.position = targetPosition;
                    rb.velocity = Vector3.zero;
                }

                swingVelocity = rb.velocity;

                if (Vector3.Distance(transform.position, arcPoints[currentPointIndex]) < tolerance)
                {
                    currentPointIndex = Mathf.Min(currentPointIndex + 1, arcPoints.Length - 1);
                    swingSpeed *= 1.01f;
                }

                if (currentSwingPoint)
                {
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, currentSwingPoint.position);

                    visualLineRenderer.SetPosition(0, handTransform.position);
                    visualLineRenderer.SetPosition(1, currentSwingPoint.position);
                }

                if (Vector3.Distance(transform.position, arcPoints[arcPoints.Length - 1]) < tolerance)
                {
                    LaunchFromSwing();
                }
            }
            else
            {
                isSwinging = false;
                rb.velocity = swingVelocity;
                animator.SetInteger("Swing", 4);

                Destroy(currentSwingPoint.gameObject);

                visualLineRenderer.enabled = false;
            }
        }
    }

    void LaunchFromSwing()
    {
        if (isSwinging)
        {
            //removing the reference point
            Destroy(currentSwingPoint.gameObject);
            
            //revert swinging anim
            animator.SetInteger("Swing", 4);
            
            isSwinging = false;
            isLaunching = true;
            lineRenderer.enabled = false;
            visualLineRenderer.enabled = false;

            float t = (float)currentPointIndex / (arcPoints.Length - 1);
            Vector3 launchDirection = rb.velocity.normalized;
            float forwardFactor = Mathf.Lerp(0.5f, 1.0f, t);
            float upwardFactor = Mathf.Lerp(0.5f, 1.0f, 1 - t);

            float launchForce = baseLaunchForce + rb.velocity.magnitude;
            Vector3 force = launchDirection * launchForce * forwardFactor + Vector3.up * launchForce * upwardFactor;

            rb.AddForce(force, ForceMode.Impulse);

            swingVelocity = rb.velocity;

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

    void UpdateState()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (isSwinging)
        {
            float arcProgress = (float)currentPointIndex / (arcPoints.Length - 1);

            if (arcProgress < 0.5f)
            {
                currentState = PlayerStateAnim.SwingingDownArc;
            }
            else
            {
                currentState = PlayerStateAnim.SwingingUpArc;
                animator.SetInteger("Swing", 3);
            }
        }
        else if (isLaunching)
        {
            if (rb.velocity.y > 0)
            {
                currentState = PlayerStateAnim.InAir;
            }
            else if (rb.velocity.y < 0)
            {
                currentState = PlayerStateAnim.Falling;
            }
        }
        else if (IsGrounded())
        {
            if (movement.isMoving)
            {
                currentState = isRunning ? PlayerStateAnim.Run : PlayerStateAnim.Walking;
            }
            else
            {
                //landing animation
                animator.SetTrigger("hasLanded");

                currentState = PlayerStateAnim.Idle;
            }
        }
        else
        {
            currentState = PlayerStateAnim.InAir;
        }

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        float currentSpeed = animator.GetFloat(Speed);
        float targetSpeed = 0f;

        switch (currentState)
        {
            case PlayerStateAnim.Idle:
                targetSpeed = 0f;
                break;
            case PlayerStateAnim.Walking:
                targetSpeed = 0.5f;
                break;
            case PlayerStateAnim.Run:
                targetSpeed = 1f;
                break;
            case PlayerStateAnim.InAir:
                animator.SetTrigger(inAir);
                return;  // Exit early since we don�t want to blend the speed value
            case PlayerStateAnim.Falling:
                animator.SetTrigger(falling);
                return;  // Exit early since we don�t want to blend the speed value
                         // Add other cases for different states as needed
        }

        float smoothSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 1f); // Adjust the 5f value for smoother or faster transitions
        animator.SetFloat(Speed, smoothSpeed);

        if (Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger(Jump);
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.1f);
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
