using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using PalexUtilities;
using VInspector;

public class Prop : MonoBehaviour
{
    [Tab("Main")]
    public bool Artefact    = false;
    public bool PhysicsProp = false;

    [ShowIf("Artefact")] [Header("Artefact Properties")]
    public float Value;
    public int   TimesPhotographed;
    public AnimationCurve ValueFalloff;

    [ShowIf("PhysicsProp")] [Header("Physics Properties")]
    public bool Destructable;
    public float Gravity = 100;

    [ShowIf("Destructable")] [Header("Destruction Properties")]
    public GameObject DestructionProp;
    public float FragmentGravity;
    [EndIf]


    [Tab("Audio")]
    [RangeResettable(0,2.5f)] public float Force;

    [Space(8)]

    [RangeResettable(0,2.5f)] public float MediumThreshold = 0.8f;
    [RangeResettable(0,2.5f)] public float LargeThreshold = 1.3f;
    [ShowIf("Destructable")]
    [RangeResettable(0,2.5f)] public float SmallShatterThreshold = 1.85f;
    [RangeResettable(0,2.5f)] public float LargeShatterThreshold = 2.1f;
    [EndIf]
    
    [Space(10)]

    public AudioClip[] CollideSmallSfx;
    public AudioClip[] CollideMediumSfx;
    public AudioClip[] CollideLargeSfx;

    [Space(10)]

    [ShowIf("Destructable")]
    public AudioClip[] ShatterSmallSfx;
    public AudioClip[] ShatterLargeSfx;
    [EndIf]


    [Tab("Settings")]
    public float sfxCooldown;
    [Space(6)]
    public Rigidbody      rb;
    public Camera         cam;
    public AudioManager   audioManager;
    public PlayerMovement playerMovement;
    public CameraManager  cameraManager;
    public Collider       colliderBounds;
    private Plane[]       frustumPlanes;


    public void Awake()
    {
        audioManager = GetComponent<AudioManager>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        cameraManager = FindAnyObjectByType<CameraManager>();
        colliderBounds = GetComponent<Collider>();
        cam = FindAnyObjectByType<CameraManager>().cam;

        if(PhysicsProp)
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
        }
    }

    public void FixedUpdate()
    {
        rb.AddForce(Vector3.down*Gravity/6f * rb.mass);

        if(sfxCooldown > 0) sfxCooldown = Math.Clamp(sfxCooldown - Time.deltaTime, 0, math.INFINITY);
    }
    

    //**************************************************
    //Physics Stuff
    public void OnCollisionEnter(Collision collision)
    {
        Force = (collision.relativeVelocity.magnitude * Math.Clamp(rb.velocity.magnitude, 0, 1) / 10) + 0.1f;
        if(sfxCooldown == 0) CollideFX(Force, collision.relativeVelocity);
        sfxCooldown = 0.25f;
    }

    public void CollideFX(float Force, Vector3 relativeVelocity)
    {
        if(Force > LargeShatterThreshold && ShatterLargeSfx.Length > 0 && Destructable)
        {
            Shatter(relativeVelocity);
            audioManager.PlayRandomSound(ShatterLargeSfx, Force, 1, 0.2f);
        }
        else if(Force > SmallShatterThreshold && ShatterSmallSfx.Length > 0 && Destructable)
        {
            Shatter(relativeVelocity);
            audioManager.PlayRandomSound(ShatterSmallSfx, Force, 1, 0.2f);
        }
        else if(Force > LargeThreshold && CollideLargeSfx.Length > 0)
        {
            audioManager.PlayRandomSound(CollideLargeSfx, Force, 1, 0.2f);
        }
        else if(Force > MediumThreshold && CollideMediumSfx.Length > 0)
        {
            audioManager.PlayRandomSound(CollideMediumSfx, Force, 1, 0.2f);
        }
        else if(CollideSmallSfx.Length > 0)
        {
            audioManager.PlayRandomSound(CollideSmallSfx, Force, 1, 0.2f);
        }
    }
    public void Shatter(Vector3 relativeVelocity)
    {
        Destructable = false;
        Value = 0;

        if(gameObject.TryGetComponent(out Outline outline)) outline.enabled = false; 

        foreach(Transform transform in transform.GetChildren())
        {
            transform.parent = null;

            Rigidbody FragRb = transform.gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
            ExtraGravity gravity = transform.gameObject.AddComponent(typeof(ExtraGravity)) as ExtraGravity;

            FragRb.velocity = relativeVelocity /10;
            FragRb.useGravity = false;
            FragRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            gravity.Gravity = FragmentGravity;
        }
    }
}
