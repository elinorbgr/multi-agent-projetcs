using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinematicMotionModel : MonoBehaviour, IMotionModel {
    
    private List<Vector3> waypoints;
    private bool moving;
    public float speed;
    public float minx;
    public float miny;
    public float maxx;
    public float maxy;
    private RTTTree<Object> tree;

    // Use this for initialization
    void Start () {
        this.waypoints = new List<Vector3>();
        this.moving = false;
    }

    static private bool visible(Vector3 a, Vector3 b) {
        return !( Physics.Raycast(a, b-a, (b-a).magnitude)
                || Physics.Raycast(b, a-b, (a-b).magnitude));
    }
    
    // Update is called once per frame
    void Update () {
        if (moving) {
            if((this.waypoints[0] - rigidbody.position).magnitude<1
                || Vector3.Dot(this.waypoints[0] - rigidbody.position, rigidbody.velocity) < 0){

                do {
                    this.waypoints.RemoveAt(0);
                } while (this.waypoints.Count > 1 && visible(rigidbody.position, this.waypoints[1]));

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
            this.setVelocity();
        }
    }

    void IMotionModel.MoveOrder(Vector3 goal) {
        this.tree = KinematicRTTPathPlanning.MoveOrder(this.transform.position, goal, minx, miny, maxx, maxy);
        ((IMotionModel)this).SetWaypoints(this.tree.nearestOf(goal).pathFromRoot());
    }
    
}