using UnityEngine;
using System.Collections;

public class Spring : MonoBehaviour
{


    public Rigidbody targetObject;
    public float spring;
    public float minDistance = 0;
    public float maxDistance = 0;

    public float distance;


    void Start()
    {

    }



    void FixedUpdate()
    {
        distance = Mathf.Sqrt(Mathf.Pow(transform.position.x - targetObject.transform.position.x, 2) + Mathf.Pow(transform.position.y - targetObject.transform.position.y, 2) + Mathf.Pow(transform.position.z - targetObject.transform.position.z, 2));


     /*   if (distance <= minDistance)
        {
            targetObject.AddExplosionForce((spring,( new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z)), distance);
        }
        */
    }
}
