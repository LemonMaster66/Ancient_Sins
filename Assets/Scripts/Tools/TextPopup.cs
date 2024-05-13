using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TextPopup : MonoBehaviour
{
    public bool Decay;
    public bool BillboardRotate;

    public float Money;
    public float DecayAfter;
    public float Age;

    public Gradient gradient;
    private TextMeshPro _TMPro;
    private Transform cam;
    private Animator animator;

    void Awake()
    {
        _TMPro   = GetComponent<TextMeshPro>();
        animator = GetComponent<Animator>();
        cam      = Camera.main.transform;

        transform.DOPunchRotation(new Vector3(0,0,25), 0.2f, UnityEngine.Random.Range(15, 30), 1);
        transform.DOPunchScale(new Vector3(0.1f,0.1f,0.1f), 0.2f, UnityEngine.Random.Range(15, 30), 1);
    }

    void LateUpdate()
    {
        if(Decay)
        {
            Age += Time.deltaTime;
            if(Age >= DecayAfter) animator.Play("Fade");
        }

        Vector3 rot = transform.eulerAngles;
        if(BillboardRotate) transform.LookAt(transform.position + cam.forward);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, rot.z);
    }


    public void DamageUpdate(float UpdatedMoney)
    {
        Money = UpdatedMoney;
        Money = (float)Math.Round(Money, 0);

        _TMPro.text = Money + "";
        gameObject.name = Money + "";
    }

    public void DestroyText()
    {
        Destroy(gameObject);
    }
}
