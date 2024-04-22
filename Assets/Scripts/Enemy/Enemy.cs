using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VInspector;

public class Enemy : MonoBehaviour
{
    public Transform   Target;
    public Transform[] Points;
    public LayerMask   OcclusionLayerMask;


    [Header("States")]
    [Variants("Wandering", "Chasing", "Searching")]
    public string State;
    public bool Watched;
    
    [Space(10)]

    //[Foldout("Debug")]
    private Camera         cam;
    private NavMeshAgent   agent;
    private PlayerMovement playerMovement;
    private Plane[]        frustumPlanes;
    private Collider       boundsCollider;


    void Awake()
    {
        cam = Camera.main;
        agent = GetComponentInParent<NavMeshAgent>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        boundsCollider = transform.GetChild(0).GetComponent<Collider>();
    }


    void FixedUpdate()
    {
        //if(FrustumCheck() && !OcclusionCheck()) Freeze(true);
        //else                                    Freeze(false);

        //if(!OcclusionCheck()) Target.position = playerMovement.transform.position;

        agent.SetDestination(Target.position);

        if(Vector3.Distance(transform.position, Target.position) < 5) RandomNavmeshLocation();
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
            Watched = false;
            agent.speed = 28;
        }
    }

    // if its in the Cameras Bounds
    public bool FrustumCheck()
    {
        Bounds bounds = boundsCollider.bounds;
        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);

        if(GeometryUtility.TestPlanesAABB(frustumPlanes, bounds)) return true;
        else return false;
    }

    // if its occluded
    public bool OcclusionCheck()
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

    [Button]
    public void RandomNavmeshLocation()
    {
        Vector3 randomDirection = Random.onUnitSphere * 30;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, 30, 1)) finalPosition = hit.position;
        //return finalPosition;
        Target.position = finalPosition;
    }
}
