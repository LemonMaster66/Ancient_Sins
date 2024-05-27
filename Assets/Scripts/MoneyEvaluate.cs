using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VInspector;

public class MoneyEvaluate : MonoBehaviour
{
    [Tab("Main")]
    public float speed = 10;
    public float totalMoney = 0;
    public float timer = 0;

    [Space(8)]

    public int ActivePhoto = 0;

    [Space(8)]

    public bool Active;
    public bool EvaluatedValue;


    [Tab("Audio")]
    public AudioClip[] PhotoAppear;
    public AudioClip   Buildup;
    public AudioClip   Release;


    [Tab("Settings")]
    public GameObject ValuableTextPopup;
    public GameObject UnValuableTextPopup;
    public GameObject photoPrefab;

    public GameObject currentPhotoObj;
    public GameObject currentTextObj;

    [Space(8)]

    public CameraManager  cameraManager;
    public PlayerStats    playerStats;
    public PlayerMovement playerMovement;
    public AudioManager   audioManager;
    


    void Awake()
    {
        cameraManager  = FindAnyObjectByType<CameraManager>();
        playerStats    = FindAnyObjectByType<PlayerStats>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        audioManager   = GetComponent<AudioManager>();

        Invoke("BeginEvaluation", 1);
    }

    void Update()
    {
        if(!Active) return;
        
        timer += Time.deltaTime * speed;

        if(timer > 2) //Done
        {
            if(ActivePhoto+1 == cameraManager.Photos.Count) //Finish
            {
                EndEvaluation();
                return;
            }
            if(currentPhotoObj != null) currentPhotoObj.transform.DOComplete(); Destroy(currentPhotoObj);
            if(currentTextObj  != null) currentTextObj.transform.DOComplete();  Destroy(currentTextObj);

            ActivePhoto++;
            Evaluate();
        }
        else if(timer > 1 && !EvaluatedValue) //Money Popup
        {
            EvaluatedValue = true;
            float currentValue = cameraManager.Photos.ElementAt(ActivePhoto).Value;
            totalMoney += cameraManager.Photos.ElementAt(ActivePhoto).Value;

            if(currentValue == 0) return;

            currentTextObj = Instantiate(currentValue > 0 ? ValuableTextPopup : UnValuableTextPopup, transform.position, Quaternion.identity);
            currentTextObj.transform.localScale  = new Vector3(1.5f, 1.5f, 1.5f);

            currentTextObj.GetComponent<TextPopup>().TextUpdate(currentValue);
            playerStats.ObtainMoney(currentValue);
        }
    }


    public void Evaluate()
    {
        if(currentPhotoObj != null) currentPhotoObj.transform.DOComplete(); Destroy(currentPhotoObj);
        if(currentTextObj  != null) currentTextObj. transform.DOComplete(); Destroy(currentTextObj);

        EvaluatedValue = false;

        timer = 0;
        speed += 0.65f;
        currentPhotoObj = Instantiate(photoPrefab, transform.position, Quaternion.identity);

        currentPhotoObj.transform.localScale = new Vector3(0.5f,0.5f,0.5f);

        Renderer Screen = currentPhotoObj.GetComponentInChildren<Renderer>();
        Screen.material.mainTexture = cameraManager.Photos.ElementAt(ActivePhoto).Key;

        audioManager.PlayRandomSound(PhotoAppear, 1, 1, 0.2f);
    }


    [Button]
    public void BeginEvaluation()
    {
        Active = true;
        EvaluatedValue = false;

        Evaluate();

        audioManager.PlaySound(Buildup);
    }

    public void EndEvaluation()
    {
        Active = false;

        if(currentPhotoObj != null) currentPhotoObj.transform.DOComplete(); Destroy(currentPhotoObj);
        if(currentTextObj  != null) currentTextObj. transform.DOComplete(); Destroy(currentTextObj);

        currentTextObj = Instantiate(totalMoney > 0 ? ValuableTextPopup : UnValuableTextPopup, transform.position, Quaternion.identity);
        currentTextObj.transform.localScale  = new Vector3(2, 2, 2);
        
        TextPopup text = currentTextObj.GetComponent<TextPopup>();
        text.TextUpdate(totalMoney);
        text.DecayAfter = 1.4f;

        audioManager.StopSound(Buildup);
        audioManager.PlaySound(Release);

        SceneLoader sceneLoader = FindAnyObjectByType<SceneLoader>();
        sceneLoader.StartCoroutine(sceneLoader.ChangeScene("Lobby", 2));

        cameraManager.Photos.Clear();
        cameraManager.Photos.Add(cameraManager.defaultImage, 0);
        cameraManager.TakenPhoto = false;

        playerStats.ResetItems();
    }

    [Button]
    public void Reset()
    {
        Active = false;
        EvaluatedValue = false;

        timer = 0;
        speed = 2;
        ActivePhoto = 0;
    }
}
