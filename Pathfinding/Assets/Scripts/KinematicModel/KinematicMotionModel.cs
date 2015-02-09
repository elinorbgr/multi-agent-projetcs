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

    private List<GameObject> lines;

    // Use this for initialization
    void Start () {
        this.waypoints = new List<Vector3>();
        this.moving = false;
        this.lines = new List<GameObject>();
    }
    
    // Update is called once per frame
    void Update () {
        if (moving) {
            if((this.waypoints[0] - rigidbody.position).magnitude<0.25){
                this.waypoints.RemoveAt(0);
                Object.Destroy(this.lines[0]);
                this.lines.RemoveAt(0);
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
            this.setVelocity();
            displayTrajectory();
        }
    }

    void IMotionModel.MoveOrder(Vector3 goal) {
        ((IMotionModel)this).SetWaypoints(KinematicRTTPathPlanning.MoveOrder(this.transform.position, goal, minx, miny, maxx, maxy));
    }
    
}