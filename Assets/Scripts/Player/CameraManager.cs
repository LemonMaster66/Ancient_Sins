using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
    public bool Focusing         = false;
    public bool Rendering        = false;
    public bool InGallery        = false;
    public bool TakenPhoto       = false;


    [Foldout("References")]
    [Range(0,1)]   public float AnimatorWeight;

    public  RenderTexture    renderTexture;
    public  Texture2D        renderingImage;
    public  Texture2D        renderStorage;

    private Camera           cam;
    private CameraFX         cameraFX;
    private Animator         animator;
    private Light            spotLight;
    private PlayerSFX        playerSFX;
    private Volume           postProcessing;
    private GameObject       cameraObj;
    private TextMeshPro      tmPro;

    
    void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        cameraFX = FindAnyObjectByType<CameraFX>();
        animator = GetComponentInChildren<Animator>();
        spotLight = GetComponentInChildren<Light>();
        playerSFX = FindAnyObjectByType<PlayerSFX>();
        postProcessing = GetComponentInChildren<Volume>();
        cameraObj = transform.GetChild(0).gameObject;
        tmPro = GetComponentInChildren<TextMeshPro>();

        tmPro.enabled = false;
    }

    void FixedUpdate()
    {
        if(spotLight.intensity > 0) spotLight.intensity -= 5;
        if(spotLight.intensity < 0) spotLight.intensity =  0;

        if(captureCooldownTime > 0 && Rendering) captureCooldownTime -= Time.deltaTime;
        if(captureCooldownTime <= 0 && Rendering)
        {
            captureCooldownTime =  0;
            Rendering = false;

            Photos[Photos.Count-1] = renderStorage;
            if(InGallery && ActivePhoto == Photos.Count-1) DisplayPhoto(Photos[Photos.Count-1]);
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
        spotLight.intensity = 175;
        captureCooldownTime = CaptureCooldown;

        #region Post Processing
            postProcessing.profile.TryGet(out ColorAdjustments colorAdjustments);
            colorAdjustments.postExposure.value = 1.5f;
            colorAdjustments.contrast.value = -10;

            cam.Render();

            colorAdjustments.postExposure.value = 0.35f;
            colorAdjustments.contrast.value = -5;
        #endregion
        #region Read Texture
            RenderTexture.active = renderTexture;
            Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            texture2D.ReadPixels(new Rect(0,0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
        #endregion
        
        if(TakenPhoto) ActivePhoto = Photos.Count;

        if(TakenPhoto) Photos.Add(renderingImage);
        else Photos[0] = renderingImage;

        renderStorage = texture2D;
        if(InGallery) DisplayPhoto(renderingImage);

        TakenPhoto = true;
        playerSFX.PlaySound(playerSFX.Capture, 1, 1, 0.05f);
    }

    [Button]
    public void DisplayPhoto(Texture2D texture2D)
    {
        Renderer Screen = cameraObj.GetComponent<Renderer>();
        if(texture2D == null) Screen.material.mainTexture = Photos[ActivePhoto];
        else                  Screen.material.mainTexture = texture2D;

        if(TakenPhoto)
        {
            ActivePhoto++;
            tmPro.text = "Photo: " + ActivePhoto + "/" + Photos.Count;
            ActivePhoto--;
        }
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
            Renderer Screen = cameraObj.GetComponent<Renderer>();

            InGallery     = InGallery ? false : true;
            tmPro.enabled = InGallery ? true : false;

            if(InGallery) Screen.material.mainTexture = Photos[Photos.Count-1];
            else          Screen.material.mainTexture = renderTexture;
        }
    }


    public void OnPhotoPick(InputAction.CallbackContext context)
    {
        if(Photos.Count < 1) return;
        float input = context.ReadValue<float>();
        if(input != 0)
        {
            ActivePhoto += (int)input;

            if(ActivePhoto < 0)             ActivePhoto = Photos.Count-1;
            if(ActivePhoto > Photos.Count-1) ActivePhoto = 0;

            if(InGallery) DisplayPhoto(Photos[ActivePhoto]);
        }
    }



    //***********************************************************************
    //***********************************************************************
    //Extra Logic
}
