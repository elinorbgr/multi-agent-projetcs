using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AssemblyCSharp
{
	public class VisibilityGraph
	{
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
								vertices.Add (new Vector3 (Single.Parse (map [i]), 0.5f, Single.Parse (map [i + 1])));
						}
				
				
				}
				int index = 0;
				foreach (GameObject mobile in mobiles) {
						foreach (GameObject customer in customers) {

								Vector3 dir = vertices [index] - customer.transform.position;
								if (!Physics.Raycast (customer.transform.position, dir, dir.magnitude)) {

								} else {
										edges [index].Add (0);
								}
						}
				}
		}
			public class Node {
				public Vector3 pos;
				public Node parent;

				
				public Node(Vector3 pos, Node parent) {
					this.pos = pos;
					this.parent = parent;
				}
				
				public List<Vector3> pathFromRoot() {
					LinkedList<Vector3> path = new LinkedList<Vector3> ();
					path.AddFirst(this.pos);
					Node p = this.parent;
					while(p != null) {
						path.AddFirst(p.pos);
						p = p.parent;
					}
					return new List<Vector3>(path);
				}
				
				
				public bool isParentOf(Node n) {
					while(n.parent != null) {
						if (n.parent == this) {
							return true;
						}
						n = n.parent;
					}
					return false;
				}
			}
			
			public Node root;
			public List<Node> nodes;
			
			public VisibilityGraph(Vector3 root) {
				this.root = new Node(root, null);
				this.nodes = new List<Node>();
				this.nodes.Add(this.root);
			}
			
			public static bool visible(Vector3 a, Vector3 b) {
				return !( Physics.Raycast(a, b-a, (b-a).magnitude)
				         || Physics.Raycast(b, a-b, (a-b).magnitude));
			}
			
			public Node nearestOf(Vector3 pos) {
				float min_dist = float.PositiveInfinity;
				Node nearest = null;
				foreach(Node n in this.nodes) {
					float dist = (pos - n.pos).magnitude;
					if (dist < min_dist) {
						min_dist = dist;
						nearest = n;
					}
				}
				return nearest;
			}
			
			public Node nearestVisibleOf(Vector3 pos) {
				float min_dist = float.PositiveInfinity;
				Node nearest = null;
				foreach(Node n in this.nodes) {
					float dist = (pos - n.pos).magnitude;
					if (dist < min_dist && visible(pos, n.pos)) {
						min_dist = dist;
						nearest = n;
					}
				}
				return nearest;
			}
			

			

			
			public List<Node> visibleInRadius(Vector3 pos, float r) {
				List<Node> lst = new List<Node>();
				foreach(Node n in this.nodes) {
					float dist = (pos - n.pos).magnitude;
					if (dist <= r && visible(pos, n.pos)) {
						lst.Add(n);
					}
				}
				return lst;
			}
			
			public List<Node> childrenOf(Node p) {
				List<Node> lst = new List<Node>();
				foreach(Node n in this.nodes) {
					if (n.parent == p) {
						lst.Add(n);
					}
				}
				return lst;
			}
			
			public Node insert(Vector3 pos, Node parent) {
				Node n = new Node(pos, parent);
				this.nodes.Add(n);
				return n;
			}
			
			public void drawGizmos() {
				Gizmos.color = Color.blue;
				foreach(Node n in this.nodes) {
					if (n.parent != null) {
						Gizmos.DrawLine(n.pos, n.parent.pos);
					}
				}
			}

	}

}

