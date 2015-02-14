using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicRTTPathPlanning {

    static private bool visible(Vector3 a, Vector3 b) {
        return !( Physics.Raycast(a, b-a, (b-a).magnitude)
                || Physics.Raycast(b, a-b, (a-b).magnitude));
    }

    static public List<Vector3> MoveOrder(Vector3 start, Vector3 goal, float minx, float miny, float maxx, float maxy) {

        RTTTree<Vector3> t = new RTTTree<Vector3>(start, new Vector3(0f, 0f, 0f));

        if (visible(start, goal)) {
            RTTTree<Vector3>.Node g = t.insert(goal, t.root, (start-goal).magnitude, new Vector3(0f,0f,0f));
            return g.pathFromRoot();
        }

        float baseradius = (maxx+maxy-minx-miny)/(2*5);

        for(int i = 0; i<1000; i++) { // do at most 10.000 iterations
            Vector3 point = new Vector3(Random.Range(minx, maxx), 0.5f, Random.Range(miny, maxy));
            RTTTree<Vector3>.Node p = t.nearestVisibleOf(point);
            if (p != null) {
                t.insert(point, p, (point-p.pos).magnitude, new Vector3(0f, 0f, 0f));
            }
        }
        return t.nearestOf(goal).pathFromRoot();
    }
}
