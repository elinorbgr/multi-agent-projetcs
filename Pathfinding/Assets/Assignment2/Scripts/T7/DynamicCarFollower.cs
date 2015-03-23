using UnityEngine;
using System.Collections;

public class DynamicCarFollower : MonoBehaviour {

    public GameObject target;
    public float maxAngle;
    public float maxForce;
    public float length;
    
    // Use this for initialization
    void Start () {
    }
    
    // Update is called once per frame
    void FixedUpdate () {

        Vector2 u = computeU(
            rigidbody.position,
            target.GetComponent<Transform>().position,
            transform.forward,
            rigidbody.velocity.magnitude,
            length,
            maxForce,
            maxAngle
        );
        rotate(Mathf.Tan(u.y) * rigidbody.velocity.magnitude / length * Time.deltaTime);
        rigidbody.AddForce(u.x * transform.forward);

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
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.GetComponent<Transform>().position);
    }
}
