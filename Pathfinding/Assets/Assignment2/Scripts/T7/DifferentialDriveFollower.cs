using UnityEngine;
using System.Collections;

public class DifferentialDriveFollower : MonoBehaviour {

    public GameObject target;
    public float maxSpeed;
    public float length;
    public float maxRotSpeed;

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
            maxSpeed,
            maxRotSpeed
        );
        
        if(Mathf.Abs(u.y)>0) {
            rotate(u.y* Time.deltaTime);
        }

        rigidbody.velocity = rigidbody.transform.forward*u.x;
    }
    
    public static Vector2 computeU(Vector3 pos, Vector3 goal, Vector3 forward, float velocity, float length, float maxSpeed, float maxRotSpeed) {
        float angle = getAngle(pos, goal, forward);
        float sign = Mathf.Sign (angle);
        float rotSpeed = 0f;
        float movSpeed = 0f;
        if (Mathf.Cos(angle) < 0.4) {
            // need to turn around
            rotSpeed = sign * maxRotSpeed;
            movSpeed = -maxSpeed;
        } else {
            //Vector3 targetdir = goal - pos;
            //float targetVelocity = maxSpeed;
            float goalrotSpeed = Mathf.Abs(angle);
            rotSpeed = sign * Mathf.Min(goalrotSpeed, maxRotSpeed);
            movSpeed = maxSpeed * (0.1f + 0.9f * Mathf.Cos(angle));
        }
        
        if (Physics.Raycast(pos, forward,1)) {
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

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.GetComponent<Transform>().position);
    }
}
