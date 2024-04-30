using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using VInspector;

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
    
    public bool Active;
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
    }

    void Update()
    {
        if(SearchDuration > 0) SearchDuration = Math.Clamp(SearchDuration - Time.deltaTime, 0, math.INFINITY);
        if(AttackCooldown > 0) AttackCooldown = Math.Clamp(AttackCooldown - Time.deltaTime, 0, math.INFINITY);

        audioSource.volume = agent.velocity.magnitude/10;
    }


    void FixedUpdate()
    {
        if(playerMovement.Dead || AttackCooldown > 0) return;

        agent.SetDestination(Target.position);

        if(!IgnorePlayer && AttackCooldown == 0)
        {
            if(FrustumCheck() && !OcclusionCheck()) Freeze(true);
            else                                    Freeze(false);

            if(!OcclusionCheck())
            {
                if(!Watched) SetState("Chasing");
                Target.position = playerMovement.transform.position;
            }
        }

        //Next Movement
        if(CalculatePathDistance(transform.position, agent.destination) < 4)
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
            SetState(OcclusionCheck() ? "Searching" : "Chasing");
            Watched = false;
            if(AttackCooldown > 0) AttackCooldown = 0.25f;
        }
    }

    
    public bool FrustumCheck()   // True if its in the Cameras Bounds
    {
        Bounds bounds = boundsCollider.bounds;
        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);

        return GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
    }
    public bool OcclusionCheck() // True if its Occluded
    {
        foreach(Transform transform in Points)
        {
            //if it Does hit something
            if(Physics.Raycast(transform.position, playerMovement.transform.position-transform.position, out RaycastHit hit, 100000, OcclusionLayerMask))
            {
                // If it hits the Player
                if(hit.transform.tag == "Player") return false;
                     //DrawThickRay(transform.position, cam.transform.position-transform.position, Color.green, 0, 0.0065f);
                //else DrawThickRay(transform.position, cam.transform.position-transform.position, Color.red, 0, 0.0065f);
            }
        }
        return true;
    }


    public Vector3 RandomNavmeshLocation(float Range = 20, float MinDistance = 0, float MaxDirectionDotDifference = -1)
    {
        int Iterations = 0;
        LastTarget.position = Target.position;

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

                if(CalculatePathDistance(Target.position, hit.position) < MinDistance) continue;
                if(CalculatePathDistance(Target.position, hit.position) > Range+3)     continue;
                
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
    float CalculatePathDistance(Vector3 startPos, Vector3 TargetPos)
    {
        NavMeshPath path = new NavMeshPath();
        float distance = 0;

        if(NavMesh.CalculatePath(startPos, TargetPos, agent.areaMask, path))
        {
            for(int i = 1; i < path.corners.Length; i++)
            {
                distance += Vector3.Distance(path.corners[i-1], path.corners[i]);
            }
        }
        return distance;
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
            if(!OcclusionCheck()) Target.position = playerMovement.transform.position;
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



    void DrawThickRay(Vector3 start, Vector3 dir, Color color, float duration, float Thickness)
    {
        for(int i = 0; i < 200; i++)
        {
            start.x += UnityEngine.Random.Range(Thickness, -Thickness);
            start.y += UnityEngine.Random.Range(Thickness, -Thickness);
            start.z += UnityEngine.Random.Range(Thickness, -Thickness);
            Debug.DrawRay(start, dir, color, duration);
        }
    }
}
