using System;
using UnityEngine;
using VInspector;

public class SmokeBomb : MonoBehaviour
{
    [Tab("Main")]
    public float ThrowForce;
    public float ThrowSpin;

    public bool SmokeActive;
    public float SmokeActiveDelay;

    [Tab("Settings")]
    public PlayerMovement playerMovement;
    public PlayerStats    playerStats;
    public AudioManager   audioManager;
    public Enemy          enemy;

    public Rigidbody      rb;
    public SphereCollider Trigger;
    public ParticleSystem particle;


    void FixedUpdate()
    {
        if(SmokeActive) SmokeActiveDelay += Time.deltaTime;
        if(SmokeActiveDelay > 1.5 && Trigger.radius < 6) Trigger.radius = Math.Clamp(Trigger.radius + Time.deltaTime * 5, 0, 6);
    }

    void Awake()
    {
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerStats    = FindAnyObjectByType<PlayerStats>();
        audioManager   = FindAnyObjectByType<AudioManager>();
        enemy          = FindAnyObjectByType<Enemy>();

        rb       = GetComponent<Rigidbody>();
        Trigger  = GetComponent<SphereCollider>();
        particle = GetComponent<ParticleSystem>();

        Trigger.radius = 0;
        rb.AddForce(playerMovement.Camera.forward * ThrowForce, ForceMode.VelocityChange);
        rb.AddTorque(playerMovement.Camera.right * ThrowSpin + playerMovement.Camera.transform.up * (ThrowSpin/8));
    }

    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag != "Player") return;

        if(enemy.State == "Chasing")
        {
            //enemy.SetState("Searching");
            //enemy.MoveUpdate();
        }
        else enemy.IgnorePlayer = true;
        //enemy.WeepingAngel = false;
        //enemy.Watched = false;
    }
    void OnTriggerExit(Collider collider)
    {
        if(collider.tag == "Enemy") enemy.WeepingAngel = true;
        if(collider.tag != "Player") return;

        //enemy.WeepingAngel = true;
        enemy.IgnorePlayer = false;
    }
    void OnTriggerStay(Collider collider)
    {
        if(collider.tag == "Enemy") enemy.WeepingAngel = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        SmokeActive = true;
        particle.Play();
    }

    void OnDestroy()
    {
        //enemy.WeepingAngel = true;
        enemy.IgnorePlayer = false;
    }
}
