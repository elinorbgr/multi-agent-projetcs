using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicMotionModel : MonoBehaviour, IMotionModel {
    
    private List<Vector3> waypoints;
    private bool moving;
    public float acceleration;
    private float speed;
    private float delta;
    private float nodeseparation;
    
    // Use this for initialization
    void Start () {
        this.waypoints = new List<Vector3>();
        this.moving = false;
    }

    // Update is called once per frame
    void FixedUpdate () {
        
        if (moving) {
            if (this.waypoints.Count == 0) {
                moving = false;
                return;
            }

            if(delta==0.00f){
                Vector3 direction2 = this.waypoints[0] - rigidbody.position;
                nodeseparation = direction2.magnitude;
                rigidbody.isKinematic = false;
            }

            delta += Time.deltaTime;
            Vector3 direction = this.waypoints[0] - rigidbody.position;
            float distance = direction.magnitude;
            direction.Normalize();

            if(distance>=(nodeseparation/2)){
                rigidbody.AddForce(direction*acceleration);
            }

            if(distance<(nodeseparation/2)){
                rigidbody.AddForce(-direction*acceleration);
            }

            if(distance <= 0.03f){
                rigidbody.isKinematic = true;
                this.waypoints.RemoveAt(0);
                distance = direction.magnitude;
                this.delta = 0.0f;
            }
        }
    }

    void IMotionModel.SetWaypoints(List<Vector3> newval) {
        this.waypoints = newval;
        this.moving = true;
    }

    void IMotionModel.MoveOrder(Vector3 goal) {
        
    }
    
}