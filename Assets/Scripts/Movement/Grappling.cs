using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("Referneces")]
    
    private PlayerMovement pm;
    public Transform cam;
    public Transform gunTip;
    public Transform grappleGun;
    public LayerMask whatisGrappleable;
    public LineRenderer lr;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;

    private bool grappling;

    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        gunTip.rotation = cam.rotation;
        grappleGun.rotation = cam.rotation;

         if(Input.GetKeyDown(grappleKey))
        {
            StartGrapple();
        }

         if(grapplingCdTimer > 0 )
        {
            grapplingCdTimer -= Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if(grappling)
        {
            lr.SetPosition(0, gunTip.position);
        }
    }

    private void StartGrapple()
    {

        if(grapplingCdTimer > 0)
        {
            return;
        }
        GetComponent<Swinging1>().StopSwing();

        grappling = true;

        pm.freeze = true;

        RaycastHit hit;
        if (Physics.Raycast(gunTip.position, gunTip.forward, out hit, maxGrappleDistance, whatisGrappleable))
        {
            grapplePoint = hit.point;

            Invoke(nameof(ExecutetGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = gunTip.position + gunTip.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);

        }
        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);

    }
    private void ExecutetGrapple()
    {
        pm.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnAcr = grapplePointRelativeYPos + overshootYAxis;

        if(grapplePointRelativeYPos < 0)
        {
            highestPointOnAcr = overshootYAxis;
        }

        pm.JumpToPosition(grapplePoint, highestPointOnAcr);

        Invoke(nameof(StopGrapple), 1f);
        
    }
    public void StopGrapple()
    {
        pm.freeze = false;
        
        grappling = false;

        grapplingCdTimer = grapplingCd;

        lr.enabled = false;
    }


}
