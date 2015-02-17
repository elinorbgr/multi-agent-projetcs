using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicMotionModel : MonoBehaviour, IMotionModel {
    
    private List<Vector3> waypoints;
    private bool moving;
    public float acceleration;
    public float minx;
    public float miny;
    public float maxx;
    public float maxy;

    private RRTTree<Vector3> tree;
    
    // Use this for initialization
    void Start () {
        this.waypoints = new List<Vector3>();
        this.moving = false;
        this.tree = new RRTTree<Vector3>(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f));
    }

    // Update is called once per frame
    void FixedUpdate () {
        
        if (moving) {
            if((this.waypoints[0] - rigidbody.position).magnitude <= 1f){
                this.waypoints.RemoveAt(0);
                if (this.waypoints.Count == 0) {
                    moving = false;
                    return;
                }
            }

            rigidbody.AddForce(computeAcceleration(rigidbody.position, rigidbody.velocity, this.waypoints[0], this.acceleration));

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
        if (this.tree != null) {
            this.tree.drawGizmos();
        }
        if(this.waypoints != null && this.waypoints.Count > 0) {
            Gizmos.color = Color.red;
            Vector3 previous= rigidbody.position;
            foreach (Vector3 v in this.waypoints) {
                Gizmos.DrawLine(previous, v);
                previous = v;
            }
        }
    }

    void IMotionModel.SetWaypoints(List<Vector3> newval) {
        this.waypoints = newval;
        if(this.waypoints.Count > 0) {
            this.moving = true;
        }
    }

    void IMotionModel.MoveOrder(Vector3 goal) {
        this.tree = DynamicRRTPathPlanning.MoveOrder(this.transform.position, goal, this.acceleration, minx, miny, maxx, maxy);
        ((IMotionModel)this).SetWaypoints(this.tree.nearestOf(goal).pathFromRoot());
    }
    
}