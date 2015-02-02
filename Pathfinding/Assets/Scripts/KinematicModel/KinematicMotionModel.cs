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
            if((this.waypoints[0] - rigidbody.position).magnitude<0.25){
                this.waypoints.RemoveAt(0);
                if (this.waypoints.Count == 0) {
                    moving = false;
                    rigidbody.velocity = new Vector3(0F,0F,0F);
                    return;
                } else {
                    this.setVelocity();
                }
            }
        }
    }

    void setVelocity() {
        Vector3 direction = this.waypoints[0] - rigidbody.position;
        direction.Normalize();
        rigidbody.velocity = direction*speed;
    }

    void IMotionModel.SetWaypoints(List<Vector3> newval) {
        this.waypoints = newval;
        this.moving = true;
        this.setVelocity();
    }

    void IMotionModel.MoveOrder(Vector3 goal) {
        ((IMotionModel)this).SetWaypoints(KinematicRTTPathPlanning.MoveOrder(this.transform.position, goal));
    }
    
}