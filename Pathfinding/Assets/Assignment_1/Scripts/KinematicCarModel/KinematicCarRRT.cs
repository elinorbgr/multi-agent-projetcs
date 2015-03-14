using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinematicCarRRT : MonoBehaviour {
	
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
	static SteerResult steer(Vector3 start, Vector3 goal, float angle, float velocity, float speed, float maxAngle, float length) {
		float step = 0.1f;
		float cost = 0f;
		Vector3 forward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
		while ((start-goal).magnitude > 1 && cost < 256) {
			Vector3 nextpos = start + forward.normalized * velocity * step;
			if(!visible(start, nextpos)) {
				// there is a collision, stop all !
				return new SteerResult(start, velocity, angle, cost, true);
			}
			Vector2 u = KinematicCarMotionModel.computeU(start, goal, forward, velocity, length, speed, maxAngle);
			angle += Mathf.Tan(u.y) * velocity / length * step;
			velocity = u.x;
			start = nextpos;
			forward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
			cost += step;
		}
		return new SteerResult(start, velocity, angle, cost, false);
	}
	static void tryToSteal(RRTTree<Vector2> t, RRTTree<Vector2>.Node n, RRTTree<Vector2>.Node me, float maxSpeed, float maxAngle, float length) {
		if (n.fullCost() <= me.fullCost()) { return; }
		SteerResult sr = steer(me.pos, n.pos, me.data.y, me.data.x, maxSpeed, maxAngle, length);
		if (n.fullCost() <= (me.fullCost() + sr.cost)) { return; }
		if (Mathf.Abs(sr.angle-n.data.y) < 2*sr.velocity/length * Mathf.Tan(maxAngle) &&
		    Mathf.Abs(sr.velocity-n.data.x) < 3 &&
		    (sr.endpos-n.pos).magnitude < 1) {
			// near enough, we steal !
			if (n.isParentOf(me)) {
				Debug.Log("Auto-parenting attempted !");
				return;
			}
			n.parent = me;
			n.cost =  sr.cost;
		} else {
			// copy it with new speed and recurse !
			RRTTree<Vector2>.Node m = t.insert(sr.endpos, me, sr.cost, new Vector2(sr.velocity, sr.angle));
			/*foreach (RRTTree<Vector2>.Node c in t.childrenOf(n)) {
                tryToSteal(t, c, m, maxSpeed, maxAngle, length);
            }*/
		}
		
	}
	
	static public RRTTree<Vector2> MoveOrder(Vector3 start, Vector3 goal, Vector3 forward, float velocity, float maxSpeed, float maxAngle, float length, float minx, float miny, float maxx, float maxy) {
		// the data member of the nodes of the tree is a Vector3 : the velocity of the mobile
		// when it reached it
		float angle = Mathf.Atan2(forward.z, forward.x);
		RRTTree<Vector2> t = new RRTTree<Vector2>(start, new Vector2(velocity, angle));
		
		float baseradius = ((maxy-miny)+(maxx-minx))/16;
		
		for(int i = 0; i < 1000; i++) { // do at most 1.000 iterations
			// draw a random point
			Vector3 point = new Vector3(Random.Range(minx, maxx), 0.5f, Random.Range(miny, maxy));
			// find the nearest node
			RRTTree<Vector2>.Node p = t.cheapestVisibleOf(point);
			if (p != null) {
				// try to simulate a move from p.pos with initial velocity p.data to point
				SteerResult sr = steer(p.pos, point, p.data.y, p.data.x, maxSpeed, maxAngle, length);
				if (!sr.collided) {
					// the steering was successful (no collision with walls), we can keep the point !
					RRTTree<Vector2>.Node me = t.insert(sr.endpos, p, sr.cost, new Vector2(sr.velocity, sr.angle));
					
					/*foreach (RRTTree<Vector2>.Node n in t.visibleInRadius(me.pos, baseradius)) {
						if (n == me) { continue; }
						tryToSteal(t, n, me, maxSpeed, maxAngle, length);
					}*/
				}
			}
		}
		return t;
	}
}
