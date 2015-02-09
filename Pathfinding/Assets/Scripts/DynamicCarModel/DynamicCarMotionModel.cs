using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicCarMotionModel : MonoBehaviour, IMotionModel {
	
	private List<Vector3> waypoints;
	private bool moving;
	public float speed;
	public float maxAngle;
	public float Force;
	private float distance;
	private float delta = 0f;
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
		this.lines = new List<GameObject>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (delta == 0f && this.waypoints.Count != 0) {
			distance = (this.waypoints[0] - transform.position).magnitude;		
		}
		delta += Time.deltaTime;
		if (this.waypoints.Count == 0) {
			moving = false;
			rotating = false;
			if(rigidbody.velocity.magnitude <0.2f){
				rigidbody.velocity = new Vector3(0f,0f,0f);
				rigidbody.angularVelocity = new Vector3(0f,0f,0f);
				return;
			}
			rigidbody.AddForce(-transform.forward*(Force/rigidbody.mass));
		}
		if (moving || rotating) {

	
			if ((this.waypoints [0] - rigidbody.position).magnitude < 0.7) {
				this.delta = 0f;
				this.waypoints.RemoveAt (0);
				Object.Destroy (this.lines [0]);
				this.lines.RemoveAt (0);
 
				if (this.waypoints.Count > 0) {
					this.rotating = true;
				}

			}

			}
		if (rotating) {
			float angle = getAngle();
			float length = transform.lossyScale.z;
			float turnPow = rigidbody.velocity.magnitude / length;
			float sign = Mathf.Sign (angle);		
			if (Mathf.Abs(angle) > rotationSpeed * Time.deltaTime) {
				rotate(sign * rotationSpeed * Time.deltaTime*turnPow);
				setVelocity(Force);
				
			} else {
				rotate(getAngle());
				setVelocity(Force);
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
	
	void setVelocity(float addForce) {
		float mass = rigidbody.mass;
		Vector3 targetDir = this.waypoints[0] - transform.position;
		rigidbody.velocity = transform.forward * rigidbody.velocity.magnitude;	

		if(targetDir.magnitude > (distance/2)){
			rigidbody.AddForce (transform.forward*(addForce/mass));
		}
		if(targetDir.magnitude <= (distance/2)){
			rigidbody.AddForce (-transform.forward*(addForce/mass));
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
        ((IMotionModel)this).SetWaypoints(KinematicRTTPathPlanning.MoveOrder(this.transform.position, goal, minx, miny, maxx, maxy));
    }
	
}