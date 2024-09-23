using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using PalexUtilities;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

public class CameraFX : MonoBehaviour
{
    public float TargetDutch;
    private float BlendDutch;

    public float TargetFOV;
    private float BlendFOV;

    public float TargetShakeAmplitude;
    private float BlendShakeAmplitude;
    public float TargetShakeFrequency;
    private float BlendShakeFrequency;

    [Space(10)]

    public GameObject LookingTarget;
    public GameObject LookingTargetStorage;


    [Foldout("Cinemachine stuff")]
    public Camera cam;
    public CinemachineVirtualCamera CMvc;
    public CinemachineInputProvider CMip;
    public CinemachineBasicMultiChannelPerlin CMbmcp;
    public CinemachineImpulseSource CMis;
    public CinemachinePOV CMpov;
    public PlayerMovement playerMovement;
    public PlayerStats playerStats;

    void Awake()
    {
        //Assign Components
        cam = Camera.main;
        CMvc = FindAnyObjectByType<CinemachineVirtualCamera>();
        CMbmcp = CMvc.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        CMpov = CMvc.GetCinemachineComponent<CinemachinePOV>();
        CMip = GetComponent<CinemachineInputProvider>();

        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerStats = FindAnyObjectByType<PlayerStats>();
    }

    void Update()
    {
        CMip.enabled = !playerMovement.Paused;
        
        if(playerStats.Dead || playerMovement.Paused)
        {
            CMvc.m_Lens.Dutch = 0;
            CMvc.m_Lens.FieldOfView = 60;
            CMbmcp.m_AmplitudeGain = 0;
            CMbmcp.m_FrequencyGain = 0;
            return;
        }


        //Dutch Tilt + Field Of View
        CMvc.m_Lens.Dutch = Mathf.SmoothDamp(CMvc.m_Lens.Dutch, TargetDutch, ref BlendDutch, 0.1f);
        CMvc.m_Lens.FieldOfView = Mathf.SmoothDamp(CMvc.m_Lens.FieldOfView, TargetFOV, ref BlendFOV, 0.2f);
        TargetDutch = playerMovement.MovementX * -1.5f;


        //Footstep
        CMbmcp.m_AmplitudeGain = Mathf.SmoothDamp(CMbmcp.m_AmplitudeGain, TargetShakeAmplitude, ref BlendShakeAmplitude, 0.1f);
        CMbmcp.m_FrequencyGain = Mathf.SmoothDamp(CMbmcp.m_FrequencyGain, TargetShakeFrequency, ref BlendShakeFrequency, 0.1f);

        
        //Land Force
        if(!playerMovement.Crouching) CMis.m_DefaultVelocity.y = 0.25f;
        else CMis.m_DefaultVelocity.y = 0.15f;


        //Footstep Shake Conditions
        if(playerMovement.WalkingCheck())
        {
            if(playerMovement.Running)
            {
                TargetShakeAmplitude = 4f;
                TargetShakeFrequency = 0.05f;
            }
            else if(playerMovement.Crouching)
            {
                TargetShakeAmplitude = 1f;
                TargetShakeFrequency = 0.03f;
            }
            else
            {
                TargetShakeAmplitude = 3f;
                TargetShakeFrequency = 0.04f;
            }
        }
        else
        {
            TargetShakeAmplitude = 0f;
            TargetShakeFrequency = 0f;
        } 

        RaycastHit hit = Tools.GetCameraForwardHit3D(5f);
        LookingTarget = hit.collider == null ? null : hit.collider.gameObject;
        if (LookingTarget != null) // Object
        {
            if (LookingTarget != LookingTargetStorage) // Unique Object
            {
                //DebugPlus.DrawSphere(Tools.GetCameraForwardHit3D().point, 0.25f).Color(Color.green).Duration(0.1f);

                Interactable newItem = LookingTarget.GetComponentInParent<Interactable>();
                if (newItem != null) newItem.MouseOver();

                Interactable oldItem = LookingTargetStorage != null ? LookingTargetStorage.GetComponentInParent<Interactable>() : null;
                if (oldItem != null) oldItem.MouseExit();

                LookingTargetStorage = LookingTarget;
            }
            //else DebugPlus.DrawSphere(Tools.GetCameraForwardHit3D().point, 0.1f).Color(Color.red);
        }
        else
        {
            Interactable oldItem = LookingTargetStorage != null ? LookingTargetStorage.GetComponentInParent<Interactable>() : null;
            if (oldItem != null) oldItem.MouseExit();
            LookingTargetStorage = null;
        }
    }


    public void OnInteract(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            if(LookingTarget == null) return;
            Interactable newItem = LookingTarget.GetComponent<Interactable>();
            if (newItem != null) newItem.InteractStart();
        }
    }

    public void Die()
    {
        CMvc.enabled = false;
        TargetShakeAmplitude = 0f;
        TargetShakeFrequency = 0f;
    }
}
