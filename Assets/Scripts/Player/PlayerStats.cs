using System;
using Cinemachine;
using TMPro;
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


    private PlayerMovement playerMovement;
    private PlayerSFX      playerSFX;

    private Image _damageScreen;
    private Image _deathScreen;

    public GameObject textPopup;



    void Awake()
    {
        //Assign Scripts
        playerSFX      = FindAnyObjectByType<PlayerSFX>();
        playerMovement = GetComponent<PlayerMovement>();

        GameObject canvas = GameObject.Find("Canvas");
        _damageScreen = canvas.transform.GetChild(0).GetComponent<Image>();
        _deathScreen  = canvas.transform.GetChild(1).GetComponent<Image>();
    }

    void Update()
    {
        if(!Dead) 
        {
            Health = Math.Clamp(Health + Time.deltaTime*2, 0, 100);
            _damageScreen.color = new Color(1,1,1, (Health / 80*-1)+1);
        }
        else
        {
            _damageScreen.color = new Color(1,1,1,0);
        }
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
    
    public void ObtainMoney(float money, bool UI = false)
    {
        if(money <= 0) return;
        Money += money;
        playerSFX.PlayRandomSound(playerSFX.ObtainMoney, 1, 1, 0.1f);
        //if(UI) SpawnTextUI(money + "");
    }

    public void SpawnTextUI(string text)
    {
        GameObject canvas = GameObject.Find("Canvas");
        GameObject MoneyTextObj = Instantiate(textPopup, canvas.transform.position, Quaternion.identity, canvas.transform);
        MoneyTextObj.GetComponent<TextMeshProUGUI>().text = text;
        MoneyTextObj.transform.position = new Vector3(50, 25, 0);
    }
}
