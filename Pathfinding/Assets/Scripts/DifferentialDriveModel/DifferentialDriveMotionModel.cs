using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DifferentialDriveMotionModel : MonoBehaviour, IMotionModel {

    private List<Vector3> waypoints;
    private bool moving;
    public float maxSpeed;
	public float length;
    public float maxRotSpeed;
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
			Vector2 u = computeU(rigidbody.position, this.waypoints[0], transform.forward, rigidbody.velocity.magnitude, length,maxSpeed, maxRotSpeed);
            if ((this.waypoints [0] - rigidbody.position).magnitude < 3f) {
                this.waypoints.RemoveAt (0);
				if(this.waypoints.Count == 0){
					moving = false;
					return;
				}
            }
			else{
				rotate(u.y* Time.deltaTime);
				rigidbody.velocity = rigidbody.transform.forward*u.x;
			}
        }
        else {
			rigidbody.velocity = new Vector3(0f,0f,0f);
        }
    }
    
	public static Vector2 computeU(Vector3 pos, Vector3 goal, Vector3 forward, float velocity, float length, float maxSpeed, float maxRotSpeed) {
		float angle = getAngle(pos, goal, forward);
		float sign = Mathf.Sign (angle);
		float rotSpeed = 0f;
		float movSpeed = 0f;

		float goalrotSpeed = maxRotSpeed;
		rotSpeed = sign * Mathf.Min(goalrotSpeed, maxRotSpeed);
		movSpeed = maxSpeed;
		if (Mathf.Abs(movSpeed) > maxSpeed) {
			movSpeed = Mathf.Sign(movSpeed) * maxSpeed;
		}
		
		if (Physics.Raycast(pos, forward, velocity*velocity/maxSpeed/2 + 1f)) {
			// if we keep this trajectory, we'll hit a wall !
			movSpeed = -maxSpeed * Mathf.Sign(movSpeed);
		}
		
		return new Vector2(movSpeed,rotSpeed);
	}
	
	static float getAngle(Vector3 pos, Vector3 goal, Vector3 velocity){
		Vector3 targetDir = goal - pos;
		float angle = Vector3.Angle(targetDir, velocity) * Mathf.Deg2Rad;
		float sign = Mathf.Sign(Vector3.Cross(targetDir,velocity).y);
		return sign*angle;
	}

    void rotate(float angle){
		this.transform.Rotate (new Vector3 (0f,-angle / Mathf.Deg2Rad, 0f));
    }
    
    void IMotionModel.SetWaypoints(List<Vector3> newval) {
        this.waypoints = newval;
        if(this.waypoints.Count > 0) {
			this.moving = true;
        }
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
    
    void IMotionModel.MoveOrder(Vector3 goal) {
		this.tree = DifferentialDriveRRT.MoveOrder(this.transform.position, goal, transform.forward, rigidbody.velocity.magnitude, maxSpeed, maxRotSpeed, length, minx, miny, maxx, maxy);
		((IMotionModel)this).SetWaypoints(this.tree.nearestOf(goal).pathFromRoot()); 
	}
    
}