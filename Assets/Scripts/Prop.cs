using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using VInspector;

public class Prop : MonoBehaviour
{
    [Tab("Main")]
    public bool Artefact    = false;
    public bool PhysicsProp = false;

    [ShowIf("Artefact")] [Header("Artefact Properties")]
    public float Value;
    public AnimationCurve ValueFalloff;

    [ShowIf("PhysicsProp")] [Header("Physics Properties")]
    public bool Destructable;
    public float Gravity = 100;

    [ShowIf("Destructable")] [Header("Destruction Properties")]
    public GameObject DestructionProp;
    public float toShatterMagnitude;
    public float FragmentGravity;
    [EndIf]


    [Tab("Audio")]
    public AudioClip[] CollideSmallSfx;
    public AudioClip[] CollideMediumSfx;
    public AudioClip[] CollideLargeSfx;

    public AudioClip[] ShatterSmallSfx;
    public AudioClip[] ShatterLargeSfx;


    [Tab("Settings")]
    public float sfxCooldown;
    [Space(6)]
    public Rigidbody rb;
    public Collider colliderBounds;
    public AudioManager audioManager;


    public void Awake()
    {
        audioManager = GetComponent<AudioManager>();

        if(PhysicsProp)
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
        }
    }

    public void Update()
    {
        rb.AddForce(Vector3.down*Gravity/6f * rb.mass);

        if(sfxCooldown > 0) sfxCooldown = Math.Clamp(sfxCooldown - Time.deltaTime, 0, math.INFINITY);
    }
    
    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.relativeVelocity.magnitude);
        DebugPlus.DrawWireSphere(transform.position, 1).Duration(0.1f).Color(Color.green);

        float volume = (collision.relativeVelocity.magnitude/5) * rb.velocity.magnitude/10;
        if(sfxCooldown == 0) audioManager.PlayRandomSound(CollideSmallSfx, volume, 1, 0.2f);
        sfxCooldown = 0.25f;
    }
}
