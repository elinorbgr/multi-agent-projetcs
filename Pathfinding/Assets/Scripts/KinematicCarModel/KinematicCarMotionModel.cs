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
    void Update () {
        if (moving) {
            if ((this.waypoints [0] - rigidbody.position).magnitude < 2f) {
                this.waypoints.RemoveAt (0);
                Object.Destroy (this.lines [0]);
                this.lines.RemoveAt (0);

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
			Vector3 targetdir = goal - pos;
			float targetVelocity = speed;
			float goalPhi = Mathf.Abs(angle);
			phi = sign * Mathf.Min(goalPhi, maxAngle);
			movSpeed = 10 * (targetVelocity-velocity) * Mathf.Cos(angle); // what is this?
			if (Mathf.Abs(movSpeed) > speed) {
				movSpeed = Mathf.Sign(movSpeed) * speed;
			}
		}
		
		if (Physics.Raycast(pos, forward, velocity + 1f)) {
			// if we keep this trajectory, we'll hit a wall !
			movSpeed = -speed * Mathf.Sign(movSpeed);
		}
		
		return new Vector2(movSpeed,phi);
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
		this.tree = KinematicCarRRT.MoveOrder(this.transform.position, goal, transform.forward, rigidbody.velocity.magnitude, speed, maxAngle, length, minx, miny, maxx, maxy);
		((IMotionModel)this).SetWaypoints(this.tree.nearestOf(goal).pathFromRoot());
	}
    
}