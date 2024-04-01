using System;
using UnityEngine;

public class Timers : MonoBehaviour
{
    private PlayerMovement playerMovement;

    [Header("Quality of Life")]
    public float CoyoteTime;
    public float JumpBuffer;


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


    void FixedUpdate()
    {
        //Auto Countdown
        if(CoyoteTime > 0) CoyoteTimeFunction();
        if(JumpBuffer > 0) JumpBufferFunction();

        #region Rounding Values
            CoyoteTime          = (float)Math.Round(CoyoteTime, 2);
            JumpBuffer          = (float)Math.Round(JumpBuffer, 2);
        #endregion
    }

    void Awake()
    {
        //Assign Components
        playerMovement = GetComponent<PlayerMovement>();
    }
}