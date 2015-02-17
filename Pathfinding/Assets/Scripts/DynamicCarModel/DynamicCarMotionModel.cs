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
	
	private RRTTree<Vector2> tree;
	
	// Use this for initialization
	void Start () {
		this.waypoints = new List<Vector3>();
		this.moving = false;
		this.tree = new RRTTree<Vector2>(new Vector3(0f,0f,0f), new Vector2(0f,0f));
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
			if ((this.waypoints [0] - rigidbody.position).magnitude < 2f) {
				this.waypoints.RemoveAt (0);
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
    	this.tree = DynamicCarRRTPathPlanning.MoveOrder(this.transform.position, goal, transform.forward, rigidbody.velocity.magnitude, maxForce, maxAngle, length, minx, miny, maxx, maxy);
        ((IMotionModel)this).SetWaypoints(this.tree.nearestOf(goal).pathFromRoot());
    }
	
}