using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class PlayerSFX : AudioManager
{
    [Space(8)]
    public float TimeBetweenSteps;
    public float stepTimer;


    [Header("Physics")]
    public AudioClip[] WalkStep;
    public AudioClip[] RunStep;
    [Space(5)]
    public AudioClip[] Jump;
    public AudioClip[] Land;

    [Header("Other Player")]
    public AudioClip Damage;
    public AudioClip Death;


    [Header("Camera")]
    public AudioClip Capture;


    private PlayerMovement PM;
    private GroundCheck groundCheck;
    [HideInInspector] public Enemy enemy;


    void Awake()
    {
        PM = FindAnyObjectByType<PlayerMovement>();
        groundCheck = GetComponentInChildren<GroundCheck>();
        enemy = FindAnyObjectByType<Enemy>();
    }


    void FixedUpdate()
    {
        if(PM.Dead) return;

        if(PM.WalkingCheck()) stepTimer -= Time.deltaTime;
        if(stepTimer < 0)
        {
            if(!PM.Running) PlayRandomSound(WalkStep, PM.Crouching ? 0.3f:0.75f, 1, 0.2f);
            else
            {
                PlayRandomSound(RunStep,  0.75f, 1, 0.2f);
                if(enemy != null) enemy.HearSound(transform.position, 35, 5);
            }
            stepTimer = TimeBetweenSteps;
        }

        if(PM.Running)        TimeBetweenSteps = 0.3f;
        else if(PM.Crouching) TimeBetweenSteps = 0.65f;
        else                  TimeBetweenSteps = 0.4f;

        stepTimer = (float)Math.Round(stepTimer, 2);
    }
}

