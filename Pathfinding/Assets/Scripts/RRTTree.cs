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
            float cost = 0f;
            Node me = this;
            while(me.parent != null) {
                cost += me.cost;
                me = me.parent;
            }
            return cost;
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

    public Node cheapestVisibleOf(Vector3 pos) {
        float min_cost = float.PositiveInfinity;
        Node cheapest = null;
        foreach(Node n in this.nodes) {
            float cost = (pos - n.pos).magnitude + n.fullCost();
            if (cost < min_cost && visible(pos, n.pos)) {
                min_cost = cost;
                cheapest = n;
            }
        }
        return cheapest;
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
