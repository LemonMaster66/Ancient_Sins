using PalexUtilities;
using UnityEngine;
using VInspector;

public class Door : Interactable
{
    [Tab("Main")]
    [Header("States")]
    public bool Open;
    public bool Locked;

    [Space(8)]

    [Header("Other")]
    public Transform SendPosition;


    [Tab("Audio")]
    public AudioClip[] Open_Small;
    public AudioClip[] Open_Medium;
    public AudioClip[] Open_Large;



    [Tab("Settings")]
    public Outline outline;
    public float   TargetOutline = 0;
    public float   BlendOutline;

    public Animator animator;

    public PlayerMovement playerMovement;
    public PlayerStats playerStats;
    public PlayerSFX playerSFX;
    public Enemy enemy;


    void Awake()
    {
        outline = GetComponent<Outline>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerStats = FindAnyObjectByType<PlayerStats>();
        playerSFX = FindAnyObjectByType<PlayerSFX>();

        enemy = FindAnyObjectByType<Enemy>();

        outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineWidth = 0;

        SendPosition = Tools.GetChildren(transform)[0].transform;
    }
    
    
    public override void MouseOver()
    {
        // Runs when the mouse Hovers Over this
    }

    public override void MouseExit()
    {
        // Runs when the mouse Exits this
    }

    public override void InteractStart()
    {
        playerMovement.Teleport(SendPosition);
    }

    public override void InteractEnd()
    {
        // Runs when E is Released on the Object
    }
}
