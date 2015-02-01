using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiscreteMotionModel : MonoBehaviour, IMotionModel {

	public GameObject graphBuilder;

	private List<Vector3> waypoints;
	private bool moving;
	private float delta;
	private DiscretePathPlanning pathPlanner;

	// Use this for initialization
	void Start () {
		this.waypoints = new List<Vector3>();
		this.moving = false;
		this.delta = 0.0f;
		if (graphBuilder != null) {
			IGraphBuilder builder = (IGraphBuilder) graphBuilder.GetComponent(typeof(IGraphBuilder));
			this.pathPlanner = new DiscretePathPlanning(builder.getGraph());
		} else {
			this.pathPlanner = new DiscretePathPlanning(new Graph());
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (moving) {

			if (this.waypoints.Count == 0) {
				moving = false;
				return;
			}

			delta += Time.deltaTime;
			if (delta >= 0.5) {
				delta -= 0.5f;
				transform.position = this.waypoints[0];
				this.waypoints.RemoveAt(0);
			}
		}
	
	}

	void IMotionModel.SetWaypoints(List<Vector3> newval) {
		this.waypoints = newval;
		this.moving = true;
    }

    void IMotionModel.MoveOrder(Vector3 goal) {
		((IMotionModel)this).SetWaypoints(this.pathPlanner.MoveOrder(this.transform.position, goal));
    }
}
