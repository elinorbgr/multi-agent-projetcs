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

}
