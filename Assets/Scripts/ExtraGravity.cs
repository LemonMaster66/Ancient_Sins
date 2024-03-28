using UnityEngine;

public class ExtraGravity : MonoBehaviour
{
    private Rigidbody rb;
    public float Gravity;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        //Gravity
        rb.AddForce(Physics.gravity * Gravity /10);
    }
}


