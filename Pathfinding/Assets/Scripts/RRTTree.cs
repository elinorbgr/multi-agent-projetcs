using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RRTTree<T> {
    public class Node {
        public Vector3 pos;
        public Node parent;
        public float cost;
        public T data;

        public Node(Vector3 pos, Node parent, float cost, T data) {
            this.pos = pos;
            this.parent = parent;
            this.cost = cost;
            this.data = data;
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

        public float fullCost() {
            if (this.parent == null) {
                return 0.0F;
            } else {
                return this.parent.fullCost() + this.cost;
            }
        }
    }

    public Node root;
    public List<Node> nodes;

    public RRTTree(Vector3 root, T data) {
        this.root = new Node(root, null, 0f, data);
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

    public Node insert(Vector3 pos, Node parent, float cost, T data) {
        Node n = new Node(pos, parent, cost, data);
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
