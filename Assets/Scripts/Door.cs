using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool Open;

    
    public float Speed;
    public float Distance = 5;

    private Vector3 TargetPosition;
    private Vector3 OriginPosition;
    public  Vector3 OpenPosition;

    private PlayerMovement playerMovement;


    private void Start()
    {
        OriginPosition = transform.position;
        TargetPosition = OriginPosition;
        playerMovement = FindAnyObjectByType<PlayerMovement>();
    }

    private void Update()
    {
        transform.position = Vector3.Slerp(transform.position, TargetPosition, Speed);
        if(Vector3.Distance(OriginPosition, playerMovement.transform.position) < Distance)
        {
            if(!Open) OpenDoor();
        }
        else
        {
            if(Open) CloseDoor();
        }
    }


    private void OpenDoor()
    {
        Open = true;
        TargetPosition += OpenPosition;
    }

    private void CloseDoor()
    {
        Open = false;
        TargetPosition = OriginPosition;
    }
}
