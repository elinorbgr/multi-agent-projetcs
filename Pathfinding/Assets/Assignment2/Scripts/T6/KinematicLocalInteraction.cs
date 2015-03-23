using UnityEngine;
using System.Collections;

public class KinematicLocalInteraction : MonoBehaviour {

    public GameObject target;
    public float distance;
    public float searchDistance;
    public float maxSpeed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 target_pos = new Vector3();

        Collider[] neighbors = Physics.OverlapSphere(transform.position, searchDistance);
        foreach(Collider c in neighbors) {
            if (c.GetComponent<KinematicLocalInteraction>() != null) {
                Vector3 towards = c.GetComponent<Transform>().position - transform.position;
                if (towards.magnitude < distance) {
                    towards = (towards - towards.normalized * distance) * 5 * (searchDistance/distance);
                } else {
                    towards = towards - towards.normalized * distance;
                }
                target_pos += towards;
            }
        }
        Vector3 towards_target =  target.GetComponent<Transform>().position - transform.position;
        target_pos += towards_target.normalized * distance;
        Vector3 target_speed = target_pos / Time.deltaTime;
        if (target_speed.magnitude > maxSpeed) {
            target_speed = target_speed.normalized * maxSpeed;
        }
        rigidbody.velocity = target_speed;
	}
}
