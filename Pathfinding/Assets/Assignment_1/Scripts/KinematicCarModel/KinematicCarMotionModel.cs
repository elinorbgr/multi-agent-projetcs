using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinematicCarMotionModel : MonoBehaviour, IMotionModel {
    
    private List<Vector3> waypoints;
    private bool moving;
    public float speed;
    public float maxAngle;
	public float length;
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
    void Update () {
        if (moving) {
            if ((this.waypoints [0] - rigidbody.position).magnitude < maxx/100f) {
                this.waypoints.RemoveAt (0);

                if (this.waypoints.Count == 0) {
					moving = false;
                    return;
                } 
            }
			Vector2 u = computeU(rigidbody.position, this.waypoints[0], transform.forward, rigidbody.velocity.magnitude, length, speed, maxAngle);
			rotate(Mathf.Tan(u.y) * rigidbody.velocity.magnitude / length * Time.deltaTime);
			rigidbody.velocity = u.x*transform.forward;
        }
		else if (rigidbody.velocity.magnitude > 0) {
			rigidbody.velocity = new Vector3 (0F, 0F, 0F);
			return;
		}
    }
    
	public static Vector2 computeU(Vector3 pos, Vector3 goal, Vector3 forward, float velocity, float length, float speed, float maxAngle) {
		float angle = getAngle(pos, goal, forward);
		float sign = Mathf.Sign (angle);
		float phi = 0f;
		float movSpeed = 0f;
		if (Mathf.Cos(angle) < 0) {
			// need to turn around
			phi = sign * maxAngle;
			movSpeed = - speed;
		} else {
			float goalPhi = Mathf.Abs(angle);
			phi = sign * Mathf.Min(goalPhi, maxAngle);
			movSpeed = speed;
		}
		
		if (Physics.Raycast(pos, forward, 1f)) {
			// if we keep this trajectory, we'll hit a wall !
			movSpeed = -speed * Mathf.Sign(movSpeed);
		}
		
		return new Vector2(movSpeed,phi);
	}


	static float getAngle(Vector3 pos, Vector3 goal, Vector3 forward){
		Vector3 targetDir = goal - pos;
		float angle = Vector3.Angle(targetDir, forward) * Mathf.Deg2Rad;
		float sign = Mathf.Sign(Vector3.Cross(targetDir,forward).y);
		return sign*angle;
	}  
    

	void rotate(float angle){
		this.transform.Rotate (new Vector3 (0f,-angle / Mathf.Deg2Rad, 0f));
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
		this.tree = KinematicCarRRT.MoveOrder(this.transform.position, goal, transform.forward, rigidbody.velocity.magnitude, speed, maxAngle, length, minx, miny, maxx, maxy);
		((IMotionModel)this).SetWaypoints(this.tree.cheapestInRadius(goal, 5).pathFromRoot());
        Debug.Log(this.tree.nearestOf(goal).fullCost());
	}
    
}