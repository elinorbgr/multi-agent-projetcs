using UnityEngine;
using System.Collections;

public class GraphBuilderKinematic : MonoBehaviour {

	private Graph g;

	// Use this for initialization
	void Start () {
		g = new Graph();

		// create a grid of nodes
		for(int i = -4; i <= 4; i += 2){
			for(int j = -4; j <= 4; j += 2){
				g.nodes.Add(new Graph.Node(new Vector3(i, 0.5F,j)));
			}

		}
		// connect the nodes that can see each other
		foreach(Graph.Node n in g.nodes){
			foreach(Graph.Node m in g.nodes){
				Vector3 direction = m.pos - n.pos;
				float distance = direction.magnitude;
				if(distance!=0 && !Physics.Raycast(n.pos, direction, distance+0.25f)){ 
					// The 0.25 constant is to give some space next to the wall												
					n.connect(m);
				}

			}
			
		}

		g.CreateDisplayers();

	}
	public Graph getGraph() {
		return this.g;
	}

}
