using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AssemblyCSharp
{
	public class VisibilityGraph: MonoBehaviour, IGraphBuilder
	{
		private Graph g;
		private string[] map;
		private List<Vector3> vertices;
		private List<List<int>> edges;
		private GameObject[] mobiles;
		private GameObject[] customers;

		private Vector3 pos;
		private List<Vector3> connections;
		
		string[] readFile(string file){
			string text = System.IO.File.ReadAllText(file);
			string[] lines = Regex.Split(text,"\n");
			return lines;
		}
		void Start(){
			g = new Graph();

				customers = GameObject.FindGameObjectsWithTag ("Customer");
				mobiles = GameObject.FindGameObjectsWithTag ("Mobile");
				edges = new List<List<int>> ();

				map = readFile ("Assets\\polygObst.txt");
				float vehicles = Single.Parse (map [0]);
				float cust = Single.Parse (map [1]);
				int startCount = 2 + 3 * (int)(2 * vehicles + cust);

				vertices = new List<Vector3> ();
				for (int i = startCount; i < map.Length; i+=2) {
						if (map [i] == "End\r") {
								i++;
						}
						if (i == map.Length - 1) {
								return;
						}
						if (map [i + 1] != "End\r") {
						g.nodes.Add(new Graph.Node(new Vector3 (Single.Parse (map [i]), 0.5f, Single.Parse (map [i + 1]))));
						}
				
				
				}
			foreach(Graph.Node n in g.nodes){
				foreach(Graph.Node m in g.nodes){
					Vector3 direction = m.pos - n.pos;
					float distance = direction.magnitude;
					if(!( Physics.Raycast(n.pos, direction, distance+0.25f) ||
					  Physics.Raycast(m.pos, -direction, distance+0.25f) )
					   ){
						//the "distance <= space" tells the nodes to connect only their adjacent nodes
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
}

