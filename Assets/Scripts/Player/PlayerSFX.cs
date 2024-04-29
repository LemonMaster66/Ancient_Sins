using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class PlayerSFX : MonoBehaviour
{
    public GameObject AudioPrefab;
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
                enemy.HearSound(transform.position, 35, 5);
            }
            stepTimer = TimeBetweenSteps;
        }

        if(PM.Running)        TimeBetweenSteps = 0.3f;
        else if(PM.Crouching) TimeBetweenSteps = 0.65f;
        else                  TimeBetweenSteps = 0.4f;

        stepTimer = (float)Math.Round(stepTimer, 2);
    }



    public void PlaySound(AudioClip audioClip, float Volume = 1, float Pitch = 1, float PitchVariation = 0, bool Loop = false)
    {
        GameObject AudioObj = Instantiate(AudioPrefab, PM.transform.position, Quaternion.identity, transform);
        AudioObj.name = audioClip.name;

        AudioSource audioSource = AudioObj.GetComponent<AudioSource>();
        audioSource.clip   =  audioClip;
        audioSource.volume =  Volume;
        audioSource.pitch  =  Pitch + UnityEngine.Random.Range(-PitchVariation, PitchVariation);
        audioSource.loop   =  Loop;

        audioSource.Play();
        if(!Loop) Destroy(AudioObj, audioClip.length);
    }


    public void PlayRandomSound(AudioClip[] audioClip, float Volume = 1, float Pitch = 1, float PitchVariation = 0, bool Loop = false)
    {
        GameObject AudioObj = Instantiate(AudioPrefab, PM.transform.position, Quaternion.identity, transform);

        AudioSource audioSource = AudioObj.GetComponent<AudioSource>();
        AudioClip RandomClip = audioClip[UnityEngine.Random.Range(0, audioClip.Length)];

        AudioObj.name = RandomClip.name;

        audioSource.clip     = RandomClip;
        audioSource.volume   = Volume;
        audioSource.pitch    = Pitch + UnityEngine.Random.Range(-PitchVariation, PitchVariation);

        audioSource.Play();
        Destroy(AudioObj, RandomClip.length);
    }

    public void StopSound(AudioClip audioClip)
    {
        AudioSource[] audioSources = GetComponentsInChildren<AudioSource>();
        foreach(AudioSource audioSource in audioSources)
        {
            if(audioSource.clip == audioClip) Destroy(audioSource.gameObject);
        }
    }
}

