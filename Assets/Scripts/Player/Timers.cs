using System;
using UnityEngine;

public class Timers : MonoBehaviour
{
    private PlayerMovement playerMovement;

    [Header("Jump Chaining")]
    public float SlamJump;
    public float SlamJumpStorageTime;
    public bool  SlamJumpStorage;

    [Header("Dash")]
    public float DashTime;

    [Header("Slide")]
    public float SlideJumpPower = 0;
    public float SlideJumpStorage = 0;

    [Header("Quality of Life")]
    public float CoyoteTime;
    public float JumpBuffer;
    public float SlideBuffer;

    private void SlamJumpStorageFunction()
    {
        SlamJumpStorageTime -= Time.deltaTime;
        if(SlamJumpStorageTime < 0)
        {
            SlamJumpStorageTime = 0;
            SlamJumpStorage = false;
        }
        return;
    }
    private void DashFunction()
    {
        DashTime -= Time.deltaTime;
        if(DashTime <= 0.001f)
        {
            DashTime = 0;
            playerMovement.EndDash();
        }
        return;
    }
    private void CoyoteTimeFunction()
    {
        CoyoteTime -= Time.deltaTime;
        if(CoyoteTime < 0) CoyoteTime = 0;
        return;
    }
    private void JumpBufferFunction()
    {
        JumpBuffer -= Time.deltaTime;
        if(JumpBuffer < 0) JumpBuffer = 0;
        return;
    }
    private void SlideBufferFunction()
    {
        SlideBuffer -= Time.deltaTime;
        if(SlideBuffer < 0) SlideBuffer = 0;
        return;
    }
    private void SlideJumpStorageFunction()
    {
        SlideJumpStorage -= Time.deltaTime;
        if(SlideJumpStorage < 0) SlideJumpStorage = 0;
        return;
    }



    void FixedUpdate()
    {
        //Auto Countdown
        if(SlamJumpStorage)      SlamJumpStorageFunction();
        if(CoyoteTime > 0)        CoyoteTimeFunction();
        if(JumpBuffer > 0)        JumpBufferFunction();
        if(SlideJumpStorage > 0)  SlideJumpStorageFunction();
        if(SlideBuffer > 0)       SlideBufferFunction();
        if(DashTime > 0)          DashFunction();

        #region Fast Fall Logic
            if(playerMovement.FastFalling) SlamJump += 0.75f;
            else if(playerMovement.Grounded && SlamJump > 0 && !SlamJumpStorage) SlamJump = 0;
            if(SlamJump <= 0f) SlamJump = 0;
        #endregion

        #region Rounding Values
            SlamJumpStorageTime = (float)Math.Round(SlamJumpStorageTime, 2);
            SlamJump            = (float)Math.Round(SlamJump, 2);
            DashTime            = (float)Math.Round(DashTime, 2);
            CoyoteTime          = (float)Math.Round(CoyoteTime, 2);
            JumpBuffer          = (float)Math.Round(JumpBuffer, 2);
            SlideBuffer         = (float)Math.Round(SlideBuffer, 2);
            SlideJumpStorage    = (float)Math.Round(SlideJumpStorage, 2);
        #endregion
    }

    void Awake()
    {
        //Assign Components
        playerMovement = GetComponent<PlayerMovement>();
    }
}