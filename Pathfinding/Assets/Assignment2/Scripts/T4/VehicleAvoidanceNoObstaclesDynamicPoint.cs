using UnityEngine;
using System.Collections;

public class VehicleAvoidanceNoObstaclesDynamicPoint : MonoBehaviour {

    public GameObject goal;
    private bool moving;
    private Vector3 target;
    public float acceleration;
    
    // Use this for initialization
    void Start () {
        this.moving = true;
        this.target = ((Transform) this.goal.GetComponent(typeof(Transform))).position;
    }

    // Update is called once per frame
    void FixedUpdate () {
        
        if (moving) {
            if((this.target - rigidbody.position).magnitude <= 0.1f){
                moving = false;
                return;
            }

            rigidbody.AddForce(computeAcceleration(rigidbody.position, rigidbody.velocity, this.target, this.acceleration));

        } else if (rigidbody.velocity.magnitude > 0) {
            if (rigidbody.velocity.magnitude < 0.1f) {
                rigidbody.velocity = new Vector3(0f, 0f, 0f);
            } else if (rigidbody.velocity.magnitude * 10 > acceleration) {
                rigidbody.AddForce(-rigidbody.velocity.normalized * acceleration);
            } else {
                rigidbody.AddForce(-rigidbody.velocity * 10);
            }
        }
    }

    // computes the acceleration given appropriate input
    // it's a static method so it can be called from RRT
    public static Vector3 computeAcceleration(Vector3 pos, Vector3 velocity, Vector3 goal, float maxAccel) {
        Vector3 targetdir = goal-pos;
        Vector3 targetVelocity = Mathf.Sqrt(2*maxAccel*targetdir.magnitude) * targetdir.normalized;
        Vector3 velocitydiff = (targetVelocity - velocity);

        if (velocitydiff.magnitude > maxAccel) {
            velocitydiff = velocitydiff.normalized * maxAccel;
        }

        float distance = velocity.magnitude*velocity.magnitude/maxAccel/2 +0.5f;
        Collider[] hitColliders = Physics.OverlapSphere(pos, 4*distance);
        if (hitColliders.Length>1) {
    		int i = 0;
    		Vector3 bias = new Vector3();
	        while (i < hitColliders.Length) {
	        	if ((hitColliders[i].bounds.center - pos).magnitude == 0f) { i++;continue; }
	        	if (Vector3.Dot((hitColliders[i].bounds.center - pos), velocity) > -0.25) {
		            bias -= (hitColliders[i].bounds.center - pos).normalized;
		        }
	            if ((hitColliders[i].bounds.center - pos).magnitude < distance) {
	            	velocitydiff = - velocity.normalized*maxAccel*2;
	            }
	            i++;
	        }
    		// add a bias to break symmetry
    		if (bias.magnitude > 0) {
	    		velocitydiff += Quaternion.Euler(0, -20, 0) * bias.normalized*maxAccel*10;
	    	}
        }

        if (velocitydiff.magnitude > maxAccel) {
            velocitydiff = velocitydiff.normalized * maxAccel;
        }
        return velocitydiff;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rigidbody.position, target);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(
        	rigidbody.position,
        	4*(rigidbody.velocity.magnitude*rigidbody.velocity.magnitude/acceleration/2 + 0.5f)
        );
    }
}
