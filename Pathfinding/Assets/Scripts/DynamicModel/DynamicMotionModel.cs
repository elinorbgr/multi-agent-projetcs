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
            Vector3 targetdir = this.waypoints[0] - rigidbody.position;

            if(targetdir.magnitude <= 3f){
                this.waypoints.RemoveAt(0);
                Object.Destroy(this.lines[0]);
                this.lines.RemoveAt(0);
                if (this.waypoints.Count == 0) {
                    moving = false;
                    return;
                }
                targetdir = this.waypoints[0] - rigidbody.position;
            }

            Vector3 targetVelocity = Mathf.Sqrt(2*acceleration*targetdir.magnitude) * targetdir.normalized;
            Vector3 velocitydiff = targetVelocity - rigidbody.velocity;

            if (velocitydiff.magnitude > acceleration) {
                velocitydiff = velocitydiff.normalized * acceleration;
            }

            rigidbody.AddForce(velocitydiff);
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
        ((IMotionModel)this).SetWaypoints(DynamicRTTPathPlanning.MoveOrder(this.transform.position, goal, minx, miny, maxx, maxy));
    }
    
}