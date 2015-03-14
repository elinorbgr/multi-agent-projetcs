using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DifferentialDriveRRT : MonoBehaviour {
	
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
	static SteerResult steer(Vector3 start, Vector3 goal, float angle, float velocity, float maxSpeed, float maxRotSpeed, float length) {
		float step = 0.1f;
		float cost = 0f;
		Vector3 forward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
		while ((start-goal).magnitude > 1 && cost < 128) {
			Vector3 nextpos = start + forward.normalized * velocity * step;
			if(!visible(start, nextpos)) {
				// there is a collision, stop all !
				return new SteerResult(start, velocity, angle, cost, true);
			}
			Vector2 u = DifferentialDriveMotionModel.computeU(start, goal, forward, velocity, length, maxSpeed, maxRotSpeed);
			angle += u.y * step;
			velocity = u.x;
			start = nextpos;
			forward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
			cost += step;
		}
		return new SteerResult(start, velocity, angle, cost, false);
	}
	static void tryToSteal(RRTTree<Vector3> t, RRTTree<Vector3>.Node n, RRTTree<Vector3>.Node me, float maxSpeed, float maxRotSpeed, float length) {
		if (n.fullCost() <= me.fullCost()) { return; }
		SteerResult sr = steer(me.pos, n.pos, me.data.x, me.data.y , maxSpeed, maxRotSpeed,length);
		if (n.fullCost() <= (me.fullCost() + sr.cost)) { return; }
		if (((me.pos-n.pos)-n.data).magnitude < 1 && (sr.endpos-n.pos).magnitude < 1) {
			// near enough, we steal !
			if (n.isParentOf(me)) {
				Debug.Log("Auto-parenting attempted !");
				return;
			}
			n.parent = me;
			n.cost =  sr.cost;
		} else {
			// copy it with new speed and recurse !
			RRTTree<Vector3>.Node m = t.insert(sr.endpos, me, sr.cost, me.data);
			/*foreach (RRTTree<Vector3>.Node c in t.childrenOf(n)) {
				tryToSteal(t, c, m, maxSpeed,maxRotSpeed,length);
			}*/
		}
		
	}
	
	static public RRTTree<Vector3> MoveOrder(Vector3 start, Vector3 goal, Vector3 forward, float velocity, float maxSpeed, float maxRotSpeed, float length, float minx, float miny, float maxx, float maxy) {
		// the data member of the nodes of the tree is a Vector3 : the velocity of the mobile
		// when it reached it
		float angle = Mathf.Atan2(forward.z, forward.x);
		RRTTree<Vector3> t = new RRTTree<Vector3>(start, new Vector3(velocity,angle));
		if (visible(start, goal)) {
			// if the goal is visible from start, no need to think too much
			t.insert(goal, t.root, (start-goal).magnitude, new Vector3(0f,0f,0f));
			return t;
		}
		float baseradius = ((maxy-miny)+(maxx-minx))/32;

		for(int i = 0; i<1000; i++) { // do at most 1.000 iterations
			// draw a random point
			Vector3 point = new Vector3(Random.Range(minx, maxx), 0.5f, Random.Range(miny, maxy));
			// find the nearest node
			RRTTree<Vector3>.Node p = t.cheapestVisibleOf(point);
			if (p != null) {
				// try to simulate a move from p.pos with initial velocity p.data to point
				SteerResult sr = steer(p.pos, point, p.data.y, p.data.x, maxSpeed, maxRotSpeed, length);
				if (!sr.collided) {
					// the steering was successful (no collision with walls), we can keep the point !
					RRTTree<Vector3>.Node me = t.insert(sr.endpos, p, sr.cost, p.data);
					// steal neighbors
					/*foreach (RRTTree<Vector3>.Node n in t.visibleInRadius(me.pos, baseradius)) {
						if (n == me) { continue; }
						tryToSteal(t, n, me, me.data.x ,me.data.y ,length);
					}*/
				}
			}
		}
		return t;
	}
}
