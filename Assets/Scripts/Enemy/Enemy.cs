using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using VInspector;
using PalexUtilities;

public class Enemy : MonoBehaviour
{
    public Transform   Target;
    public Transform   LastTarget;
    public Transform[] Points;
    public LayerMask   OcclusionLayerMask;

    [Space(10)]

    //public SerializedDictionary<Transform, float> CheckedPoints = new SerializedDictionary<Transform, float>();


    [Header("Properties")]
    public float CurrentNoisePriority;
    public float SearchDuration;
    public float AttackCooldown;


    [Header("States")]
    [Variants("Wandering", "Searching", "Hearing", "Chasing")]
    public string State;
    
    public bool Active = true;
    public bool Watched;
    public bool IgnorePlayer;
    
    [Space(10)]

    //[Foldout("Debug")]
    

    #region References
        private Camera         cam;
        private NavMeshAgent   agent;
        private PlayerMovement playerMovement;
        private Plane[]        frustumPlanes;
        private Collider       boundsCollider;
        private AudioSource    audioSource;
    #endregion

    


    void Awake()
    {
        cam = Camera.main;
        agent = GetComponentInParent<NavMeshAgent>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        boundsCollider = transform.GetChild(0).GetComponent<Collider>();
        audioSource = GetComponentInChildren<AudioSource>();
        FindAnyObjectByType<PlayerSFX>().enemy = this;
    }

    void Update()
    {
        if(SearchDuration > 0) SearchDuration = Math.Clamp(SearchDuration - Time.deltaTime, 0, math.INFINITY);
        if(AttackCooldown > 0) AttackCooldown = Math.Clamp(AttackCooldown - Time.deltaTime, 0, math.INFINITY);

        audioSource.volume = agent.velocity.magnitude/10;
    }


    void FixedUpdate()
    {
        if(playerMovement.Dead || AttackCooldown > 0 || !Active) return;

        agent.SetDestination(Target.position);

        if(!IgnorePlayer && AttackCooldown == 0)
        {
            if(Tools.FrustumCheck(boundsCollider, cam) && !Tools.OcclusionCheck(Points, playerMovement.transform, OcclusionLayerMask)) Freeze(true);
            else                                    Freeze(false);

            if(!Tools.OcclusionCheck(Points, playerMovement.transform, OcclusionLayerMask))
            {
                if(!Watched) SetState("Chasing");
                Target.position = playerMovement.transform.position;
            }
        }

        //Next Movement
        if(Tools.CalculatePathDistance(transform.position, agent.destination, agent) < 4)
        {
            MoveUpdate();
            if(IgnorePlayer || Watched || State != "Chasing") return;
            if(AttackCooldown == 0 && Vector3.Distance(transform.position, playerMovement.transform.position) < 5) Attack(90);
        }
    }


    public void Freeze(bool state)
    {
        if(state && !Watched) //Freeze
        {
            SetState("Chasing");
            Watched = true;
            agent.speed = 0;
        }
        else if(!state && Watched) //Unfreeze
        {
            SetState(Tools.OcclusionCheck(Points, playerMovement.transform, OcclusionLayerMask) ? "Searching" : "Chasing");
            Watched = false;
            if(AttackCooldown > 0) AttackCooldown = 0.25f;
        }
    }


    public Vector3 RandomNavmeshLocation(float Range = 20, float MinDistance = 0, float MaxDirectionDotDifference = -1)
    {
        int Iterations = 0;

        while(Iterations < 20)
        {
            Iterations++;
            Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized * Range;
            Vector3 dir = new Vector3(randomDirection.x, 0, randomDirection.y);

            dir += transform.position;
            if (NavMesh.SamplePosition(dir, out NavMeshHit hit, Range, 1))
            {
                Vector3 LastDir = Vector3.Normalize(Target.position - transform.position);
                Vector3 NextDir = Vector3.Normalize(hit.position    - transform.position);

                if(Tools.CalculatePathDistance(Target.position, hit.position, agent) < MinDistance) continue;
                if(Tools.CalculatePathDistance(Target.position, hit.position, agent) > Range+3)     continue;
                
                if(Vector3.Dot(LastDir, NextDir) < MaxDirectionDotDifference && Iterations < 20) continue;

                return hit.position;
            }
            return RandomNavmeshLocation(Range, MinDistance, MaxDirectionDotDifference);
        }
        // Failed all Checks
        return RandomNavmeshLocation(Range+6, MinDistance-5, MaxDirectionDotDifference + 0.25f);
    }
    public Vector3 RandomNavmeshLocationOld(float Range = 20)
    {
        Vector3 randomDirection = UnityEngine.Random.onUnitSphere * Range;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, Range, 1)) finalPosition = hit.position;
        return finalPosition;
    }


    public void MoveUpdate()
    {
        if(State == "Wandering") Target.position = RandomNavmeshLocation(30, 18, 0.4f);
        if(State == "Searching")
        {
            Target.position = RandomNavmeshLocation(20, 8, 0.4f);
            if(SearchDuration == 0) SetState("Wandering");
        }
        if(State == "Hearing")
        {
            SetState("Searching");
            CurrentNoisePriority = 0;
        }
        if(State == "Chasing")
        {
            if(!Tools.OcclusionCheck(Points, playerMovement.transform, OcclusionLayerMask)) Target.position = playerMovement.transform.position;
            else
            {
                SetState("Searching");
                MoveUpdate();
            }
        }
    }
    public void SetState(string state)
    {
        State = state;
        if(state == "Wandering") agent.speed = 20;
        if(state == "Searching") agent.speed = 32;   SearchDuration = 5;
        if(state == "Hearing")   agent.speed = 28;
        if(state == "Chasing")   agent.speed = 32;
    }


    public void Attack(float Damage = 100)
    {
        playerMovement.TakeDamage(Damage);
        AttackCooldown = 0.3f;
    }
    public void HearSound(Vector3 position, float Size = 1000, float priority = 1000)
    {
        if(State == "Chasing") return;

        if(priority >= CurrentNoisePriority)
        {
            if(Vector3.Distance(transform.position, position) > Size) return;

            DebugPlus.DrawWireSphere(position, Size).Duration(0.3f);
            SetState("Hearing");
            CurrentNoisePriority = priority;
            Target.position = position;
        }
    }
}
