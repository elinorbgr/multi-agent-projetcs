using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using R = ReservationMap;

public class SpaceTimeAStar : MonoBehaviour, ISynchroStart {

    public GameObject reserver;
    public GameObject goal;

    private R reservations;

    private List<Vector3> waypoints;
    private bool moving;
    private float delta;

	// Use this for initialization
	void Start () {
        this.waypoints = new List<Vector3>();
        this.moving = false;
        this.delta = -2.0f;
        ResMapBuilder builder = (ResMapBuilder) reserver.GetComponent(typeof(ResMapBuilder));
        this.reservations = builder.getMap();
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

    static float distance(Graph.Node a, Graph.Node b) {
        return (a.pos - b.pos).magnitude;
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

    void ISynchroStart.act() {
        this.reservations.unreserveLocation(
            this.reservations.graph.NearestNodeOf(transform.position),
            1
        );
        Debug.Log(name);
        this.waypoints = spaceTimeAStar(
            this.reservations.graph.NearestNodeOf(transform.position),
            this.reservations.graph.NearestNodeOf(goal.transform.position)
        );
        this.moving = true;
    }

    void ISynchroStart.prepare() {
        this.reservations.reserveLocation(
            this.reservations.graph.NearestNodeOf(transform.position),
            0
        );
        this.reservations.reserveLocation(
            this.reservations.graph.NearestNodeOf(transform.position),
            1
        );
    }

    private List<Vector3> spaceTimeAStar(Graph.Node start, Graph.Node goal) {
        // init
        HashSet<R.Entry> closedset = new HashSet<R.Entry>();
        HashSet<R.Entry> openset = new HashSet<R.Entry>();
        R.Entry beginning = new R.Entry(start, 0);
        openset.Add(beginning);
        Dictionary<R.Entry, R.Entry> came_from = new Dictionary<R.Entry, R.Entry>();
        Dictionary<R.Entry, float> g_score = new Dictionary<R.Entry, float>();
        g_score[beginning] = 0;
        Dictionary<R.Entry, float> f_score = new Dictionary<R.Entry, float>();
        f_score[beginning] = distance(start, goal);

        while(openset.Count > 0) {
            // choose current
            R.Entry current = null;
            float best_f = float.PositiveInfinity;
            foreach (R.Entry e in openset) {
                if (f_score[e] < best_f) {
                    best_f = f_score[e];
                    current = e;
                }
            }
            // stop ?
            if (current.node == goal && this.reservations.isFutureProof(current)) {
                // Okay, no one will pass, we can stay here
                List<Vector3> path = new List<Vector3>();
                path.Add(current.node.pos);
                this.reservations.reserveForever(current.node, current.time);
                R.Entry old = current;
                while (came_from.ContainsKey(current)) {
                    current = came_from[current];
                    path.Add(current.node.pos);
                    this.reservations.reserveMove(current.node, old.node, old.time);
                    old = current;
                }
                path.Reverse();
                return path;
            }
            // step
            openset.Remove(current);
            closedset.Add(current);
            foreach (Graph.Node n in current.node.neighbors) {
                R.Entry e = new R.Entry(n, current.time+1);
                if (closedset.Contains(e)) { continue; }
                if (!this.reservations.isFree(e)) { continue; }
                float tentative_g_score = g_score[current] + 1;

                if (!openset.Contains(e) || tentative_g_score < g_score[e]) {
                    came_from[e] = current;
                    g_score[e] = tentative_g_score;
                    f_score[e] = tentative_g_score + distance(e.node, goal);
                    openset.Add(e);
                }
            }
            // also try not moving
            {
                R.Entry e = new R.Entry(current.node, current.time+1);
                if ((!closedset.Contains(e)) && this.reservations.isFree(e))
                {
                    float tentative_g_score = g_score[current] + 1;

                    if (!openset.Contains(e) || tentative_g_score < g_score[e]) {
                        came_from[e] = current;
                        g_score[e] = tentative_g_score;
                        f_score[e] = tentative_g_score + distance(e.node, goal);
                        openset.Add(e);
                    }
                }
            }
        }

        return new List<Vector3>();
    }
}
