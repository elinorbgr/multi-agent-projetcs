using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneticSTASMobile : MonoBehaviour {

    private List<Vector3> waypoints;
    private bool moving;
    private float delta;

    // Use this for initialization
    void Start () {
        this.waypoints = new List<Vector3>();
        this.moving = false;
        this.delta = -1.0f;
    }
    
    // Update is called once per frame
    void Update () {
        if (moving) {

            if (this.waypoints.Count == 0) {
                moving = false;
                return;
            }

            delta += Time.deltaTime;
            if (delta >= 1.0) {
                delta -= 1.0f;
                transform.position = this.waypoints[0];
                this.waypoints.RemoveAt(0);
            }
        }
    }

    public void setWaypoints(List<Vector3> w) {
        this.waypoints = w;
        moving = true;
    }

    void OnDrawGizmos() {
        if(this.waypoints != null && this.waypoints.Count > 0) {
            Gizmos.color = Color.red;
            Vector3 previous= rigidbody.position;
            Vector3 offset = new Vector3(0f, 1f, 0f);
            foreach (Vector3 v in this.waypoints) {
                Gizmos.DrawLine(previous + offset, v + offset);
                previous = v;
            }
        }
    }
}
