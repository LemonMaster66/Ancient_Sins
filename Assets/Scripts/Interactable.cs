using System;
using Cinemachine;
using PalexUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using VInspector;

public class Interactable : MonoBehaviour
{
    [Tab("Main")]
    [Header("Types")]
    public bool Pickup = true;

    [Header("Properties")]
    [ShowIf("Pickup")] public float Value;

    [EndIf] [Space(8)]

    public bool overrideText = false;
    public bool overrideHearRange = false;
    public bool overrideHearPriority = false;

    [Space(8)]

    [ShowIf("overrideText")]         public string text;
    [ShowIf("overrideHearRange")]    public float  hearRange;
    [ShowIf("overrideHearPriority")] public float  hearPriority;


    [EndIf]
    [Header("States")]
    public bool Hovering;
    public bool Interacting;

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

    public void MouseOver()
    {
        Hovering = true;
        TargetOutline = 20f;
    }

    public void MouseExit()
    {
        Hovering = false;
        TargetOutline = 0f;
    }

    public void InteractStart()
    {
        if(InteractSFX != null && InteractSFX.Length > 0) playerSFX.PlayRandomSound(InteractSFX, 0.5f, 1, 0.15f);
        
        if(Pickup)
        {
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

            Interactable[] interactables = FindObjectsByType<Interactable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if(interactables.Length < 2)
            {
                Tools.ChangeScene("YouWinYippee");
            }

            Destroy(gameObject, 0.5f);
        }
    }

    public void InteractEnd()
    {
        
    }

    public void SpawnText(string text)
    {
        GameObject MoneyTextObj = Instantiate(MoneyText, transform.position, Quaternion.identity);
        MoneyTextObj.GetComponent<TextMeshPro>().text = text;
    }
}
