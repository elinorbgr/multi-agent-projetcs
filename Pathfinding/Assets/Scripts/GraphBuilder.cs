using UnityEngine;
using System.Collections;

public class GraphBuilder : MonoBehaviour {

	private Graph g;

	// Use this for initialization
	void Start () {
		g = new Graph();
		// 9 nodes
		g.nodes.Add(new Graph.Node(new Vector3(-2F, 0.5F,-1F)));
		g.nodes.Add(new Graph.Node(new Vector3(-2F, 0.5F, 0F)));
		g.nodes.Add(new Graph.Node(new Vector3(-1F, 0.5F, 0F)));
		g.nodes.Add(new Graph.Node(new Vector3(-1F, 0.5F, -1F)));
		g.nodes.Add(new Graph.Node(new Vector3(1F, 0.5F, -1F)));
		g.nodes.Add(new Graph.Node(new Vector3(1F, 0.5F, 0F)));
		g.nodes.Add(new Graph.Node(new Vector3(0F, 0.5F, 0F)));
		g.nodes.Add(new Graph.Node(new Vector3(0F, 0.5F, 1F)));
		g.nodes.Add(new Graph.Node(new Vector3(1F, 0.5F, 1F)));
		g.nodes.Add(new Graph.Node(new Vector3(-3F, 0.5F, 1F)));
		g.nodes.Add(new Graph.Node(new Vector3(-1F, 0.5F, -2F)));
		g.nodes.Add(new Graph.Node(new Vector3(0F, 0.5F, -2F)));
		g.nodes.Add(new Graph.Node(new Vector3(0F, 0.5F, -3F)));
		g.nodes.Add(new Graph.Node(new Vector3(-2F, 0.5F, -3F)));
		g.nodes.Add(new Graph.Node(new Vector3(-3F, 0.5F, -1F)));
		g.nodes.Add(new Graph.Node(new Vector3(-4F, 0.5F, 1F)));

		// link them
		g.nodes[0].connect(g.nodes[1]);
		g.nodes[0].connect(g.nodes[2]);
		g.nodes[0].connect(g.nodes[3]);
		g.nodes[0].connect(g.nodes[13]);
		g.nodes[1].connect(g.nodes[2]);
		g.nodes[1].connect(g.nodes[7]);
		g.nodes[1].connect(g.nodes[9]);
		g.nodes[1].connect(g.nodes[14]);
		g.nodes[2].connect(g.nodes[3]);
		g.nodes[2].connect(g.nodes[7]);
		g.nodes[3].connect(g.nodes[4]);
		g.nodes[3].connect(g.nodes[5]);
		g.nodes[3].connect(g.nodes[6]);
		g.nodes[3].connect(g.nodes[10]);
		g.nodes[4].connect(g.nodes[5]);
		g.nodes[4].connect(g.nodes[6]);
		g.nodes[4].connect(g.nodes[11]);
		g.nodes[5].connect(g.nodes[6]);
		g.nodes[5].connect(g.nodes[7]);
		g.nodes[5].connect(g.nodes[8]);
		g.nodes[6].connect(g.nodes[7]);
		g.nodes[6].connect(g.nodes[8]);
		g.nodes[7].connect(g.nodes[8]);
		g.nodes[7].connect(g.nodes[9]);
		g.nodes[9].connect(g.nodes[15]);
		g.nodes[10].connect(g.nodes[11]);
		g.nodes[11].connect(g.nodes[12]);
		g.nodes[12].connect(g.nodes[13]);
		g.nodes[13].connect(g.nodes[14]);
		g.nodes[14].connect(g.nodes[15]);

		g.CreateDisplayers();
	}

	public Graph getGraph() {
		return this.g;
	}
}
