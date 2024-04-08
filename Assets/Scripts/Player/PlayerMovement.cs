using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public int    Health;
    private int   MaxHealth = 100;

    [Header("States")]
    public bool  Grounded       = true;
    public bool  Crouching      = true;
    public bool  Running        = false;

    public bool  CanMove        = true;
    public bool  Dead           = false;
    public bool  Paused         = false;

    public bool HasJumped      = false;
    public bool HoldingCrouch  = false;


    #region Debug Stats
        [Foldout("Debug Stats")]
        public Vector3     PlayerVelocity;
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

        //Assign Scripts
        groundCheck  = GetComponentInChildren<GroundCheck>();
        timers       = GetComponent<Timers>();
        playerSFX    = FindAnyObjectByType<PlayerSFX>();

        //Component Values
        rb.useGravity = false;

        //Property Values
        Health     = MaxHealth;
        _maxSpeed  = MaxSpeed;
        _speed     = Speed;
        _gravity   = Gravity;
    }

    void FixedUpdate()
    {
        #region Physics
            
        #endregion
        //**********************************
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

            //Gravity
            rb.AddForce(Physics.gravity * Gravity /10);

            LockToMaxSpeed();

            if(Running) Speed = 200;
            else        Speed = 125;
        #endregion
        //**********************************


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

        playerSFX.PlayRandomSound(playerSFX.Jump, 1, 1, 0.15f);
    } 


    //***********************************************************************
    //***********************************************************************
    //Abilities
    public void OnRun(InputAction.CallbackContext context)
    {
        if(Paused) return;

        if(context.started)  
        {
            Running = true;
            Speed = 200;
        }
        if(context.canceled)
        {
            Running = false;
            Speed = _speed;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if(Paused) return;

        if(context.started) HoldingCrouch = true;
        if(context.canceled) HoldingCrouch = false;
    }
    public void CrouchState(bool state)
    {
        if(state && HoldingCrouch)
        {
            Crouching = true;
            MaxSpeed = _maxSpeed -10;

            transform.localScale += new Vector3(0, -1, 0);
            rb.position += new Vector3(0,-1,0);
        }
        else
        {
            Crouching = false;
            MaxSpeed = _maxSpeed;

            transform.localScale += new Vector3(0,1,0);
            rb.position += new Vector3(0,1,0);
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


    //***********************************************************************
    //***********************************************************************
    //Extra Logic

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
