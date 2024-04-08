using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraFX : MonoBehaviour
{
    public CinemachineVirtualCamera CMvc;
    public CinemachineBasicMultiChannelPerlin CMbmcp;
    public CinemachineImpulseSource CMis;
    private PlayerMovement playerMovement;

    [Space(10)]

    public float TargetDutch;
    private float BlendDutch;

    public float TargetFOV;
    private float BlendFOV;

    public float TargetShakeAmplitude;
    private float BlendShakeAmplitude;
    public float TargetShakeFrequency;
    private float BlendShakeFrequency;    

    void Awake()
    {
        //Assign Components
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        CMbmcp = CMvc.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        CMis = playerMovement.GetComponentInChildren<CinemachineImpulseSource>();
    }

    void Update()
    {
        CMvc.m_Lens.Dutch = Mathf.SmoothDamp(CMvc.m_Lens.Dutch, TargetDutch, ref BlendDutch, 0.1f);
        CMvc.m_Lens.FieldOfView = Mathf.SmoothDamp(CMvc.m_Lens.FieldOfView, TargetFOV, ref BlendFOV, 0.2f);

        CMbmcp.m_AmplitudeGain = Mathf.SmoothDamp(CMbmcp.m_AmplitudeGain, TargetShakeAmplitude, ref BlendShakeAmplitude, 0.1f);
        CMbmcp.m_FrequencyGain = Mathf.SmoothDamp(CMbmcp.m_FrequencyGain, TargetShakeFrequency, ref BlendShakeFrequency, 0.1f);

        TargetDutch = playerMovement.MovementX * -1.5f;
        TargetFOV = 60 + (playerMovement.ForwardVelocityMagnitude / 1.5f);

        //Footsteps
        if(playerMovement.WalkingCheck())
        {
            if(playerMovement.Running)
            {
                TargetShakeAmplitude = 4f;
                TargetShakeFrequency = 0.05f;
            }
            else
            {
                TargetShakeAmplitude = 2f;
                TargetShakeFrequency = 0.04f;
            }
        }
        else
        {
            TargetShakeAmplitude = 0f;
            TargetShakeFrequency = 0f;
        }
    }
}
