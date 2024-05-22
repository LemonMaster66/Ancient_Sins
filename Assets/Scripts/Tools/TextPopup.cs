using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TextPopup : Popup
{
    private TextMeshPro _TMPro;
    public float Money;

    public override void Awake()
    {
        base.Awake();
        _TMPro = GetComponent<TextMeshPro>();
    }


    public void TextUpdate(float UpdatedMoney)
    {
        Money = UpdatedMoney;
        Money = (float)Math.Round(Money, 0);

        _TMPro.text = Money + "";
        gameObject.name = Money + "";
    }
}
