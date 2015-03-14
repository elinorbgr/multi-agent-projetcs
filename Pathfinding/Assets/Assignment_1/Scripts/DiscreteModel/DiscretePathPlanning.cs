using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiscretePathPlanning {
	public Graph graph;

	static float distance(Graph.Node a, Graph.Node b) {
		return (a.pos - b.pos).magnitude;
	}

	public DiscretePathPlanning(Graph g) {
		this.graph = g;
	}

	public List<Vector3> MoveOrder(Vector3 start, Vector3 goal) {
		// safety
		if (this.graph.nodes.Count == 0) { return new List<Vector3>(); }
		// find best matches for start & end points
		Graph.Node node_start = null;
		Graph.Node node_end = null;
		float best_start = float.PositiveInfinity;
		float best_end = float.PositiveInfinity;
		foreach (Graph.Node n in this.graph.nodes) {
			if ((n.pos - start).magnitude < best_start) {
				best_start = (n.pos - start).magnitude;
				node_start = n;
			}
			if ((n.pos - goal).magnitude < best_end) {
				best_end = (n.pos - goal).magnitude;
				node_end = n;
			}
		}
		return a_star(node_start, node_end);
	}

	private List<Vector3> a_star(Graph.Node start, Graph.Node goal) {
		// init
		HashSet<Graph.Node> closedset = new HashSet<Graph.Node>();
		HashSet<Graph.Node> openset = new HashSet<Graph.Node>();
		openset.Add(start);
		Dictionary<Graph.Node, Graph.Node> came_from = new Dictionary<Graph.Node, Graph.Node>();
		Dictionary<Graph.Node, float> g_score = new Dictionary<Graph.Node, float>();
		g_score[start] = 0;
		Dictionary<Graph.Node, float> f_score = new Dictionary<Graph.Node, float>();
		f_score[start] = distance(start, goal);

		while(openset.Count > 0) {
			// choose current
			Graph.Node current = null;
			float best_f = float.PositiveInfinity;
			foreach (Graph.Node n in openset) {
				if (f_score[n] < best_f) {
					best_f = f_score[n];
					current = n;
				}
			}
			// stop ?
			if (current == goal) {
				List<Vector3> path = new List<Vector3>();
				path.Add(current.pos);
				while (came_from.ContainsKey(current)) {
					current = came_from[current];
					path.Add(current.pos);
				}
				path.Reverse();
				return path;
			}
			// step
			openset.Remove(current);
			closedset.Add(current);
			foreach (Graph.Node n in current.neighbors) {
				if (closedset.Contains(n)) { continue; }
				float tentative_g_score = g_score[current] + distance(current, n);

				if (!openset.Contains(n) || tentative_g_score < g_score[n]) {
					came_from[n] = current;
					g_score[n] = tentative_g_score;
					f_score[n] = tentative_g_score + distance(n, goal);
					openset.Add(n);
				}
			}
		}

		return new List<Vector3>();

	}
}
