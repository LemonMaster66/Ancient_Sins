using System;
using Cinemachine;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class PlayerStats : MonoBehaviour
{
    [Tab("Main")]
    [Header("Properties")]
    public float Health = 100;
    public float Money  = 0;


    [Header("States")]
    public bool Dead = false;

    
    [Tab("Items")]
    public int extraFilm = 0;       // Costs: 25   |  Gives: +20    Film
    public int renderSpeed = 0;     // Costs: 50   |  Gives: -0.25  Render Speed
    public int adrenalineShot = 0;  // Costs: 75   |  Gives: +10    Speed
    public int SmokeBomb = 0;       // Costs: 100  |  Gives: +1     Smoke Bomb

    [Space(6)]

    public GameObject SmokeBombPrefab;


    [Tab("Settings")]
    public PlayerMovement playerMovement;
    public PlayerSFX      playerSFX;
    public CameraManager  cameraManager;
    public Image _damageScreen;
    public Image _deathScreen;
    public GameObject textPopup;



    void Awake()
    {
        //Assign Scripts
        playerSFX      = FindAnyObjectByType<PlayerSFX>();
        playerMovement = GetComponent<PlayerMovement>();
        cameraManager  = FindAnyObjectByType<CameraManager>();

        GameObject canvas = GameObject.Find("Canvas");
        if(canvas != null) _damageScreen = canvas.transform.GetChild(0).GetComponent<Image>();
        if(canvas != null) _deathScreen  = canvas.transform.GetChild(1).GetComponent<Image>();
    }

    void Update()
    {
        if(!Dead) 
        {
            Health = Math.Clamp(Health + Time.deltaTime*2, 0, 100);
            if(_damageScreen != null) _damageScreen.color = new Color(1,1,1, (Health / 80*-1)+1);
        }
        else if(_damageScreen != null) _damageScreen.color = new Color(1,1,1,0);

        if(Input.GetKeyDown(KeyCode.Q)) UseSmokeBomb();
    }


    public void TakeDamage(float Damage = 100)
    {
        Health -= Damage;
        playerSFX.enemy.GetComponent<CinemachineImpulseSource>().GenerateImpulseWithForce(-2);
        playerSFX.PlaySound(playerSFX.Damage);
        if(Health < 0) Die();
    }
    public void Die()
    {
        Dead = true;
        _deathScreen.color = new Color(1,1,1,1);
        playerSFX.enemy.GetComponent<CinemachineImpulseSource>().GenerateImpulseWithForce(-5);
        playerSFX.StopSound(playerSFX.Damage);
        playerSFX.PlaySound(playerSFX.Death);
    }
    
    public void ObtainMoney(float money, bool ChaChing = false)
    {
        if(money == 0) return;
        Money += money;
        if(money > 0 || ChaChing) playerSFX.PlayRandomSound(playerSFX.ObtainMoney, 1, 1, 0.1f);
        else if(money < 0)        playerSFX.PlayRandomSound(playerSFX.LoseMoney,   1, 1, 0.1f);
    }

    [Button]
    public void UsePassiveItems()
    {
        cameraManager.FilmLength += 10 * extraFilm;

        cameraManager.CaptureCooldown -= 0.35f * renderSpeed;

        playerMovement.extraSpeed += 10 * adrenalineShot;
        playerMovement.Speed = playerMovement._speed + playerMovement.extraSpeed;
    }

    public void ResetItems()
    {
        cameraManager.FilmLength = 40;
        cameraManager.CaptureCooldown = 1.5f;
        playerMovement.extraSpeed = 0;
        playerMovement.Speed = playerMovement._speed;

        extraFilm = 0;
        renderSpeed = 0;
        adrenalineShot = 0;
        SmokeBomb = 0;
    }

    public void UseSmokeBomb()
    {
        if(SmokeBomb == 0) return;
        if(FindAnyObjectByType<SmokeBomb>() != null) return;

        Instantiate(SmokeBombPrefab, transform.position, Quaternion.identity);
    }

    public void SpawnTextUI(string text)
    {
        GameObject canvas = GameObject.Find("Canvas");
        GameObject MoneyTextObj = Instantiate(textPopup, canvas.transform.position, Quaternion.identity, canvas.transform);
        MoneyTextObj.GetComponent<TextMeshProUGUI>().text = text;
        MoneyTextObj.transform.position = new Vector3(50, 25, 0);
    }
}
