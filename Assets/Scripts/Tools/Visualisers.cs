using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VInspector;

public class Visualisers : MonoBehaviour
{
    [Variants("Money", "Camera Film", "Render Speed", "Speed", "Smoke Bombs")]
    public string Type;

    private TextMeshPro textMeshPro;
    private PlayerMovement playerMovement;
    private PlayerStats playerStats;
    private CameraManager cameraManager;

    // Update is called once per frame
    void Awake()
    {
        textMeshPro = GetComponent<TextMeshPro>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerStats = FindAnyObjectByType<PlayerStats>();
        cameraManager = FindAnyObjectByType<CameraManager>();

        UpdateText();
    }

    public void UpdateText()
    {
        if(Type == "Money")            textMeshPro.text = Type + ": " + playerStats.Money + "";
        if(Type == "Camera Film")      textMeshPro.text = Type + ": " + (cameraManager.FilmLength + (10 * playerStats.extraFilm)) + "";
        if(Type == "Render Speed")     textMeshPro.text = Type + ": " + cameraManager.CaptureCooldown + "";
        if(Type == "Speed")            textMeshPro.text = Type + ": " + playerMovement.Speed + "";
        if(Type == "Smoke Bombs")      textMeshPro.text = Type + ": " + playerStats.SmokeBomb + "";
    }
}
