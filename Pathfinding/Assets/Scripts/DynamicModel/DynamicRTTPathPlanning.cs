﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicRTTPathPlanning {

    static private bool visible(Vector3 a, Vector3 b) {
        return !( Physics.Raycast(a, b-a, (b-a).magnitude)
                || Physics.Raycast(b, a-b, (a-b).magnitude));
    }

    class SteerResult {
        public Vector3 endpos;
        public Vector3 velocity;
        public float cost;
        public bool collided;

        public SteerResult(Vector3 endpos, Vector3 velocity, float cost, bool collided) {
            this.endpos = endpos;
            this.velocity = velocity;
            this.cost = cost;
            this.collided = collided;
        }
    }

    static SteerResult steer(Vector3 start, Vector3 goal, Vector3 velocity, float acc) {
        float step = 0.1f;
        float cost = 0f;
        while ((start-goal).magnitude > 1 || cost < 3) {
            Vector3 nextpos = start + velocity * step;
            if(!visible(start, nextpos)) {
                // collision !
                return new SteerResult(start, velocity, cost, true);
            }
            velocity += DynamicMotionModel.computeAcceleration(start, velocity, goal, acc) * step;
            start = nextpos;
            cost += step;
        }
        return new SteerResult(start, velocity, cost, false);
    }

    static public List<Vector3> MoveOrder(Vector3 start, Vector3 goal, float acc, float minx, float miny, float maxx, float maxy) {

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
                SteerResult sr = steer(p.pos, point, p.data, acc);
                if (!sr.collided) {
                    t.insert(sr.endpos, p, sr.cost, sr.velocity);
                }
            }
        }
        return t.nearestOf(goal).pathFromRoot();
    }
}
