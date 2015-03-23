using UnityEngine;
using System.Collections;

public class KinematicFollower : MonoBehaviour {

    public GameObject leader;
    public float distance;
    public float angle;
    public float maxSpeed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 leader_pos = leader.GetComponent<Rigidbody>().position;
        Vector3 target_pos = leader_pos - Quaternion.AngleAxis(angle, Vector3.up)*leader.GetComponent<Transform>().forward*distance;
        Vector3 speed = (target_pos - rigidbody.position) / Time.deltaTime;
        if (speed.magnitude > maxSpeed) {
            speed = speed.normalized * maxSpeed;
        }
        rigidbody.velocity = speed;
        if (speed.magnitude > 0) {
            transform.forward = speed.normalized;
        }
	}

    void OnDrawGizmos() {
        Vector3 leader_pos = leader.GetComponent<Rigidbody>().position;
        Vector3 target_pos = leader_pos - Quaternion.AngleAxis(angle, Vector3.up)*leader.GetComponent<Transform>().forward*distance;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(leader_pos, target_pos);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(rigidbody.position, target_pos);
    }
}
