using System;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VInspector;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Physics")]
    public float Speed            = 50;
    public float MaxSpeed         = 80;
    public float CounterMovement  = 10;
    public float JumpForce        = 8;
    public float Gravity          = 100;

    [Header("Properties")]
    public float Health = 100;

    [Header("States")]
    public bool  Grounded       = true;
    public bool  Crouching      = false;
    public bool  Running        = false;

    public bool  CanMove        = true;
    public bool  Dead           = false;
    public bool  Paused         = false;

    public bool HasJumped      = false;
    public bool HoldingCrouch  = false;
    public bool HoldingRun     = false;


    #region Debug Stats
        [Foldout("Debug Stats")]
        public Vector3     TargetScale;
        public Vector3     PlayerVelocity;
        public Vector3     SmoothVelocity;
        public float       VelocityMagnitude;
        [HideInInspector]  public float   ForwardVelocityMagnitude;
        [HideInInspector]  public Vector3 VelocityXZ;
        [HideInInspector]  public Vector3 CamF;
        [HideInInspector]  public Vector3 CamR;
        [HideInInspector]  public Vector3 Movement;
        [HideInInspector]  public float   MovementX;
        [HideInInspector]  public float   MovementY;
        [HideInInspector]  public float   _speed;
        [HideInInspector]  public float   _maxSpeed;
        [HideInInspector]  public float   _gravity;

        private Image _damageScreen;
        private Image _deathScreen;
    #endregion
    
    
    #region Script / Component Reference
        [HideInInspector] public Rigidbody    rb;
        [HideInInspector] public Transform    Camera;

        private Timers       timers;
        private GroundCheck  groundCheck;
        private PlayerSFX    playerSFX;
    #endregion


    void Awake()
    {
        //Assign Components
        Camera  = GameObject.Find("Main Camera").transform;
        rb      = GetComponent<Rigidbody>();

        GameObject canvas = GameObject.Find("Canvas");
        _damageScreen = canvas.transform.GetChild(0).GetComponent<Image>();
        _deathScreen  = canvas.transform.GetChild(1).GetComponent<Image>();

        //Assign Scripts
        groundCheck  = GetComponentInChildren<GroundCheck>();
        timers       = GetComponent<Timers>();
        playerSFX    = FindAnyObjectByType<PlayerSFX>();

        //Component Values
        rb.useGravity = false;

        //Property Values
        _maxSpeed  = MaxSpeed;
        _speed     = Speed;
        _gravity   = Gravity;
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

        if(Input.GetKeyDown(KeyCode.E))
        {
            rb.AddForce(CamF * 5000);
        }
    }

    void FixedUpdate()
    {
        #region PerFrame stuff
            #region Camera Orientation Values
                CamF = Camera.forward;
                CamR = Camera.right;
                CamF.y = 0;
                CamR.y = 0;
                CamF = CamF.normalized;
                CamR = CamR.normalized;

                //Rigidbody Velocity Magnitude on the X/Z Axis
                VelocityXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z);

                // Calculate the Forward velocity magnitude
                Vector3 ForwardVelocity = Vector3.Project(rb.velocity, CamF);
                ForwardVelocityMagnitude = ForwardVelocity.magnitude;
                ForwardVelocityMagnitude = (float)Math.Round(ForwardVelocityMagnitude, 2);
            #endregion

            SmoothVelocity = Vector3.Slerp(SmoothVelocity, rb.velocity, 0.15f);
            SmoothVelocity.x    = (float)Math.Round(SmoothVelocity.x, 4);
            SmoothVelocity.y    = (float)Math.Round(SmoothVelocity.y, 4);
            SmoothVelocity.z    = (float)Math.Round(SmoothVelocity.z, 4);

            //Gravity
            rb.AddForce(Physics.gravity * Gravity /10);

            LockToMaxSpeed();
        #endregion
        //**********************************

        transform.localScale = Vector3.Slerp(transform.localScale, TargetScale, 0.175f);

        // Movement Code
        if(!Paused && !Dead && CanMove)
        {
            Movement = (CamF * MovementY + CamR * MovementX).normalized;
            rb.AddForce(Movement * Speed);
        }

        //CounterMovement
        rb.AddForce(VelocityXZ * -(CounterMovement / 10));

        #region Rounding Values
            PlayerVelocity      = rb.velocity;
            PlayerVelocity.x    = (float)Math.Round(PlayerVelocity.x, 2);
            PlayerVelocity.y    = (float)Math.Round(PlayerVelocity.y, 2);
            PlayerVelocity.z    = (float)Math.Round(PlayerVelocity.z, 2);
            VelocityMagnitude   = (float)Math.Round(rb.velocity.magnitude, 2);
        #endregion
    }

    //***********************************************************************
    //***********************************************************************
    //Movement Functions
    public void OnMove(InputAction.CallbackContext MovementValue)
    {
        if(Paused) return;
        Vector2 inputVector = MovementValue.ReadValue<Vector2>();
        MovementX = inputVector.x;
        MovementY = inputVector.y;

        //if(MovementX == 0 && MovementY == 0) playerSFX.stepTimer = 0;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(Paused) return;
        if(context.started && !Dead)
        {
            if((Grounded || timers.CoyoteTime > 0) && !HasJumped) Jump();
            else if(!Grounded || timers.CoyoteTime == 0) timers.JumpBuffer = 0.15f;
        }
    }
    public void Jump()
    {
        HasJumped = true;
        rb.velocity = new Vector3(rb.velocity.x, math.clamp(rb.velocity.y, 0, math.INFINITY), rb.velocity.z);
        rb.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);

        CrouchState(false);

        playerSFX.PlayRandomSound(playerSFX.Jump, 1, 1, 0.15f);
    } 

    public void OnRun(InputAction.CallbackContext context)
    {
        if(Paused) return;

        if(context.started && !Crouching)
        {
            HoldingRun = true;
            RunState(true);
        }
        if(context.canceled) 
        {
            HoldingRun = false;
            RunState(false);
        }
    }
    public void RunState(bool state)
    {
        if(state)
        {
            Running = true;
            Speed = 200;
        }
        else
        {
            Running = false;
            Speed = _speed;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if(Paused) return;

        if(context.started && !Crouching) 
        {
            HoldingCrouch = true;
            CrouchState(true);
        }
        if(context.canceled && Crouching)
        {
            HoldingCrouch = false;
            CrouchState(false);
        }
    }
    public void CrouchState(bool state)
    {
        if(state)
        {
            RunState(false);

            Crouching = true;
            TargetScale.y = 0.5f;
            Speed = 50;
        }
        else
        {
            Crouching = false;
            TargetScale.y = 1.5f;
            Speed = _speed;

            if(HoldingRun) RunState(true);
        }
    }


    //***********************************************************************
    //***********************************************************************
    //Extra Logic
    public void TakeDamage(float Damage = 100)
    {
        Health -= Damage;
        playerSFX.enemy.GetComponent<CinemachineImpulseSource>().GenerateImpulseWithForce(-2);
        playerSFX.PlaySound(playerSFX.Damage);
        if(Health < 0)
        {
            Dead = true;
            _deathScreen.color = new Color(1,1,1,1);
            playerSFX.enemy.GetComponent<CinemachineImpulseSource>().GenerateImpulseWithForce(-5);
            playerSFX.StopSound(playerSFX.Damage);
            playerSFX.PlaySound(playerSFX.Death);
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            if(Paused) Paused = false;
            else       Paused = true;

            Debug.Log("InputLock = " + Paused);
        }
    }

    public void LockToMaxSpeed()
    {
        // Get the velocity direction
        Vector3 newVelocity = rb.velocity;
        newVelocity.y = 0f;
        newVelocity = Vector3.ClampMagnitude(newVelocity, MaxSpeed);
        newVelocity.y = rb.velocity.y;
        rb.velocity = newVelocity;
    }

    public void SetGrounded(bool state) 
    {
        Grounded = state;
    }

    public bool WalkingCheck()
    {
        if(MovementX != 0 || MovementY != 0)
        {
            if(Grounded) return true;
            else return false;
        }
        else return false;
    }
}
