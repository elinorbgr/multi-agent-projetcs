using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinematicMouseFollower : MonoBehaviour, IMotionModel {

    private Vector3 target;
    public float maxSpeed;

	// Use this for initialization
	void Start () {
        this.target = rigidbody.position;

	}
	
	// Update is called once per frame
	void Update () {
        Vector3 speed = (target - rigidbody.position) / Time.deltaTime;
        if (speed.magnitude > maxSpeed) {
            speed = speed.normalized * maxSpeed;
        }
        rigidbody.velocity = speed;
        transform.forward = (target - rigidbody.position).normalized;
	}

    void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(rigidbody.position, target);
    }

    void IMotionModel.SetWaypoints(List<Vector3> w) {}

    void IMotionModel.MoveOrder(Vector3 v) {
        this.target = v;
    }
}
