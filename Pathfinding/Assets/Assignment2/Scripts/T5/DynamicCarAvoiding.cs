using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicCarAvoiding : MonoBehaviour {

    
    private List<Vector3> waypoints;
    private bool moving;
    public float maxAngle;
    public float maxForce;
    public float length;
    public float minx;
    public float miny;
    public float maxx;
    public float maxy;

    public GameObject goal;

    private RRTTree<Vector2> tree;
    
    // Use this for initialization
    void Start () {
        this.waypoints = new List<Vector3>();
        this.moving = false;
        this.tree = new RRTTree<Vector2>(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f));
        this.collider.enabled = false;
        this.MoveOrder(goal.GetComponent<Transform>().position);
        this.collider.enabled = true;
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
            if ((this.waypoints [0] - rigidbody.position).magnitude < maxx/50f) {
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
        Vector3 target = goal - pos;

        float distance = velocity*velocity/maxForce/2 +5f;
        Collider[] hitColliders = Physics.OverlapSphere(pos, distance);
        bool collides = false;

        if (hitColliders.Length>1) {
            int i = 0;
            target = target.normalized * distance;
            while (i < hitColliders.Length) {
                if ((hitColliders[i].bounds.center - pos).magnitude == 0f) { i++;continue; }
                Vector3 hitpoint = NearestVertexTo(pos, hitColliders[i].gameObject);
                if ((hitpoint-pos).magnitude > distance) { i++; continue; }
                if (Vector3.Dot((hitpoint - pos), forward*velocity) > 0.25) {
                    target -= hitpoint - pos;
                }
                i++;
            }
        }

        float angle = getAngle(target, forward);
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

        if (collides) {
            // if we keep this trajectory, we'll hit a wall !
            force = -maxForce * Mathf.Sign(force);
        }

        return new Vector2(force,phi);
    }
    
    static float getAngle(Vector3 target, Vector3 velocity){
        float angle = Vector3.Angle(target, velocity) * Mathf.Deg2Rad;
        float sign = Mathf.Sign(Vector3.Cross(target,velocity).y);
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
        /*if (this.tree != null) {
            this.tree.drawGizmos();
        }*/
        if(this.waypoints != null && this.waypoints.Count > 0) {
            Gizmos.color = Color.red;
            Vector3 previous= rigidbody.position;
            foreach (Vector3 v in this.waypoints) {
                Gizmos.DrawLine(previous, v);
                previous = v;
            }
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(
            rigidbody.position,
            (rigidbody.velocity.magnitude*rigidbody.velocity.magnitude/maxForce/2 + 5f)
        );
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            rigidbody.position,
            5f
        );
    }

    void MoveOrder(Vector3 goal) {
        Debug.Log("run");
        this.tree = DynamicCarRRTPathPlanning.MoveOrder(this.transform.position, goal, this.transform.forward, this.rigidbody.velocity.magnitude, this.maxForce, this.maxAngle, this.length, minx, miny, maxx, maxy);
        Debug.Log(this.tree.nodes.Count);
        this.SetWaypoints(this.tree.nearestOf(goal).pathFromRoot());
        Debug.Log(this.tree.nearestOf(goal).fullCost());
    }
    
    void SetWaypoints(List<Vector3> newval) {
        this.waypoints = newval;
        if(this.waypoints.Count > 0) {
            this.moving = true;
        }
    }

    public static Vector3 NearestVertexTo(Vector3 point, GameObject other) {
        // convert point to local space
        point = other.GetComponent<Transform>().InverseTransformPoint(point);
        Mesh mesh = other.GetComponent<MeshFilter>().mesh;
        float minDistanceSqr = Mathf.Infinity;
        Vector3 nearestVertex = Vector3.zero;
         
        // scan all vertices to find nearest
        if (mesh != null) {
            foreach (Vector3 vertex in mesh.vertices) {
                Vector3 diff = point-vertex;
                float distSqr = diff.sqrMagnitude; 
                if (distSqr < minDistanceSqr) {
                    minDistanceSqr = distSqr;
                    nearestVertex = vertex;
                }
            }
        }
         
        // convert nearest vertex back to world space
        return other.GetComponent<Transform>().transform.TransformPoint(nearestVertex);
    } 
}
