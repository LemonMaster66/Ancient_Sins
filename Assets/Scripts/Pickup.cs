using System;
using PalexUtilities;
using TMPro;
using UnityEngine;
using VInspector;

public class Pickup : Interactable
{
    [Tab("Main")]
    public float Value;

    [Space(8)]

    public bool overrideText = false;
    public bool overrideHearRange = false;
    public bool overrideHearPriority = false;

    [EndIf] [Space(8)]

    [ShowIf("overrideText")]         public string text;
    [ShowIf("overrideHearRange")]    public float  hearRange;
    [ShowIf("overrideHearPriority")] public float  hearPriority;
    [EndIf]


    [Header("Other")]
    public GameObject collectParticle;
    public GameObject MoneyText;


    [Tab("Audio")]
    public AudioClip[] InteractSFX;


    [Tab("Settings")]
    public Outline outline;
    public float   TargetOutline = 0;
    public float   BlendOutline;

    public Rigidbody rb;
    public Collider col;
    
    public PlayerMovement playerMovement;
    public PlayerStats playerStats;
    public PlayerSFX playerSFX;

    public Enemy enemy;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        outline = GetComponent<Outline>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerStats = FindAnyObjectByType<PlayerStats>();
        playerSFX = FindAnyObjectByType<PlayerSFX>();

        enemy = FindAnyObjectByType<Enemy>();

        outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineWidth = 0;
    }

    void Update()
    {
        outline.OutlineWidth = Mathf.SmoothDamp(outline.OutlineWidth, TargetOutline, ref BlendOutline, 0.075f);
    }

    public override void MouseOver()
    {
        TargetOutline = 20f;
    }

    public override void MouseExit()
    {
        TargetOutline = 0f;
    }

    public override void InteractStart()
    {
        if(InteractSFX != null && InteractSFX.Length > 0) playerSFX.PlayRandomSound(InteractSFX, 0.5f, 1, 0.15f);
        
        transform.localScale = new Vector3(0,0,0);
        if(rb != null)  rb.isKinematic = true;
        if(col != null) col.enabled = false;

        foreach(Transform child in Tools.GetChildren(transform))
        {
            Collider collider = child.GetComponent<Collider>();
            if(collider != null) collider.enabled = false;
        }

        if(collectParticle != null)
        {
            Instantiate(collectParticle, transform.position, Quaternion.identity, null);
            collectParticle.GetComponent<ParticleSystem>().Play();
        }

        if(MoneyText != null) SpawnText(!overrideText ? Value + "" : text);
        enemy.HearSound(transform.position,
            overrideHearRange == false  ? Value*4 : hearRange,
            overrideHearRange == false  ? Value   : hearPriority);

        if(Value > 0) playerStats.ObtainMoney(Value);

        Destroy(gameObject, 0.5f);
    }

    public void SpawnText(string text)
    {
        GameObject MoneyTextObj = Instantiate(MoneyText, transform.position, Quaternion.identity);
        MoneyTextObj.GetComponent<TextMeshPro>().text = text;
    }
}
