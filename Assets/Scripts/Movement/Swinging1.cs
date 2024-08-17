using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swinging1 : MonoBehaviour
{
    [Header("Prediction")]
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;


    [Header("OdmGear")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;


    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;

    [Header("References")]
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask whatIsGrappleable;
    public PlayerMovement pm;

    [Header("Swinging")]
    private float maxSwingDistance = 30f;
    private Vector3 swingPoint;
    private SpringJoint joint;


    private void Update()
    {
        if(Input.GetKeyDown(swingKey))
        {
            StartSwing();
            DrawRope();

        }
        if (Input.GetKeyUp(swingKey))
        {
            StopSwing();
        }

        CheckForSwingPoints();

        if(joint != null)
        {
            OdmMovement();
        }

    }

    private void StartSwing()
    { 
        if(predictionHit.point == Vector3.zero)
        {
            return;
        }

        if(GetComponent<Grappling>() != null)
        {
            GetComponent<Grappling>().StopGrapple();
        }
        pm.ResetRestrictions();

        pm.swinging = true;
        
        swingPoint = predictionHit.point;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

        //distance from grapple point
        joint.maxDistance = distanceFromPoint * 100f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = 6.5f;
        joint.damper = 9f;
        joint.massScale = 6.5f;

        lr.positionCount = 2;
        currentGrapplePosition = gunTip.position;
            
    }

    public void StopSwing()
    {
        pm.swinging = false;
        
        lr.positionCount = 0;
        Destroy(joint);
    }

    private Vector3 currentGrapplePosition;
    void DrawRope()
    {
        //if not grappling, dont draw
        if(!joint)
        {
            return;
        }

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);

        Debug.Log("Drawing rope from " + gunTip.position + " to " + currentGrapplePosition);

    }

    private void OdmMovement()
    {
        //right
        if(Input.GetKey(KeyCode.D))
        {
            rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        }
        //Left
        if(Input.GetKey(KeyCode.A))
        {
            rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        }
        //Forward
        if(Input.GetKey(KeyCode.W))
        {
            rb.AddForce(orientation.forward * forwardThrustForce * Time.deltaTime);
        }
        //ShortenCable
        if(Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = swingPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distaneFromPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distaneFromPoint * 0.8f;
            joint.minDistance = distaneFromPoint * 0.25f;
        }

        //extend cable
        if(Input.GetKey(KeyCode.S))
        {
            float extenedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

            joint.maxDistance = extenedDistanceFromPoint * 0.8f;
            joint.minDistance = extenedDistanceFromPoint * 0.25f;
        }
    }

    private void CheckForSwingPoints()
    {
        if(joint != null)
        {
            return;
        }

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward, out sphereCastHit, maxSwingDistance, whatIsGrappleable);
        
        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward, out raycastHit, maxSwingDistance, whatIsGrappleable);

        Vector3 realHitPoint;

        //direct
        if(raycastHit.point !=Vector3.zero)
        {
            realHitPoint = raycastHit.point;
        }
        
        //indirect
        else if(sphereCastHit.point !=Vector3.zero)
        {
            realHitPoint = sphereCastHit.point;
        }
        
        //miss
        else
        {
            realHitPoint = Vector3.zero;
        }

        //real hit point
        if(realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }

        //real hit point not found
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }
}
