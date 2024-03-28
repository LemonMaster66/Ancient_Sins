using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraFX : MonoBehaviour
{
    public CinemachineVirtualCamera CMvc;
    private CinemachineBasicMultiChannelPerlin CMbmcp;
    private PlayerMovement playerMovement;
    [Space(10)]

    public float TargetDutch;
    private float BlendDutch;

    public float TargetFOV;
    private float BlendFOV;

    private Coroutine shakeCoroutine;

    void Awake()
    {
        //Assign Components
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        CMbmcp = CMvc.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    void Update()
    {
        CMvc.m_Lens.Dutch = Mathf.SmoothDamp(CMvc.m_Lens.Dutch, TargetDutch, ref BlendDutch, 0.1f);
        CMvc.m_Lens.FieldOfView = Mathf.SmoothDamp(CMvc.m_Lens.FieldOfView, TargetFOV, ref BlendFOV, 0.3f);

        TargetDutch = playerMovement.MovementX * -1.5f;
        TargetFOV = 65 + (playerMovement.ForwardVelocityMagnitude / 2);
    }

    public void CameraShake(float amplitude, float frequency, float duration)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeCoroutine(amplitude, frequency, duration));
    }

    private IEnumerator ShakeCoroutine(float amplitude, float frequency, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            CMbmcp.m_AmplitudeGain = Mathf.Lerp(amplitude, 0f, elapsed / duration);
            CMbmcp.m_FrequencyGain = frequency;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset camera shake parameters
        CMbmcp.m_AmplitudeGain = 0f;
        shakeCoroutine = null;
    }
}
