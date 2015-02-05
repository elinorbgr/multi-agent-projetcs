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
                Object.Destroy(this.lines[0]);
                this.lines.RemoveAt(0);
                distance = direction.magnitude;
                this.delta = 0.0f;
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
            line_renderer.SetWidth(0.02F, 0.02F);
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
        ((IMotionModel)this).SetWaypoints(KinematicRTTPathPlanning.MoveOrder(this.transform.position, goal));
    }
    
}