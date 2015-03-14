using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graph {

	public class Node {
		public Vector3 pos;
		public List<Node> neighbors;

		public Node(Vector3 pos) {
			this.pos = pos;
			this.neighbors = new List<Node>();
		}

		public void connect(Node other) {
			this.neighbors.Add(other);
			other.neighbors.Add(this);
		}
	}

	public List<Node> nodes;

	public Graph() {
		nodes = new List<Node>();
	}

	public void CreateDisplayers() {
		foreach (Node n in this.nodes) {
			GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			sphere.transform.position = n.pos;
			sphere.transform.localScale = new Vector3(0.25F, 0.25F, 0.25F);
			sphere.renderer.material.color = new Color(0.0F, 0.5F, 1.0F);
			foreach (Node o in n.neighbors) {
				GameObject line = new GameObject();
				LineRenderer line_renderer = line.AddComponent<LineRenderer>();
				line_renderer.useWorldSpace = true;
				line_renderer.material = new Material(Shader.Find("Sprites/Default"));
				line_renderer.SetColors(Color.black, Color.black);
				line_renderer.SetVertexCount(2);
				line_renderer.SetPosition(0, n.pos);
				line_renderer.SetPosition(1, o.pos);
				line_renderer.SetWidth(0.05F, 0.05F);
			}
		}
	}


}
