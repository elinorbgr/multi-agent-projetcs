using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinematicCarMotionModel : MonoBehaviour, IMotionModel {
    
    private List<Vector3> waypoints;
    private bool moving;
    public float speed;
    public float maxAngle;
	private bool rotating;
	public float rotationSpeed;
    public float minx;
    public float miny;
    public float maxx;
    public float maxy;

    private List<GameObject> lines;
    
    // Use this for initialization
    void Start () {
        this.waypoints = new List<Vector3>();
        this.moving = false;
		this.rotating = false;
        this.lines = new List<GameObject>();
    }
    
    // Update is called once per frame
    void Update () {
        if (moving || rotating) {
            if ((this.waypoints [0] - rigidbody.position).magnitude < 0.9f) {
                this.waypoints.RemoveAt (0);
                Object.Destroy (this.lines [0]);
                this.lines.RemoveAt (0);
				this.moving = false;
				if (this.waypoints.Count > 0) {
					this.rotating = true;
				}
                if (this.waypoints.Count == 0) {
					rigidbody.velocity = new Vector3 (0F, 0F, 0F);
					moving = false;
                    return;
                } 
            }
        }
		if (rotating) {
			float angle = getAngle();
			float length = transform.lossyScale.z;
			float turn = rigidbody.velocity.magnitude / length;
			if (Mathf.Abs(angle) > rotationSpeed * Time.deltaTime) {
				rotate(Mathf.Sign(angle) * rotationSpeed * Time.deltaTime*turn);
				setVelocity(speed);
				
			} else {
				rotate(getAngle());
				setVelocity(speed);
				this.rotating = false;
				this.moving = true;
			}
		}
    }
    
    float getAngle(){
        Vector3 targetDir = this.waypoints[0] - transform.position;
        Vector3 forward = transform.forward;
        float angle = Vector3.Angle(targetDir, forward);
        float sign = Mathf.Sign(Vector3.Cross(targetDir,forward).y);
        return sign*angle;
        
    }
    void rotate(float angle){
        this.transform.Rotate (new Vector3 (0f,-angle, 0f));
    }

    void setVelocity(float movespeed) {
        rigidbody.velocity = transform.forward*movespeed;
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
        ((IMotionModel)this).SetWaypoints(KinematicRTTPathPlanning.MoveOrder(this.transform.position, goal, minx, miny, maxx, maxy));
    }
    
}