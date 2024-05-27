using PalexUtilities;
using UnityEngine;
using VInspector;

public class Door : Interactable
{
    public string Scene;
    public bool TransitionOut;
    private Transform SendPosition;
    
    private PlayerMovement playerMovement;
    // private PlayerStats playerStats;
    // private PlayerSFX playerSFX;
    // private Enemy enemy;


    void Awake()
    {
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        // playerStats = FindAnyObjectByType<PlayerStats>();
        // playerSFX = FindAnyObjectByType<PlayerSFX>();
        // enemy = FindAnyObjectByType<Enemy>();

        Transform[] Pos = Tools.GetChildren(transform).ToArray();
        if(Pos.Length != 0) SendPosition = Pos[0].transform;
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
        if(Scene == "") playerMovement.Teleport(SendPosition);
        else 
        {
            SceneLoader sceneLoader = FindAnyObjectByType<SceneLoader>();
            if(TransitionOut)
            {
                sceneLoader.StartCoroutine(sceneLoader.TransitionScene(Scene, 0));
                CameraManager cameraManager = FindAnyObjectByType<CameraManager>();
                cameraManager.HasCamera = false;
                cameraManager.Hidden = true;

                sceneLoader.AssignComponents();
                sceneLoader.BackgroundImage.color = Color.black;
                sceneLoader.mask.GetComponent<Animator>().Play("Set In", 0, 0f);
            }
            else sceneLoader.StartCoroutine(sceneLoader.ChangeScene(Scene, 0));
        }
    }

    public override void InteractEnd()
    {
        // Runs when E is Released on the Object
    }
}
