using UnityEngine;
using System.Collections;

public class DynamicPointFollower : MonoBehaviour {

    public GameObject target;
    public float acceleration;
    
    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void FixedUpdate () {
        
        rigidbody.AddForce(computeAcceleration(
            rigidbody.position,
            rigidbody.velocity,
            target.GetComponent<Transform>().position,
            this.acceleration
        ));

    }

    // computes the acceleration given appropriate input
    // it's a static method so it can be called from RRT
    public static Vector3 computeAcceleration(Vector3 pos, Vector3 velocity, Vector3 goal, float maxAccel) {
        Vector3 targetdir = goal-pos;
        Vector3 targetVelocity = Mathf.Sqrt(2*maxAccel*targetdir.magnitude) * targetdir.normalized;
        Vector3 velocitydiff = 10*(targetVelocity - velocity);

        if (Physics.Raycast(pos, velocity, velocity.magnitude*velocity.magnitude/maxAccel/2)) {
            // if we keep this trajectory, we'll hit a wall !
            velocitydiff -= velocity.normalized * maxAccel * 10;
        }

        if (velocitydiff.magnitude > maxAccel) {
            velocitydiff = velocitydiff.normalized * maxAccel;
        }
        return velocitydiff;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.GetComponent<Transform>().position);
    }
}
