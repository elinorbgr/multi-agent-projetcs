using UnityEngine;
using System.Collections;

public class KinematicVirtualStructureFollower : MonoBehaviour {

    public GameObject center;
    public float maxSpeed;
    public float x;
    public float z;

	// Use this for initialization
    void Start () {
    
    }
    
    // Update is called once per frame
	void Update () {
        Vector3 o = center.GetComponent<Transform>().position;
        Vector3 vx = center.GetComponent<Transform>().right;
        Vector3 vz = center.GetComponent<Transform>().forward;
        Vector3 target_pos = o + x*vx + z* vz;
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
        Vector3 o = center.GetComponent<Transform>().position;
        Vector3 vx = center.GetComponent<Transform>().right;
        Vector3 vz = center.GetComponent<Transform>().forward;
        Vector3 target_pos = o + x*vx + z* vz;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(rigidbody.position, target_pos);
    }
}
