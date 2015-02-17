using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicRRTPathPlanning {

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

    // Tries to simulate the mobile moving from 'start' with initial velocity 'velocity'
    // up to as near as possible to 'goal', with maximal acceleration 'acc'
    static SteerResult steer(Vector3 start, Vector3 goal, Vector3 velocity, float acc) {
        float step = 0.1f;
        float cost = 0f;
        while ((start-goal).magnitude > 1 && cost < 16) {
            Vector3 nextpos = start + velocity * step;
            if(!visible(start, nextpos)) {
                // there is a collision, stop all !
                return new SteerResult(start, velocity, cost, true);
            }
            velocity += DynamicMotionModel.computeAcceleration(start, velocity, goal, acc) * step;
            start = nextpos;
            cost += step;
        }
        return new SteerResult(start, velocity, cost, false);
    }

    static public RRTTree<Vector3> MoveOrder(Vector3 start, Vector3 goal, float acc, float minx, float miny, float maxx, float maxy) {
        // the data member of the nodes of the tree is a Vector3 : the velocity of the mobile
        // when it reached it
        RRTTree<Vector3> t = new RRTTree<Vector3>(start, new Vector3(0f, 0f, 0f));

        if (visible(start, goal)) {
            // if the goal is visible from start, no need to think too much
            t.insert(goal, t.root, (start-goal).magnitude, new Vector3(0f,0f,0f));
            return t;
        }

        for(int i = 0; i<10000; i++) { // do at most 10.000 iterations
            // draw a random point
            Vector3 point = new Vector3(Random.Range(minx, maxx), 0.5f, Random.Range(miny, maxy));
            // find the nearest node
            RRTTree<Vector3>.Node p = t.nearestVisibleOf(point);
            if (p != null) {
                // try to simulate a move from p.pos with initial velocity p.data to point
                SteerResult sr = steer(p.pos, point, p.data, acc);
                if (!sr.collided) {
                    // the steering was successful (no collision with walls), we can keep the point !
                    t.insert(sr.endpos, p, sr.cost, sr.velocity);
                }
            }
        }
        // now, simple return the best path from the tree
        return t;
    }
}
