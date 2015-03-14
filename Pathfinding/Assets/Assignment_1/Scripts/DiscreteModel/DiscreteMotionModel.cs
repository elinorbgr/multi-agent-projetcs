using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiscreteMotionModel : MonoBehaviour, IMotionModel {

	public GameObject graphBuilder;

	private List<Vector3> waypoints;
	private bool moving;
	private float delta;
	private DiscretePathPlanning pathPlanner;

	private List<GameObject> lines;

	// Use this for initialization
	void Start () {
		this.waypoints = new List<Vector3>();
		this.lines = new List<GameObject>();
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
                Object.Destroy(this.lines[0]);
                this.lines.RemoveAt(0);
			}
		}
	
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
            line_renderer.SetWidth(0.5F, 0.5F);
            previous = v;
        }
    }

	void IMotionModel.SetWaypoints(List<Vector3> newval) {
		this.waypoints = newval;
		displayTrajectory();
		this.moving = true;
    }

    void IMotionModel.MoveOrder(Vector3 goal) {
		((IMotionModel)this).SetWaypoints(this.pathPlanner.MoveOrder(this.transform.position, goal));
    }
}
