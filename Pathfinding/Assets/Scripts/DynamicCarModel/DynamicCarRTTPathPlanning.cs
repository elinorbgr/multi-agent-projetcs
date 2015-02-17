using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicCarRTTPathPlanning : MonoBehaviour {

    static private bool visible(Vector3 a, Vector3 b) {
        return !( Physics.Raycast(a, b-a, (b-a).magnitude)
                || Physics.Raycast(b, a-b, (a-b).magnitude));
    }

    class SteerResult {
        public Vector3 endpos;
        public float velocity;
        public float angle;
        public float cost;
        public bool collided;

        public SteerResult(Vector3 endpos, float velocity, float angle, float cost, bool collided) {
            this.endpos = endpos;
            this.velocity = velocity;
            this.angle = angle;
            this.cost = cost;
            this.collided = collided;
        }
    }

    // Tries to simulate the mobile moving from 'start' with initial velocity 'velocity'
    // up to as near as possible to 'goal', with maximal acceleration 'acc'
    static SteerResult steer(Vector3 start, Vector3 goal, float angle, float velocity, float maxForce, float maxAngle, float length) {
        float step = 0.1f;
        float cost = 0f;
        Vector3 forward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        while ((start-goal).magnitude > 1 && cost < 16) {
            Vector3 nextpos = start + forward.normalized * velocity * step;
            if(!visible(start, nextpos)) {
                // there is a collision, stop all !
                return new SteerResult(start, velocity, angle, cost, true);
            }
            Vector2 u = DynamicCarMotionModel.computeU(start, goal, forward, velocity, length, maxForce, maxAngle);
            angle += Mathf.Tan(u.y) * velocity / length * step;
            velocity += u.x * step;
            start = nextpos;
            forward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            cost += step;
        }
        return new SteerResult(start, velocity, angle, cost, false);
    }

    static public RTTTree<Vector2> MoveOrder(Vector3 start, Vector3 goal, Vector3 forward, float velocity, float maxForce, float maxAngle, float length, float minx, float miny, float maxx, float maxy) {
        // the data member of the nodes of the tree is a Vector3 : the velocity of the mobile
        // when it reached it
        float angle = Mathf.Atan2(forward.z, forward.x);
        RTTTree<Vector2> t = new RTTTree<Vector2>(start, new Vector2(velocity, angle));

        for(int i = 0; i<10000; i++) { // do at most 1.000 iterations
            // draw a random point
            Vector3 point = new Vector3(Random.Range(minx, maxx), 0.5f, Random.Range(miny, maxy));
            // find the nearest node
            RTTTree<Vector2>.Node p = t.nearestVisibleOf(point);
            if (p != null) {
                // try to simulate a move from p.pos with initial velocity p.data to point
                SteerResult sr = steer(p.pos, point, p.data.y, p.data.x, maxForce, maxAngle, length);
                if (!sr.collided) {
                    // the steering was successful (no collision with walls), we can keep the point !
                    t.insert(sr.endpos, p, sr.cost, new Vector2(sr.velocity, sr.angle));
                }
            }
        }
        return t;
    }
}
