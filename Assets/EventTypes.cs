using UnityEngine;
using UnityEngine.UIElements;
using VInspector;

public class EventTypes : Interactable
{
    [Header("Types")]
    public bool Interactable;
    public bool Trigger;


    [Header("Events")]
    public bool DestroySelf;
    [Space(5)]
    public bool PausePlayer;
    public bool UnPausePlayer;
    public bool EquipCamera;
    public bool UnEquipCamera;
    public bool UsePassiveItems;
    public bool ResetPassiveItems;
    public AudioClip PlaySound;
    [Space(5)]
    public bool LoadScene;
    [ShowIf("LoadScene")]
    public bool Transition;
    public string Scene;
    public float Delay;
    [EndIf]
    [Space(5)]
    public bool ActivateEnemy;

    [Space(10)]

    private PlayerMovement playerMovement;
    private PlayerStats playerStats;
    private PlayerSFX playerSFX;

    private CameraManager cameraManager;
    private CameraFX cameraFX;

    private Enemy enemy;

    private SceneLoader sceneLoader;


    void Awake()
    {
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerStats = FindAnyObjectByType<PlayerStats>();
        playerSFX = FindAnyObjectByType<PlayerSFX>();

        cameraManager = FindAnyObjectByType<CameraManager>();
        cameraFX = FindAnyObjectByType<CameraFX>();

        enemy = FindAnyObjectByType<Enemy>();

        sceneLoader = FindAnyObjectByType<SceneLoader>();
    }

    public override void InteractStart()
    {
        if(Interactable) Activate();
    }

    void OnTriggerEnter(Collider collider)
    {
        if(Trigger && collider.gameObject == playerMovement.gameObject) Activate();  
    }

    public void Activate()
    {
        if(PausePlayer)
        {
            playerMovement.Pause(true);
            playerMovement.CanMove = false;
        }
        if(UnPausePlayer)
        {
            playerMovement.Pause(false);
            playerMovement.CanMove = true;
        }
        if(EquipCamera)
        {
            cameraManager.HasCamera = true;
            cameraManager.Hidden = false;
        }
        if(UnEquipCamera)
        {
            cameraManager.HasCamera = false;
            cameraManager.Hidden = true;
        }
        if(UsePassiveItems)
        {
            playerStats.UsePassiveItems();
        }
        if(PlaySound != null)
        {
            playerSFX.PlaySound(PlaySound, 0.85f);
        }
        if(LoadScene && !Transition)
        {
            sceneLoader.StartCoroutine(sceneLoader.ChangeScene(Scene, Delay));
        }
        if(Transition)
        {
            sceneLoader.AssignComponents();
            sceneLoader.StartCoroutine(sceneLoader.TransitionScene(Scene, Delay));
            sceneLoader.BackgroundImage.color = Color.black;
            sceneLoader.mask.GetComponent<Animator>().Play("Set In", 0, 0f);
        }
        if(ActivateEnemy)
        {
            enemy.Active = true;
            enemy.SetState("Wandering");
        }
        if(DestroySelf)
        {
            Destroy(gameObject);
        }
    }
}
