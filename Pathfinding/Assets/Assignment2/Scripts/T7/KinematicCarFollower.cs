using UnityEngine;
using System.Collections;

public class KinematicCarFollower : MonoBehaviour {
    
    public GameObject target;
    public float speed;
    public float maxAngle;
    public float length;
    
    // Use this for initialization
    void Start () {
    }
    
    // Update is called once per frame
    void Update () {

        Vector2 u = computeU(
            rigidbody.position,
            target.GetComponent<Transform>().position,
            transform.forward,
            rigidbody.velocity.magnitude,
            length,
            speed,
            maxAngle
        );
        rotate(Mathf.Tan(u.y) * rigidbody.velocity.magnitude / length * Time.deltaTime);
        rigidbody.velocity = u.x*transform.forward;

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
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.GetComponent<Transform>().position);
    }
}
