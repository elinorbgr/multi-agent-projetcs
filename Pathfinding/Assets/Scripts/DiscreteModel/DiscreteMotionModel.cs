using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiscreteMotionModel : MonoBehaviour, IMotionModel {

	private List<Vector3> waypoints;
	private bool moving;
	private float delta;

	// Use this for initialization
	void Start () {
		this.waypoints = new List<Vector3>();
		this.moving = false;
		this.delta = 0.0f;
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

	void IMotionModel.SetWaypoints(List<Vector3> newval) {
		this.waypoints = newval;
		this.moving = true;
    }
}
