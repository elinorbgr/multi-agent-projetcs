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

    private List<GameObject> lines;
    
    // Use this for initialization
    void Start () {
        this.waypoints = new List<Vector3>();
        this.moving = false;
        this.lines = new List<GameObject>();
    }

    // Update is called once per frame
    void FixedUpdate () {
        
        if (moving) {
            if((this.waypoints[0] - rigidbody.position).magnitude <= 3f){
                this.waypoints.RemoveAt(0);
                Object.Destroy(this.lines[0]);
                this.lines.RemoveAt(0);
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
                rigidbody.AddForce(-rigidbody.velocity.normalized * 10);
            }
        }
    }

    // computes the acceleration given appropriate input
    // it's a static method so it can be called from RTT
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

    void displayTrajectory() {
        foreach(GameObject o in this.lines) {
            Object.Destroy(o);
        }
        this.lines.Clear();
        Vector3 previous = this.waypoints[0];
        foreach(Vector3 v in this.waypoints) {
            GameObject line = new GameObject();
            this.lines.Add(line);
            LineRenderer line_renderer = line.AddComponent<LineRenderer>();
            line_renderer.useWorldSpace = true;
            line_renderer.material = new Material(Shader.Find("Sprites/Default"));
            line_renderer.SetColors(Color.red, Color.red);
            line_renderer.SetVertexCount(2);
            line_renderer.SetPosition(0, previous);
            line_renderer.SetPosition(1, v);
            line_renderer.SetWidth(0.1F, 0.1F);
            previous = v;
        }
    }

    void IMotionModel.SetWaypoints(List<Vector3> newval) {
        this.waypoints = newval;
        if(this.waypoints.Count > 0) {
            this.moving = true;
            displayTrajectory();
        }
    }

    void IMotionModel.MoveOrder(Vector3 goal) {
        ((IMotionModel)this).SetWaypoints(DynamicRTTPathPlanning.MoveOrder(this.transform.position, goal, this.acceleration, minx, miny, maxx, maxy));
    }
    
}