using System.Collections;
using System.Collections.Generic;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.AI;
using VInspector;

public class Enemy : MonoBehaviour
{
    public Transform   Target;
    public Transform   LastTarget;
    public Transform[] Points;
    public LayerMask   OcclusionLayerMask;


    [Header("Properties")]
    public float SearchDuration;


    [Header("States")]
    [Variants("Wandering", "Chasing", "Searching", "Nigerundayo")]
    public string State;
    
    public bool Watched;
    public bool IgnorePlayer;
    
    [Space(10)]

    [Foldout("Debug")]
    private Camera         cam;
    private NavMeshAgent   agent;
    private PlayerMovement playerMovement;
    private Plane[]        frustumPlanes;
    private Collider       boundsCollider;
    private float _searchDuration;

    private AudioSource audioSource;


    void Awake()
    {
        cam = Camera.main;
        agent = GetComponentInParent<NavMeshAgent>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        boundsCollider = transform.GetChild(0).GetComponent<Collider>();

        audioSource = GetComponentInChildren<AudioSource>();
    }


    void FixedUpdate()
    {
        agent.SetDestination(Target.position);

        if(!IgnorePlayer)
        {
            if(FrustumCheck() && !OcclusionCheck()) Freeze(true);
            else                                    Freeze(false);

            if(!OcclusionCheck()) Target.position = playerMovement.transform.position;
        }

        //Next Movement
        if(Vector3.Distance(transform.position, agent.destination) < 5) Target.position = RandomNavmeshLocation(20, 8, 0.4f);

        audioSource.volume = agent.speed;
    }


    public void Freeze(bool state)
    {
        if(state && !Watched)
        {
            Watched = true;
            agent.speed = 0;
        }
        else if(!state && Watched)
        {
            State = "Chasing";
            Watched = false;
            agent.speed = 25;
        }
    }

    
    public bool FrustumCheck()   // True if its in the Cameras Bounds
    {
        Bounds bounds = boundsCollider.bounds;
        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);

        if(GeometryUtility.TestPlanesAABB(frustumPlanes, bounds)) return true;
        else return false;
    }
    public bool OcclusionCheck() // True if its Occluded
    {
        bool Occluded = true;

        //True = Occluded
        foreach(Transform transform in Points)
        {
            //if it Does hit something
            if(Physics.Raycast(transform.position, playerMovement.transform.position-transform.position, out RaycastHit hit, 100000, OcclusionLayerMask))
            {
                // If it hits the Player
                if(hit.transform.tag == "Player")
                {
                    Debug.DrawRay(transform.position, cam.transform.position-transform.position, Color.green);
                    Occluded = false;
                }
                else
                {
                    Debug.DrawRay(transform.position, cam.transform.position-transform.position, Color.red);
                }
            }
        }
        if(Occluded) return true;
        else         return false;
    }


    public Vector3 RandomNavmeshLocation(float Range = 20, float MinDistance = 0, float MaxDirectionDotDifference = -1)
    {
        int Iterations = 0;
        LastTarget.position = Target.position;

        while(Iterations < 10)
        {
            Iterations++;
            Vector2 randomDirection = Random.insideUnitCircle.normalized * Range;
            Vector3 dir = new Vector3(randomDirection.x, 0, randomDirection.y);

            dir += transform.position;
            if (NavMesh.SamplePosition(dir, out NavMeshHit hit, Range, 1))
            {
                Vector3 LastDir = Vector3.Normalize(Target.position - transform.position);
                Vector3 NextDir = Vector3.Normalize(hit.position    - transform.position);

                if(CalculatePathDistance(Target.position, hit.position) < MinDistance || CalculatePathDistance(Target.position, hit.position) > Range+3)
                {
                    //DebugPlus.DrawSphere(hit.position, 1).Color(Color.red).Duration(0.3f);
                    continue;
                }
                
                if(Vector3.Dot(LastDir, NextDir) < MaxDirectionDotDifference && Iterations < 10)
                {
                    //DrawThickRay(transform.position, NextDir*6, Color.red, 0.5f, 0.015f);
                    continue;
                }
                
                //DebugPlus.DrawSphere(hit.position, 1).Color(Color.green).Duration(0.4f);
                //DebugPlus.DrawWireSphere(Target.position, Range).Color(Color.white).Duration(0.4f);
                
                //DrawThickRay(transform.position, LastDir*10, Color.white, 0.5f, 0.015f);
                //DrawThickRay(transform.position, NextDir*15, Color.green, 0.5f, 0.015f);

                return hit.position;
            }
            return RandomNavmeshLocation(Range, MinDistance, MaxDirectionDotDifference);
        }
        // Failed all Checks
        return RandomNavmeshLocation(Range+6, MinDistance-5, MaxDirectionDotDifference + 0.25f);
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

    void DrawThickRay(Vector3 start, Vector3 dir, Color color, float duration, float Thickness)
    {
        for(int i = 0; i < 200; i++)
        {
            start.x += Random.Range(Thickness, -Thickness);
            start.y += Random.Range(Thickness, -Thickness);
            start.z += Random.Range(Thickness, -Thickness);
            Debug.DrawRay(start, dir, color, duration);
        }
    }
}
