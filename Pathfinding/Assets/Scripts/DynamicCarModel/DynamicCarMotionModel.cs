using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicCarMotionModel : MonoBehaviour, IMotionModel {
	
	private List<Vector3> waypoints;
	private bool moving;
	public float maxAngle;
	public float maxForce;
	public float length;
	private float distance;
    public float minx;
    public float miny;
    public float maxx;
    public float maxy;
	
	private RTTTree<Vector2> tree;
	private List<GameObject> lines;
	
	// Use this for initialization
	void Start () {
		this.waypoints = new List<Vector3>();
		this.moving = false;
		this.lines = new List<GameObject>();
		this.tree = new RTTTree<Vector2>(new Vector3(0f,0f,0f), new Vector2(0f,0f));
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (this.waypoints.Count == 0) {
			moving = false;
			if(rigidbody.velocity.magnitude <0.2f){
				rigidbody.velocity = new Vector3(0f,0f,0f);
				return;
			}
			rigidbody.AddForce(-transform.forward*(maxForce/rigidbody.mass));
			return;
		}
		if (moving) {
			if ((this.waypoints [0] - rigidbody.position).magnitude < 2) {
				this.waypoints.RemoveAt (0);
				Object.Destroy (this.lines [0]);
				this.lines.RemoveAt (0);
				if (this.waypoints.Count == 0) {
					moving = false;
					return;
				}
			}

			Vector2 u = computeU(rigidbody.position, this.waypoints[0], transform.forward, rigidbody.velocity.magnitude, length, maxForce, maxAngle);
			rotate(Mathf.Tan(u.y) * rigidbody.velocity.magnitude / length * Time.deltaTime);
			rigidbody.AddForce(u.x * transform.forward);

		} else if (rigidbody.velocity.magnitude > 0) {
            if (rigidbody.velocity.magnitude < 0.1f) {
                rigidbody.velocity = new Vector3(0f, 0f, 0f);
            } else if (rigidbody.velocity.magnitude * 10 > maxForce) {
                rigidbody.AddForce(-rigidbody.velocity.normalized * maxForce);
            } else {
                rigidbody.AddForce(-rigidbody.velocity * 10);
            }
        }

	}

	public static Vector2 computeU(Vector3 pos, Vector3 goal, Vector3 forward, float velocity, float length, float maxForce, float maxAngle) {
		float angle = getAngle(pos, goal, forward);
		float sign = Mathf.Sign (angle);
		float phi = 0f;
		float force = 0f;
		if (Mathf.Cos(angle) < 0) {
			// need to turn around
			phi = sign * maxAngle;
			force = - maxForce;
		} else {
			Vector3 targetdir = goal - pos;
			float targetVelocity = Mathf.Sqrt(2*maxForce*targetdir.magnitude);
			float goalPhi = Mathf.Abs(angle);
			phi = sign * Mathf.Min(goalPhi, maxAngle);
			force = 10 * (targetVelocity-velocity) * Mathf.Cos(angle);
        	if (Mathf.Abs(force) > maxForce) {
        		force = Mathf.Sign(force) * maxForce;
        	}
		}

		if (Physics.Raycast(pos, forward, velocity*velocity/maxForce/2 + 1f)) {
            // if we keep this trajectory, we'll hit a wall !
            force = -maxForce * Mathf.Sign(force);
        }

		return new Vector2(force,phi);
	}
	
	static float getAngle(Vector3 pos, Vector3 goal, Vector3 velocity){
		Vector3 targetDir = goal - pos;
		float angle = Vector3.Angle(targetDir, velocity) * Mathf.Deg2Rad;
		float sign = Mathf.Sign(Vector3.Cross(targetDir,velocity).y);
		return sign*angle;
	}

	void rotate(float angle){
		this.transform.Rotate (new Vector3 (0f,-angle / Mathf.Deg2Rad, 0f));
		// keep the velocity aligned
		rigidbody.velocity = transform.forward
								* rigidbody.velocity.magnitude
								* Mathf.Sign(Vector3.Dot(transform.forward, rigidbody.velocity));
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

	void OnDrawGizmos() {
		if (this.tree != null) {
			this.tree.drawGizmos();
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
    	this.tree = DynamicCarRTTPathPlanning.MoveOrder(this.transform.position, goal, transform.forward, rigidbody.velocity.magnitude, maxForce, maxAngle, length, minx, miny, maxx, maxy);
        ((IMotionModel)this).SetWaypoints(this.tree.nearestOf(goal).pathFromRoot());
    }
	
}