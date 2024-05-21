using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VInspector;

public class MoneyEvaluate : MonoBehaviour
{
    public float speed = 10;
    public float totalMoney = 0;
    public float timer = 0;

    [Space(5)]

    public int ActivePhoto = 0;

    [Space(5)]

    public bool Active;
    public bool EvaluatedValue;

    [Space(8)]

    public GameObject textPopup;
    public GameObject photoPrefab;
    public GameObject currentPhotoObj;
    public GameObject currentTextObj;

    //public Transform currentTextObj;

    [Space(5)]

    public CameraManager  cameraManager;
    public PlayerStats    playerStats;
    public PlayerMovement playerMovement;
    


    void Awake()
    {
        cameraManager = FindAnyObjectByType<CameraManager>();
        playerStats = FindAnyObjectByType<PlayerStats>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
    }

    void Update()
    {
        if(!Active) return;
        timer += Time.deltaTime * speed;

        if(timer > 2) //Done
        {
            if(ActivePhoto+1 == cameraManager.Photos.Count) //Finish
            {
                Debug.Log("Done");
                Active = false;
                //Destroy(currentPhotoObj);
                return;
            }

            //Destroy(currentPhotoObj);
            //Destroy(currentPhotoObj);
            ActivePhoto++;
            Evaluate();
        }
        else if(timer > 1 && !EvaluatedValue) //Money Popup
        {
            EvaluatedValue = true;
            currentTextObj = Instantiate(textPopup, transform.position, Quaternion.identity);
            playerStats.Money += cameraManager.Photos.ElementAt(ActivePhoto).Value;
        }
    }

    [Button]
    public void Evaluate()
    {
        Active = true;
        EvaluatedValue = false;
        timer = 0;
        speed += 0.25f;
        currentPhotoObj = Instantiate(photoPrefab, transform.position, Quaternion.identity);

        currentPhotoObj.transform.position += new Vector3(90, 0, 0);
        currentPhotoObj.transform.eulerAngles += new Vector3(90, 0, 0);
        currentPhotoObj.transform.localScale = new Vector3(0.5f,0.5f,0.5f);

        Renderer Screen = currentPhotoObj.GetComponent<Renderer>();
        Screen.material.mainTexture = cameraManager.Photos.ElementAt(ActivePhoto).Key;
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
