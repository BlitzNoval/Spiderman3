using UnityEngine;

public class RunningCamera : BaseCamera
{
    [Header("Running Camera Settings")]
    public float runDistance = 7.0f;  // Camera distance when running

    protected override void Start()
    {
        base.Start();
        offset = new Vector3(0, heightOffset, -runDistance);
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        HandleRunningCamera();
    }

    private void HandleRunningCamera()
    {
        // Adjust the offset for running distance
        offset.z = Mathf.Lerp(offset.z, -runDistance, Time.deltaTime * smoothSpeed);
        HandleObstacleAvoidance(runDistance);
    }
}
