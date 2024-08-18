using UnityEngine;

public class WalkingCamera : BaseCamera
{
    [Header("Walking Camera Settings")]
    public float walkDistance = 5.0f;  // Camera distance when walking

    protected override void Start()
    {
        base.Start();
        offset = new Vector3(0, heightOffset, -walkDistance);
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        HandleWalkingCamera();
    }

    private void HandleWalkingCamera()
    {
        // Adjust the offset for walking distance
        offset.z = Mathf.Lerp(offset.z, -walkDistance, Time.deltaTime * smoothSpeed);
        HandleObstacleAvoidance(walkDistance);
    }
}
