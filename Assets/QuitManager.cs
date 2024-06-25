using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QuitManager : MonoBehaviour
{
    public bool HoldingEsc;
    public float Value = 0;
    public AudioClip WirrSfx;
    [Space(5)]
    public Slider slider;
    public PlayerSFX playerSFX;

    void Awake()
    {
        playerSFX = FindAnyObjectByType<PlayerSFX>();

        slider.gameObject.SetActive(false);

        playerSFX.PlaySound(WirrSfx, 0, 0, 0, true);
    }

    void Update()
    {
        Value = Math.Clamp(Value += Time.deltaTime * (HoldingEsc ? 1 : -2), 0, 1);
        slider.value = Value;

        slider.gameObject.SetActive(Value > 0.1f);

        playerSFX.SetValues(WirrSfx, Value, (Value*2) + 0.2f, 0, true);

        if(Value == 1) Quit();
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        if(context.started)  HoldingEsc = true;
        if(context.canceled) HoldingEsc = false;
    }

    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
