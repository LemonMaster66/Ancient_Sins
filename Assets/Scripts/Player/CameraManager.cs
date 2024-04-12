using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

public class CameraManager : MonoBehaviour
{
    public List<Texture2D> Photos = new List<Texture2D>();
    public int ActivePhoto;

    [Space(10)]

    public float CaptureCooldown;
    public float captureCooldownTime;

    [Space(5)]

    [Range(0,100)] public float Zoom;


    [Header("States")]
    public bool Focusing  = false;
    public bool Rendering = false;


    [Foldout("Debug")]
    [Range(0,1)]   public float AnimatorWeight;

    public Camera     cam;
    public CameraFX   cameraFX;
    public Animator   animator;
    public Light      spotLight;
    public PlayerSFX  playerSFX;

    
    void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        cameraFX = FindAnyObjectByType<CameraFX>();
        animator = GetComponentInChildren<Animator>();
        spotLight = GetComponentInChildren<Light>();
        playerSFX = FindAnyObjectByType<PlayerSFX>();
    }

    void FixedUpdate()
    {
        if(spotLight.intensity > 0) spotLight.intensity -= 8;
        if(spotLight.intensity < 0) spotLight.intensity =  0;

        if(captureCooldownTime > 0) captureCooldownTime -= Time.deltaTime;
        if(captureCooldownTime <= 0)
        {
            captureCooldownTime =  0;
            Rendering = false;
        }
    }
    void Update()
    {
        animator.SetFloat("Blend", AnimatorWeight, 0.1f, Time.deltaTime);
    }


    
    //***********************************************************************
    //***********************************************************************
    //Camera Functions
    public void OnCapture(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            if(!Rendering) Capture();
        }
    }
    [Button]
    public void Capture()
    {
        Rendering = true;
        spotLight.intensity = 200;
        captureCooldownTime = CaptureCooldown;

        playerSFX.PlaySound(playerSFX.Capture, 1, 1, 0.05f);
    }


    public void OnFocus(InputAction.CallbackContext context)
    {
        if(context.started) 
        {
            Focusing = true;
            cameraFX.TargetFOV = 40;
            AnimatorWeight = 1;
        }
        if(context.canceled)
        {
            Focusing = false;
            cameraFX.TargetFOV = 60;
            AnimatorWeight = 0;
        }
    }


    public void OnZoom(InputAction.CallbackContext context)
    {
        float inputScroll = context.ReadValue<float>();
        Zoom = Math.Clamp(Zoom + inputScroll *10, 0, 100);
        cam.fieldOfView = 60 - Zoom/2;
    }


    public void OnToggleGallery(InputAction.CallbackContext context)
    {
        if(context.started) 
        {
            
        }
    }


    public void OnPhotoPick(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();
    }



    //***********************************************************************
    //***********************************************************************
    //Extra Logic
}
