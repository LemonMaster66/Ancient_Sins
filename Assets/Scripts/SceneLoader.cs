using System.Collections;
using PalexUtilities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public bool TransitionIn;
    public bool TransitionOut;
    public bool PausePlayerOnAwake;
    public bool ResetItems;

    [Space(6)]

    public GameObject PlayerTemp;
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public GameObject mask;
    [HideInInspector] public CutoutMaskUI BackgroundImage;

    PlayerStats playerStats;
    PlayerMovement playerMovement;

    void Awake()
    {
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerStats = FindAnyObjectByType<PlayerStats>();

        if(playerMovement == null) Instantiate(PlayerTemp, Vector3.zero, Quaternion.identity);

        if(TransitionIn)
        {
            AssignComponents();
            mask.GetComponent<Animator>().Play("Transition In", 0, 0.0f);
        }
        if(PausePlayerOnAwake)
        {
            playerMovement.Pause(true);
            playerMovement.Movement = Vector3.zero;
            playerMovement.Grounded = false;
        }
        else
        {
            playerMovement.Pause(false);
            playerMovement.CanMove = true;
        }
        if(ResetItems) playerStats.ResetItems();
        
        
        Transform SceneOrigin = Tools.GetChildren(transform)[0].transform;
        if(SceneOrigin != null) FindAnyObjectByType<PlayerMovement>().Teleport(SceneOrigin);
    }
    
    public IEnumerator ChangeScene(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
    public IEnumerator TransitionScene(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject mask = GameObject.Find("Mask");
        if(mask != null) mask.GetComponent<Animator>().Play("Transition Out", 0, 0f);

        yield return new WaitForSeconds(0.5f);
        
        StartCoroutine(ChangeScene(sceneName, 0));
    }

    public void AssignComponents()
    {
        mask = GameObject.Find("Mask");

        canvas = mask.GetComponentInParent<Canvas>();
        canvas.enabled = false;
        canvas.enabled = true;

        BackgroundImage = mask.GetComponentInChildren<CutoutMaskUI>();
    }
}