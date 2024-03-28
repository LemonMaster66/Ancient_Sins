using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    public GameObject AudioPrefab;
    [Space(8)]
    public float TimeBetweenSteps;


    [Header("Physics Sounds")]
    public AudioClip[] Step;
    [Space(5)]
    public AudioClip Jump;
    [Space(5)]
    public AudioClip[] Land;
    public AudioClip Slam;
    [Space(5)]
    public AudioClip Dash;
    public AudioClip[] RefreshDash;
    public AudioClip NoDashLeft;
    [Space(5)]
    public AudioClip NoWallJumpsLeft;
    [Space(8)]
    public AudioSource FastFall;
    public AudioSource Slide;

    [Space(15)]
    [Header("Gun General")]
    public AudioClip WeaponSwap;

    [Header("Piercer")]
    public AudioClip[] Piercer_Shoot;
    public AudioClip[] Piercer_PierceShoot;
    public AudioClip Piercer_Charge;
    public AudioClip Piercer_Recharge;

    [Header("Shotgun")]
    public AudioClip Shotgun_Shoot;
    public AudioClip Shotgun_Charge;
    public AudioClip Shotgun_Rack;

    [Header("Nailgun")]
    public AudioClip[] Nailgun_Shoot;


    [Header("Info")]
    public float stepTimer;

    private float TargetSlideVolume;
    private float TargetFastFallVolume;

    private float SlideBlend;
    private float FastFallBlend;


    private PlayerMovement PC;
    private GroundCheck groundCheck;


    void Awake()
    {
        PC = FindAnyObjectByType<PlayerMovement>();
        groundCheck = GetComponentInChildren<GroundCheck>();
    }


    void FixedUpdate()
    {
        if(PC.WalkingCheck()) stepTimer -= Time.deltaTime;
        if(stepTimer < 0)
        {
            PlayRandomSound(Step, 1, 1, 0.2f, false);
            stepTimer = TimeBetweenSteps;
        }

        if(PC.AgainstWall && !PC.Grounded)
        {
            if(PC.rb.velocity.y < 0) TargetSlideVolume = PC.rb.velocity.y/10*-1;
        }
        else if(PC.Grounded && PC.Sliding) TargetSlideVolume = PC.VelocityMagnitudeXZ/33;
        if((PC.Grounded || !PC.AgainstWall) && (!PC.Sliding || !PC.Grounded)) TargetSlideVolume = 0;


        if(!PC.Grounded && !PC.AgainstWall) TargetFastFallVolume = (PC.rb.velocity.y/100) *-1;
        else TargetFastFallVolume = 0;


        Slide.volume    = Mathf.SmoothDamp(Slide.volume,    TargetSlideVolume,    ref SlideBlend,    0.05f);
        FastFall.volume = Mathf.SmoothDamp(FastFall.volume, TargetFastFallVolume, ref FastFallBlend, 0.1f);

        stepTimer = (float)Math.Round(stepTimer, 2);
    }



    public void PlaySound(AudioClip audioClip, float Pitch = 1, float Volume = 1, float PitchVariation = 0, bool Loop = false)
    {
        GameObject AudioObj = Instantiate(AudioPrefab, PC.transform.position, Quaternion.identity, transform);
        AudioObj.name = audioClip.name;

        AudioSource audioSource = AudioObj.GetComponent<AudioSource>();
        audioSource.clip   =  audioClip;
        audioSource.volume =  Volume;
        audioSource.pitch  =  Pitch + UnityEngine.Random.Range(-PitchVariation, PitchVariation);
        audioSource.loop   =  Loop;

        audioSource.Play();
        if(!Loop) Destroy(AudioObj, audioClip.length);
    }


    public void PlayRandomSound(AudioClip[] audioClip, float Pitch, float Volume, float PitchVariation, bool Loop)
    {
        GameObject AudioObj = Instantiate(AudioPrefab, PC.transform.position, Quaternion.identity, transform);

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

