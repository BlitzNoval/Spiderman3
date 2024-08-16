using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class CameraEventSystem : MonoBehaviour
{
    [System.Serializable]
    public class CameraEvent
    {
        public string eventName; // Name of the camera event
        public List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>(); // List of cameras in this event
        public List<TransitionType> transitionTypes = new List<TransitionType>(); // Transition types for each camera
        public List<float> cameraDurations = new List<float>(); // Duration for each camera
        public List<float> transitionDurations = new List<float>(); // Transition duration for blend, zoom, etc.
    }

    public List<CameraEvent> cameraEvents = new List<CameraEvent>(); // List of camera events
    public Button testButton; // Button to test the camera event
    public List<Button> eventButtons; // Buttons to trigger specific events

    public enum TransitionType
    {
        Snap,
        Cut,
        Blend,
        Fade,
        Zoom,
        Shake
    }

    private bool isSwitching = false; // Track if a camera event is active

    private void Start()
    {
        if (testButton != null)
        {
            testButton.onClick.AddListener(() => StartCameraEvent(0)); // Test the first event by default
        }

        for (int i = 0; i < eventButtons.Count; i++)
        {
            int index = i; // Capture the loop variable
            eventButtons[i].onClick.AddListener(() => StartCameraEvent(index));
        }

        // Trigger setup can go here, example for later:
        // private void OnTriggerEnter(Collider other)
        // {
        //     if (other.CompareTag("Player")) 
        //     {
        //         StartCameraEvent(1); // Start event 1 on trigger
        //     }
        // }
    }

    public void StartCameraEvent(int eventIndex)
    {
        if (!isSwitching && eventIndex < cameraEvents.Count)
        {
            StartCoroutine(SwitchCameras(eventIndex));
        }
    }

    private IEnumerator SwitchCameras(int eventIndex)
    {
        isSwitching = true;
        CameraEvent cameraEvent = cameraEvents[eventIndex];

        for (int i = 0; i < cameraEvent.cameras.Count; i++)
        {
            ApplyTransition(cameraEvent, i);

            Debug.Log($"Event: {cameraEvent.eventName}, Switching to Camera {i} (\"{cameraEvent.cameras[i].name}\") using {cameraEvent.transitionTypes[i]} transition for {cameraEvent.cameraDurations[i]} seconds.");

            yield return new WaitForSeconds(cameraEvent.cameraDurations[i]);
        }

        ResetToMainCamera();
        isSwitching = false;
    }

    private void ApplyTransition(CameraEvent cameraEvent, int cameraIndex)
    {
        CinemachineVirtualCamera currentCamera = cameraEvent.cameras[cameraIndex];
        TransitionType transition = cameraEvent.transitionTypes[cameraIndex];
        float transitionDuration = cameraEvent.transitionDurations[cameraIndex];

        foreach (CinemachineVirtualCamera cam in cameraEvent.cameras)
        {
            cam.Priority = 0;
        }

        switch (transition)
        {
            case TransitionType.Snap:
                currentCamera.Priority = 10;
                break;
            case TransitionType.Cut:
                currentCamera.Priority = 10;
                SetCinemachineBlendStyle(CinemachineBlendDefinition.Style.Cut);
                break;
            case TransitionType.Blend:
                currentCamera.Priority = 10;
                SetCinemachineBlendStyle(CinemachineBlendDefinition.Style.EaseInOut, transitionDuration);
                break;
            case TransitionType.Fade:
                StartCoroutine(FadeCamera(currentCamera, transitionDuration));
                break;
            case TransitionType.Zoom:
                StartCoroutine(ZoomCamera(currentCamera, transitionDuration));
                break;
            case TransitionType.Shake:
                StartCoroutine(ShakeCamera(currentCamera, transitionDuration));
                break;
        }
    }

    private void ResetToMainCamera()
    {
        // Deactivate all cameras to revert to the main camera
        foreach (var cameraEvent in cameraEvents)
        {
            foreach (var cam in cameraEvent.cameras)
            {
                cam.Priority = 0;
            }
        }

        Debug.Log("Returning to Main Camera.");
    }

    private void SetCinemachineBlendStyle(CinemachineBlendDefinition.Style style, float duration = 0f)
    {
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        brain.m_DefaultBlend.m_Style = style;
        brain.m_DefaultBlend.m_Time = duration;
    }

    private IEnumerator FadeCamera(CinemachineVirtualCamera camera, float duration)
    {
        // Example fade effect logic (fade in)
        // Implement the actual fade effect here, possibly using a Canvas Group or a shader
        camera.Priority = 10;
        Debug.Log("Fading in camera.");
        yield return new WaitForSeconds(duration);
    }

    private IEnumerator ZoomCamera(CinemachineVirtualCamera camera, float duration)
    {
        // Example zoom effect logic
        camera.Priority = 10;
        CinemachineComponentBase component = camera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        if (component is CinemachineFramingTransposer framing)
        {
            float initialDistance = framing.m_CameraDistance;
            float targetDistance = initialDistance / 2; // Example zoom value
            float elapsed = 0f;

            while (elapsed < duration)
            {
                framing.m_CameraDistance = Mathf.Lerp(initialDistance, targetDistance, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            framing.m_CameraDistance = targetDistance;
        }
    }

    private IEnumerator ShakeCamera(CinemachineVirtualCamera camera, float duration)
    {
        // Example shake effect logic
        camera.Priority = 10;
        var noise = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise != null)
        {
            float originalAmplitude = noise.m_AmplitudeGain;
            noise.m_AmplitudeGain = 1f; // Example shake strength
            yield return new WaitForSeconds(duration);
            noise.m_AmplitudeGain = originalAmplitude;
        }
    }
}
