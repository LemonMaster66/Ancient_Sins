using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Physics")]
    public float Speed      = 50;
    public float MaxSpeed   = 80;
    public float JumpForce  = 8;
    public float Gravity    = 100;


    [Header("Properties")]
    public int    Health;
    private int   MaxHealth = 100;
    [Space(10)]
    public float  DashCount = 3;
    private float DashUsedStorage = 3;
    public float  DashDuration = 0.2f;
    public float  DashPower = 10f;
    [Space(10)]
    public float  SlideJumpPower = 3;


    [Header("States")]
    public bool Grounded       = true;
    public bool AgainstWall    = true;

    public bool CanMove        = true;
    public bool Dead           = false;
    public bool Paused         = false;

    public bool Dashing        = false;
    public bool LongJumping    = false;
    public bool Sliding        = false;
    public bool SlideJumping   = false;
    public bool FastFalling    = false;

    public bool HasJumped      = false;
    public bool HoldingCrouch  = false;


    #region Debug Stats
        [Header("Debug Stats")]
        public Vector3     PlayerVelocity;
    private Vector3 LastFrameVelocity;
    public float       VelocityMagnitudeXZ;
        public float       ForwardVelocityMagnitude;
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
        Health    = MaxHealth;
        _maxSpeed = MaxSpeed;
        _speed = Speed;
        _gravity = Gravity;
    }

    void FixedUpdate()
    {
        #region Physics Stuff
            //Gravity
            rb.AddForce(Physics.gravity * Gravity /10);
        #endregion
        //**********************************
        #region PerFrame Calculations
            //Camera Orientation
            CamF = Camera.forward;
            CamR = Camera.right;
            CamF.y = 0;
            CamR.y = 0;
            CamF = CamF.normalized;
            CamR = CamR.normalized;

            //Rigidbody Velocity Magnitude on the X/Z Axis
            VelocityMagnitudeXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

            // Calculate the Forward velocity magnitude
            Vector3 ForwardVelocity = Vector3.Project(rb.velocity, CamF);
            ForwardVelocityMagnitude = ForwardVelocity.magnitude;
            ForwardVelocityMagnitude = (float)Math.Round(ForwardVelocityMagnitude, 2);

            //Dash Managment
            if(DashCount < 3 && !Sliding) DashCount += 0.015f;
            if(DashCount > 3) DashCount = 3;
            if(DashCount < 0) DashCount = 0;

            if(Math.Floor(DashCount) -1 == DashUsedStorage)
            {
                DashUsedStorage++;
                playerSFX.PlayRandomSound(playerSFX.RefreshDash, 0.6f + DashCount/3, 1, 0, false);
            }

            if(Grounded && SlideJumpPower > 5 && timers.SlideJumpStorage == 0) SlideJumpPower = 5;

            LockToMaxSpeed();
        #endregion
        //**********************************
        #region Conditions
            if(Grounded) Speed = _speed;   //Grounded
            else if(!Grounded) Speed = 35; //Not Grounded

            //Sliding
            if(Sliding) rb.velocity += Movement *8;
            //Fast Falling
            if(FastFalling) timers.SlamJumpStorageTime = 0.3f;

            //CanMove Check
            if(Sliding || FastFalling || Dashing || Paused) CanMove = false;
            else CanMove = true;
        #endregion
        //**********************************

        // Main Movement Code
        if(CanMove)
        {
            Movement = (CamF * MovementY + CamR * MovementX).normalized;
            rb.AddForce(Movement * Speed);
        }

        #region Rounding Values
            PlayerVelocity      = rb.velocity;
            PlayerVelocity.x    = (float)Math.Round(PlayerVelocity.x, 2);
            PlayerVelocity.y    = (float)Math.Round(PlayerVelocity.y, 2);
            PlayerVelocity.z    = (float)Math.Round(PlayerVelocity.z, 2);
            VelocityMagnitudeXZ = (float)Math.Round(VelocityMagnitudeXZ, 2);
            DashCount           = (float)Math.Round(DashCount, 2);
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

        if(MovementX == 0 && MovementY == 0) playerSFX.stepTimer = 0;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(Paused) return;
        if(context.started && !Dead) JumpAll();
    }
    public void JumpAll()
    {
        if((Grounded || timers.CoyoteTime > 0) && !HasJumped) Jump();
        else if(!Grounded || timers.CoyoteTime == 0) timers.JumpBuffer = 0.15f;
    }
    
    #region Jump Types
    public void Jump()
    {
        HasJumped = true;
        float JumpHeight = JumpForce;
        
        playerSFX.PlaySound(playerSFX.Jump, 1f, 1f, 0.15f, false);

        if(!SlideJumping) JumpHeight += timers.SlamJump;
        rb.AddForce(Vector3.up * JumpHeight, ForceMode.VelocityChange);
    }
        
    #endregion
    


    //***********************************************************************
    //***********************************************************************
    //Abilities

    public void OnDash(InputAction.CallbackContext context)
    {
        if(Paused) return;
        if(context.started && !Dead && !Dashing && DashCount > 1)
        {
            Dashing = true;
            LongJumping = false;
            DashCount--;
            DashUsedStorage--;
            Gravity = 0;
            MaxSpeed = DashPower;
            timers.DashTime = DashDuration;

            SlideJumpPower  = 0;
            timers.SlamJump = 0;

            GetComponent<Collider>().material.dynamicFriction = 0;

            if(MovementX == 0 && MovementY == 0) Movement = CamF;

            rb.velocity = Movement * 100;

            playerSFX.PlaySound(playerSFX.Dash, 1, 0.4f, 0.1f, false);
        }
        else if(context.started && DashCount < 1) playerSFX.PlaySound(playerSFX.NoDashLeft, 1, 1, 0.1f, false);
    }
    public void EndDash()
    {
        Dashing = false;
        Gravity = _gravity;
        if(!LongJumping) MaxSpeed = _maxSpeed;

        GetComponent<Collider>().material.dynamicFriction = 9;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if(Paused) return;
        //Start Pressing Ctrl
        if(context.started && !Dead)
        {
            HoldingCrouch = true;

            if(!Grounded)
            {
                timers.SlideBuffer = 0.08f;
                SlideJumpPower = VelocityMagnitudeXZ;
            }

            //Fast Fall
            if(!Grounded && !FastFalling)
            {
                FastFalling = true;
                timers.SlamJumpStorage = true;
                timers.SlamJumpStorageTime = 0.25f;
                timers.SlamJump /= 3f;

                rb.velocity = new Vector3(0, rb.velocity.y, 0);
                rb.AddForce(Vector3.down * 85, ForceMode.VelocityChange);
            }
            //Slide
            else if(Grounded) SlideState(true);
        }
        //Stop Pressing Ctrl
        if(context.canceled && !Dead)
        {
            HoldingCrouch = false;
            if(Sliding) SlideState(false);
        }
    }
    public void SlideState(bool state)
    {
        if(state && HoldingCrouch)
        {
            Sliding = true;
            MaxSpeed = _maxSpeed +5;
            GetComponent<Collider>().material.dynamicFriction = 0;

            transform.localScale += new Vector3(0, -1, 0);
            rb.position += new Vector3(0,-1,0);

            if(MovementX == 0 && MovementY == 0) Movement = CamF;
        }
        else
        {
            Sliding = false;
            GetComponent<Collider>().material.dynamicFriction = 9;

            transform.localScale += new Vector3(0,1,0);
            rb.position += new Vector3(0,1,0);

            if(!HasJumped) groundCheck.GroundState(true);
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
    public void SetAgainstWall(bool state) 
    {
        AgainstWall = state;
    }

    public bool WalkingCheck()
    {
        if(MovementX != 0 || MovementY != 0)
        {
            if(Grounded && !Sliding) return true;
            else return false;
        }
        else return false;
    }
}
