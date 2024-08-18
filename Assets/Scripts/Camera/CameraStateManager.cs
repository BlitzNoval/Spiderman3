using UnityEngine;

public class CameraStateManager : MonoBehaviour
{
    public WalkingCamera walkingCamera;
    public RunningCamera runningCamera;

    [Header("Test Keybindings")]
    public KeyCode walkKey = KeyCode.Alpha1;
    public KeyCode runKey = KeyCode.Alpha2;

    private BaseCamera activeCamera;

    private void Start()
    {
        SetActiveCamera(walkingCamera);  // Start with the walking camera
    }

    private void Update()
    {
        if (Input.GetKeyDown(walkKey))
        {
            SetActiveCamera(walkingCamera);
        }
        else if (Input.GetKeyDown(runKey))
        {
            SetActiveCamera(runningCamera);
        }
    }

    private void SetActiveCamera(BaseCamera camera)
    {
        if (activeCamera != null)
        {
            activeCamera.enabled = false;
        }
        activeCamera = camera;
        activeCamera.enabled = true;
    }
}
