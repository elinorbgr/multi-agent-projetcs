using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinematicMotionModel : MonoBehaviour, IMotionModel {
    
    private List<Vector3> waypoints;
    private bool moving;
    public float speed;
    
    // Use this for initialization
    void Start () {
        this.waypoints = new List<Vector3>();
        this.moving = false;
    }
    
    // Update is called once per frame
    void Update () {
        if (moving) {
            if (this.waypoints.Count == 0) {
                moving = false;
                return;
            }

            Vector3 direction = this.waypoints[0] - rigidbody.position;
            float distance = direction.magnitude;
            direction.Normalize();
            rigidbody.MovePosition(rigidbody.position + direction*speed*Time.deltaTime);

            if(distance<0.05){
                this.waypoints.RemoveAt(0);
                distance = direction.magnitude;
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