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

        public float distToRoot() {
            if (this.parent == null) {
                return 0.0F;
            } else {
                return this.parent.distToRoot() + (pos - parent.pos).magnitude;
            }
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

    public void stealChildren(Node me, float maxdist) {
        List<KeyValuePair<float, Node>> targets = new List<KeyValuePair<float, Node>>();
        foreach(Node n in this.nodes) {
            float d = (me.pos - n.pos).magnitude;
            if (n != me && d < maxdist) {
                targets.Add(new KeyValuePair<float, Node>(d, n));
            }
        }
        targets.Sort((x, y) => x.Key.CompareTo(y.Key));
        float myd = me.distToRoot();
        foreach(KeyValuePair<float, Node> kv in targets) {
            if(kv.Value.distToRoot() > myd + kv.Key) {
                kv.Value.parent = me;
            }
        }
    }
}
