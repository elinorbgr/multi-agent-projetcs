using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinematicRTTPathPlanning {

    static private bool visible(Vector3 a, Vector3 b) {
        return !( Physics.Raycast(a, b-a, (b-a).magnitude)
                || Physics.Raycast(b, a-b, (a-b).magnitude));
    }

    static public List<Vector3> MoveOrder(Vector3 start, Vector3 goal) {

        float rmin = -4.5f;
        float rmax = 4.5f;

        Tree t = new Tree(start);

        if (visible(start, goal)) {
            Tree.Node g = new Tree.Node(goal, t.root);
            t.nodes.Add(g);
            return g.pathFromRoot();
        }

        for(int i = 0; i<10000; i++) { // do at most 10.000 iterations
            Vector3 point = new Vector3(Random.Range(rmin, rmax), 0.5f, Random.Range(rmin, rmax));
            Tree.Node n = t.connectNearestVisible(point);
            if (n != null) {
                if (visible(point, goal)) {
                    Tree.Node g = new Tree.Node(goal, n);
                    t.nodes.Add(g);
                    return g.pathFromRoot();
                }
            }
        }
        return t.nearestOf(goal).pathFromRoot();
    }

}
