using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VInspector;

public class Enemy : MonoBehaviour
{
    public Transform Target;

    [Header("Movement Physics")]
    public float Acceleration     = 50;
    public float MaxSpeed         = 80;
    public float Gravity          = 100;

    [Header("Properties")]
    public int Damage;

    [Header("States")]
    [Button] public bool Watched;
    private bool WatchedStorage;

    private NavMeshAgent   agent;
    private PlayerMovement playerMovement;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();

        Target = playerMovement.transform;
    }

    void FixedUpdate()
    {
        agent.SetDestination(Target.position);
        agent.speed = Watched ? 0 : 27.5f;

        // if(Watched != WatchedStorage)
        // {
        //     WatchedStorage = Watched;
        // }
    }

    public void Freeze()
    {
        agent.speed = 0;
    }
}
