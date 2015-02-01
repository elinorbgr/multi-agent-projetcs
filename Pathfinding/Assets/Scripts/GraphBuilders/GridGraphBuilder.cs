using UnityEngine;
using System.Collections;

public class GraphCreatorDiscrete: MonoBehaviour, IGraphBuilder {
	
	private Graph g;
	
	// Use this for initialization
	void Start () {
		g = new Graph();
		
		for(int i = -4; i <= 4; i += 2){
			for(int j = -4; j <= 4; j += 2){
				g.nodes.Add(new Graph.Node(new Vector3(i, 0.5F,j)));
			}
		}

		foreach(Graph.Node n in g.nodes){
			foreach(Graph.Node m in g.nodes){
				Vector3 direction = m.pos - n.pos;
				float distance = direction.magnitude;
				if(distance == 2 && !Physics.Raycast(n.pos, direction, distance+0.25f)){
					//the "distance == 2" tells the nodes to connect only their adjacent nodes
					n.connect(m);
				}
			}
		}
		
		g.CreateDisplayers();
	}

	Graph IGraphBuilder.getGraph() {
		return this.g;
	}
	
}
