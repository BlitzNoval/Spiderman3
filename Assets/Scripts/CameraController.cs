using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera cinemachineCam;
    public Transform playerTransform;
    public Volume postProcessingVolume;
    private CinemachineTransposer transposer;
    private PlayerStateAnim currentState;
    private Vignette vignette;
    private float baseOffsetY;
    private float swingArcYOffset = -5f;  // How far below the player the camera should move
    private float shakeIntensity = 2.0f;
    private float shakeDuration = 1.0f;

    private void Start()
    {
        transposer = cinemachineCam.GetCinemachineComponent<CinemachineTransposer>();
        baseOffsetY = transposer.m_FollowOffset.y;

        if (postProcessingVolume.profile.TryGet(out Vignette vignetteEffect))
        {
            vignette = vignetteEffect;
        }
    }

    private void Update()
    {
        // Assuming you have a reference to the player's current state.
        currentState = playerTransform.GetComponent<WebSwinging>().currentState;

        switch (currentState)
        {
            case PlayerStateAnim.SwingingDownArc:
                FollowSwingArc();
                break;
            case PlayerStateAnim.InAir:
                StartCoroutine(HandleInAirEffects());
                break;
            default:
                ResetCamera();
                break;
        }
    }

    private void FollowSwingArc()
    {
        transposer.m_FollowOffset = new Vector3(transposer.m_FollowOffset.x, swingArcYOffset, transposer.m_FollowOffset.z);
    }

    private IEnumerator HandleInAirEffects()
    {
        ApplyScreenShake(shakeIntensity, shakeDuration);
        ApplyVignetteEffect(Color.blue, 0.5f);  // Add blue vignette for the first second

        yield return new WaitForSeconds(1.0f);

        ResetVignetteEffect();  // Reset the vignette effect after the first second
        ResetCamera();          // Return camera to normal position
    }

    private void ApplyScreenShake(float intensity, float duration)
    {
        var perlin = cinemachineCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_AmplitudeGain = intensity;

        StartCoroutine(StopShakeAfterDuration(duration, perlin));
    }

    private IEnumerator StopShakeAfterDuration(float duration, CinemachineBasicMultiChannelPerlin perlin)
    {
        yield return new WaitForSeconds(duration);
        perlin.m_AmplitudeGain = 0f;
    }

    private void ApplyVignetteEffect(Color color, float intensity)
    {
        if (vignette != null)
        {
            vignette.color.value = color;
            vignette.intensity.value = intensity;
        }
    }

    private void ResetVignetteEffect()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0f;  // Reset intensity to zero
        }
    }

    private void ResetCamera()
    {
        transposer.m_FollowOffset = new Vector3(transposer.m_FollowOffset.x, baseOffsetY, transposer.m_FollowOffset.z);
    }
}
