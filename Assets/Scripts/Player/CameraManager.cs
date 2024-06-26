using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VInspector;
using PalexUtilities;

public class CameraManager : MonoBehaviour
{
    public VInspector.SerializedDictionary<Texture2D, float> Photos;
    public int ActivePhoto;
    public int FilmLength = 40;

    [Space(10)]

    public float CaptureCooldown;
    public float captureCooldownTime;

    public float FlashBrightness = 200;
    public float FlashDecaySpeed = 6;

    [Space(5)]

    [Range(0,100)] public float Zoom;


    [Header("States")]
    public bool Hidden           = false;
    public bool HasCamera        = false;
    public bool Focusing         = false;
    public bool Rendering        = false;
    public bool InGallery        = false;
    public bool TakenPhoto       = false;
    public bool OutOfFilm        = false;


    [Foldout("Debug Stuff")]
    [Range(0,1)]   public float AnimatorWeight = 0.5f;

    public  Texture2D        defaultImage;
    public  Texture2D        renderingImage;
    public  RenderTexture    renderTexture;
    [Space(6)]
    public  Texture2D        renderStorage;
    public  float            valueStorage;

    [HideInInspector] public Camera cam;
    private CameraFX         cameraFX;
    private Animator         animator;
    private Light            spotLight;
    private PlayerSFX        playerSFX;
    private Volume           postProcessing;
    private GameObject       cameraObj;
    private TextMeshPro      tmPro;
    private PlayerMovement   playerMovement;
    private PlayerStats      playerStats;

    
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
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerStats = FindAnyObjectByType<PlayerStats>();

        tmPro.enabled = false;
    }

    void FixedUpdate()
    {
        if(spotLight.intensity > 0) spotLight.intensity -= FlashDecaySpeed;
        if(spotLight.intensity < 0) spotLight.intensity =  0;

        if(captureCooldownTime > 0 && Rendering) captureCooldownTime -= Time.deltaTime;
        if(captureCooldownTime <= 0 && Rendering)
        {
            captureCooldownTime =  0;
            Rendering = false;

            var lastKey = Photos.ElementAt(Photos.Count - 1).Key;
            Photos.Remove(lastKey);
            Photos.Add(renderStorage, valueStorage);
            if(InGallery && ActivePhoto == Photos.Count-1) DisplayPhoto(Photos.ElementAt(Photos.Count-1).Key);
        }
    }
    void Update()
    {
        animator.SetFloat("Blend", Hidden ? 0 : AnimatorWeight, 0.1f, Time.deltaTime);
    }


    
    //***********************************************************************
    //***********************************************************************
    //Camera Functions
    public void OnCapture(InputAction.CallbackContext context)
    {
        if(!HasCamera || playerStats.Dead) return;

        if(context.started)
        {
            if(!Rendering && Photos.Count < FilmLength) Capture();
            else if(Rendering) playerSFX.PlaySound(playerSFX.RenderingCapture, 0.15f, 1f, 0.1f);
            else
            {
                playerSFX.PlaySound(playerSFX.RenderingCapture, 0.15f, 0.75f, 0.1f);

                if(OutOfFilm) return;

                playerSFX.PlaySound(playerSFX.OutOfFilm, 0.85f, 1f, 0.05f);
                OutOfFilm = true;
                ApplyText();
            }
        }
    }
    [Button]
    public void Capture()
    {
        Rendering = true;
        OutOfFilm = false;
        spotLight.intensity = FlashBrightness;
        captureCooldownTime = CaptureCooldown;
        cam.Render();

        #region Read Texture
            RenderTexture.active = renderTexture;
            Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false, true);
            texture2D.ReadPixels(new Rect(0,0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
        #endregion

        if (TakenPhoto)
        {
            ActivePhoto = Photos.Count;
            Photos.Add(renderingImage, 0);
        }
        else
        {
            Photos.Clear();
            Photos.Add(renderingImage, 0);
        }

        renderStorage = texture2D;
        if(InGallery) DisplayPhoto(renderingImage);

        TakenPhoto = true;

        valueStorage = (float)Math.Round(CalculateArtefacts(), 1);
        playerSFX.PlaySound(playerSFX.Capture, 1, 1, 0.05f);
        if(playerSFX.enemy != null) playerSFX.enemy.HearSound(transform.position, 100, 15);
    }

    public float CalculateArtefacts()
    {
        float Money = 0;
        Prop[] props = FindObjectsByType<Prop>(FindObjectsSortMode.None);
        foreach(Prop prop in props)
        {
            if(prop.Artefact && Tools.FrustumCheck(prop.colliderBounds, prop.cam) && !Tools.OcclusionCheck(prop.transform, playerMovement.transform, 50))
            {
                Money += prop.Value * prop.ValueFalloff.Evaluate((float)prop.TimesPhotographed/3.5f);
                prop.TimesPhotographed++;
            }
        }
        return Money;
    }

    [Button]
    public void DisplayPhoto(Texture2D texture2D)
    {
        Renderer Screen = cameraObj.GetComponent<Renderer>();
        if(texture2D == null) Screen.material.mainTexture = Photos.ElementAt(ActivePhoto).Key;
        else                  Screen.material.mainTexture = texture2D;

        ApplyText();
    }


    public void OnFocus(InputAction.CallbackContext context)
    {
        if(!HasCamera || playerStats.Dead) return;

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
            AnimatorWeight = 0.5f;
        }
    }


    public void OnZoom(InputAction.CallbackContext context)
    {
        float inputScroll = context.ReadValue<float>();
        if(inputScroll == 0 || InGallery) return;

        Zoom = Math.Clamp(Zoom + inputScroll *10, 0, 100);
        cam.fieldOfView = 60 - Zoom/2;

        playerSFX.PlaySound(playerSFX.ZoomInterval, 0.5f, 0.6f+(Zoom/200));
    }


    public void OnToggleGallery(InputAction.CallbackContext context)
    {
        if(context.started) 
        {
            Renderer Screen = cameraObj.GetComponent<Renderer>();

            InGallery     = InGallery ? false : true;
            tmPro.enabled = InGallery ? true : false;

            if(InGallery)
            {
                Screen.material.mainTexture = Photos.ElementAt(Photos.Count-1).Key;
                playerSFX.PlaySound(playerSFX.ViewGallery, 0.4f, 1f);
            }
            else          
            {
                Screen.material.mainTexture = renderTexture;
                playerSFX.PlaySound(playerSFX.ViewGallery, 0.4f, 0.8f);
            }

            ApplyText();
        }
    }


    public void OnPhotoPick(InputAction.CallbackContext context)
    {
        if(Photos.Count < 1) return;
        float input = context.ReadValue<float>();
        if(input != 0 && InGallery)
        {
            ActivePhoto += (int)input;

            if(ActivePhoto < 0)             ActivePhoto = Photos.Count-1;
            if(ActivePhoto > Photos.Count-1) ActivePhoto = 0;

            if(InGallery) DisplayPhoto(Photos.ElementAt(ActivePhoto).Key);
            playerSFX.PlaySound(playerSFX.ZoomInterval, 0.5f, 0.5f + ((float)ActivePhoto / Photos.Count)/2);
        }
    }



    //***********************************************************************
    //***********************************************************************
    //Extra Logic

    void ApplyText()
    {
        if(!TakenPhoto) return;
        ActivePhoto++;
        tmPro.text = "Photo: " + ActivePhoto + "/" + FilmLength;
        ActivePhoto--;

        if(!InGallery && OutOfFilm) 
        {
            tmPro.enabled = true;
            tmPro.text = "Out Of Film";
        }
    }

    public void EnableCamera()
    {

    }
}