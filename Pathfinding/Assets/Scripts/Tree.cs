using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tree {
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
    }

    public Node root;
    public List<Node> nodes;

    public Tree(Vector3 root) {
        this.root = new Node(root, null);
        this.nodes = new List<Node>();
        this.nodes.Add(this.root);
    }

    private bool visible(Vector3 a, Vector3 b) {
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

    public Node connectNearestVisible(Vector3 pos) {
        float min_dist = float.PositiveInfinity;
        Node nearest = null;
        foreach(Node n in this.nodes) {
            float dist = (pos - n.pos).magnitude;
            if (dist < min_dist && visible(pos, n.pos)) {
                min_dist = dist;
                nearest = n;
            }
        }
        if (nearest == null) { return null; }
        Node node = new Node(pos, nearest);
        this.nodes.Add(node);
        return node;
    }
}
