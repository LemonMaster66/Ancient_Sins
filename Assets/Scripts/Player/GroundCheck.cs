using System;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerSFX      playerSFX;
    private CameraFX       cameraFX;
    private Timers         timers;

    public GameObject GroundObject;
    public bool Grounded;

    void Awake()
    {
        //Assign Components
        playerMovement = GetComponentInParent<PlayerMovement>();
        playerSFX      = FindAnyObjectByType<PlayerSFX>();
        cameraFX       = FindAnyObjectByType<CameraFX>();
        timers         = GetComponentInParent<Timers>();
    }

    public bool CheckGround()
    {
        if(GroundObject != null) return true;
        else return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == playerMovement.gameObject) return;
        playerMovement.SetGrounded(true);
        Grounded = true;

        if(GroundObject == null)
        {
            if(timers.JumpBuffer > 0) playerMovement.Jump();
            else playerMovement.HasJumped = false;

            cameraFX.CMis.GenerateImpulseWithForce(Math.Clamp(playerMovement.SmoothVelocity.y, -22, 0) * (cameraFX.CMis.enabled ? 1 : 0));
            playerSFX.PlayRandomSound(playerSFX.Land, playerMovement.SmoothVelocity.y*-1/50, 1f, 0.15f, false);
            if(playerSFX.enemy != null) playerSFX.enemy.HearSound(transform.position, (50*Math.Clamp(playerMovement.SmoothVelocity.y*-1, 0,1)), 10);
        }
        
        GroundObject = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == playerMovement.gameObject) return;
        playerMovement.SetGrounded(false);
        GroundObject = null;
        Grounded = false;

        timers.CoyoteTime = 0.3f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == playerMovement.gameObject) return;
        playerMovement.SetGrounded(true);
        GroundObject = other.gameObject;
        Grounded = true;
    }
}