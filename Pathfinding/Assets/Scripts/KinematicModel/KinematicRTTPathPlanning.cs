using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinematicRTTPathPlanning {

    static private bool visible(Vector3 a, Vector3 b) {
        return !( Physics.Raycast(a, b-a, (b-a).magnitude)
                || Physics.Raycast(b, a-b, (a-b).magnitude));
    }

    static public List<Vector3> MoveOrder(Vector3 start, Vector3 goal, float minx, float miny, float maxx, float maxy) {

        RTTTree<Object> t = new RTTTree<Object>(start, null);

        if (visible(start, goal)) {
            RTTTree<Object>.Node g = new RTTTree<Object>.Node(goal, t.root, (goal-start).magnitude, null);
            t.nodes.Add(g);
            return g.pathFromRoot();
        }

        float baseradius = (maxx+maxy-minx-miny)/(2*5);

        for(int i = 0; i<1000; i++) { // do at most 10.000 iterations
            Vector3 point = new Vector3(Random.Range(minx, maxx), 0.5f, Random.Range(miny, maxy));
            RTTTree<Object>.Node p = t.nearestVisibleOf(point);
            if (p != null) {
                RTTTree<Object>.Node me = t.insert(point, p, (p.pos-point).magnitude, null);
                // RTT* stealing children
                List<KeyValuePair<float, RTTTree<Object>.Node>> targets = new List<KeyValuePair<float, RTTTree<Object>.Node>>();
                foreach(RTTTree<Object>.Node q in t.nodes) {
                    float d = (me.pos - q.pos).magnitude;
                    if (q != me && d < baseradius && visible(q.pos, me.pos)) {
                        targets.Add(new KeyValuePair<float, RTTTree<Object>.Node>(d, q));
                    }
                }
                targets.Sort((x, y) => x.Key.CompareTo(y.Key));
                float myd = me.fullCost();
                foreach(KeyValuePair<float, RTTTree<Object>.Node> kv in targets) {
                    if(kv.Value.fullCost() > myd + kv.Key) {
                        kv.Value.parent = me;
                        kv.Value.cost = (me.pos-kv.Value.pos).magnitude;
                    }
                }
            }
        }
        return t.nearestOf(goal).pathFromRoot();
    }
}
