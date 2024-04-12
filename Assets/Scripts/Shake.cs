using UnityEngine;
using Cinemachine;
using System;
using VInspector;

public class Shake : MonoBehaviour
{
    public NoiseSettings noiseProfile;
    public float PositionAmplitude = 1f;
    public float RotationAmplitude = 1f;
    public float frequencyGain = 1f;

    private float noiseTime = 0f;

    private Vector3    Porigin;
    private Quaternion Rorigin;
    [Range(0,1)] public float AutoRecentreStrength = 0.75f;


    void Start()
    {
        Porigin = transform.position;
        Rorigin = transform.rotation;
    }


    private void Update()
    {
        noiseTime += Time.deltaTime * frequencyGain;
        ApplyShake();

        transform.position = Vector3.Slerp    (transform.position, Porigin, AutoRecentreStrength);
        transform.rotation = Quaternion.Slerp (transform.rotation, Rorigin, AutoRecentreStrength);
    }


    private void ApplyShake()
    {
        if (noiseProfile == null) return;

        // Get combined noise results for position and rotation
        Vector3 positionNoise = NoiseSettings.GetCombinedFilterResults(noiseProfile.PositionNoise,    noiseTime, Vector3.zero);
        Vector3 rotationNoise = NoiseSettings.GetCombinedFilterResults(noiseProfile.OrientationNoise, noiseTime, Vector3.zero);

        // Apply amplitude gain to noise
        positionNoise *= PositionAmplitude;
        rotationNoise *= RotationAmplitude;

        transform.position += positionNoise;
        transform.rotation *= Quaternion.Euler(rotationNoise);
    }
}