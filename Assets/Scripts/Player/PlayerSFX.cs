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


    [Header("Physics Sounds")]
    public AudioClip[] Step;
    public AudioClip Jump;
    public AudioClip[] Land;


    [Header("Info")]
    public float stepTimer;

    private PlayerMovement PM;
    private GroundCheck groundCheck;


    void Awake()
    {
        PM = FindAnyObjectByType<PlayerMovement>();
        groundCheck = GetComponentInChildren<GroundCheck>();
    }


    void FixedUpdate()
    {
        if(PM.WalkingCheck()) stepTimer -= Time.deltaTime;
        if(stepTimer < 0)
        {
            PlayRandomSound(Step, 1, 1, 0.2f, false);
            stepTimer = TimeBetweenSteps;
        }

        stepTimer = (float)Math.Round(stepTimer, 2);
    }



    public void PlaySound(AudioClip audioClip, float Pitch = 1, float Volume = 1, float PitchVariation = 0, bool Loop = false)
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


    public void PlayRandomSound(AudioClip[] audioClip, float Pitch, float Volume, float PitchVariation, bool Loop)
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

