using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GeneticRRTcar : MonoBehaviour, ISynchroStart {
	
	public List<GameObject> mobiles;
	public List<GameObject> clients;
	
	public int poolSize;
	public int childrenCount;
	public int generations;
	
	public float mutationRate;
	
	public float acceleration;
	public float maxAngle;
	public float length;
	
	public float minx;
	public float miny;
	public float maxx;
	public float maxy;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.P))
		{
			Debug.Break();
		}
	}
	
	void ISynchroStart.prepare() {}
	
	void ISynchroStart.act() {
		Debug.Log("Gonna genalg !");
		List<List<Vector3>> paths = genalg();
		Debug.Log("genalged !");
		Debug.Break ();
		for(int i=0; i< this.mobiles.Count; i++) {
			(
				(IMotionModel) this.mobiles[i].GetComponent(typeof(IMotionModel))
				).SetWaypoints(paths[i]);
		}
	}
	
	
	class Individual {
		public List<List<Vector3>> genom;
		public List<List<Vector3>> paths;
		public float score;
		
		public Individual(List<List<Vector3>> genom, GeneticRRTcar simulator) {
			this.genom = genom;
			this.paths = simulator.generic_rrt(genom);
			System.GC.Collect();
			this.score = simulator.score(this.paths);
		}
	}
	
	private List<List<Vector3>> genalg() {
		List<Individual> pool = new List<Individual>();
		for(int i=0; i<this.poolSize; i++) {
			pool.Add(randomIndividual());
		}
		pool = pool.OrderBy(x => x.score).ToList();
		for(int i=0; i<this.generations; i++) {
			List<Individual> children = new List<Individual>();
			for(int j=0; j<this.childrenCount; j++) {
				Individual a = breedChoose(pool);
				Individual b = breedChoose(pool);
				children.Add(breed(a, b));
			}
			pool.AddRange(children);
			pool = pool.OrderBy(x => x.score).ToList();
			// kill the least fit
			pool.RemoveRange(pool.Count - this.childrenCount, this.childrenCount);
		}
		// now, return the best !
		Debug.Log(pool[0].score);
		return pool[0].paths;
	}
	
	private Individual breedChoose(List<Individual> pool) {
		System.Random r = new System.Random();
		double score = r.NextDouble();
		int M = pool.Count;
		for(int k=0; k<pool.Count; k++) {
			if (score < 2*((double)(M-k))/(M*(M+1))) {
				return pool[k];
			} else {
				score -= 2*((double)(M-k))/(M*(M+1));
			}
		}
		return pool[M-1];
	}
	
	private Individual randomIndividual() {
		List<List<Vector3>> genom = new List<List<Vector3>>();
		for(int i=0; i<this.mobiles.Count; i++) {
			genom.Add(new List<Vector3>());
		}
		foreach(GameObject c in this.clients) {
			Vector3 pos = ((Transform) c.GetComponent(typeof(Transform))).position;
			System.Random r = new System.Random();
			int j = r.Next(0, genom.Count);
			int k = r.Next(0, genom[j].Count);
			genom[j].Insert(k, pos);
		}
		return new Individual(genom, this);
	}
	
	private Individual breed(Individual a, Individual b) {
		// first, mix the genomes
		List<int> stolen_ids = new List<int>();
		// choose the half of genome of b that will be inserted into a
		System.Random r = new System.Random();
		for (int i=0; i<a.genom.Count; i++) {
			if(r.NextDouble() <= (a.genom.Count/2 - stolen_ids.Count)/(a.genom.Count-i)) {
				stolen_ids.Add(i);
			}
			if (stolen_ids.Count >= a.genom.Count/2) { break; }
		}
		// choose the sublists to extract
		List<List<Vector3>> stolen = new List<List<Vector3>>();
		foreach (int i in stolen_ids) {
			if(b.genom[i].Count == 0) {
				stolen.Add(new List<Vector3>());
			} else {
				int j = r.Next(0, b.genom[i].Count);
				int k = r.Next(j, b.genom[i].Count);
				stolen.Add(b.genom[i].GetRange(j,k-j));
			}
		}
		// now copy genome a while epuring it
		List<List<Vector3>> new_genom = new List<List<Vector3>>();
		int id = 0;
		foreach (List<Vector3> l in a.genom) {
			List<Vector3> new_mobile = new List<Vector3>();
			foreach (Vector3 v in l) {
				// avoid values coming from b
				if (!stolen.Any(m => m.Contains(v))) {
					new_mobile.Add(v);
				}
			}
			// insert back the genome of b
			if (stolen_ids.Contains(id)) {
				int j = r.Next(0, new_mobile.Count);
				new_mobile.InsertRange(j, stolen[stolen_ids.IndexOf(id)]);
			}
			id++;
			new_genom.Add(new_mobile);
		}
		// now mutate it
		if(r.NextDouble() <= mutationRate) {
			int j = r.Next(0, new_genom.Count);
			int k = r.Next(j, new_genom.Count);
			List<Vector3> temp = new_genom[j];
			new_genom[j] = new_genom[k];
			new_genom[k] = temp;
		}
		int total_count = 0;
		for(int i=0; i<new_genom.Count; i++) {
			total_count += new_genom[i].Count;
			// flip some chromosoms
			if(r.NextDouble() <= mutationRate) {
				new_genom[i].Reverse();
			}
		}
		for(int i=0; i<total_count*mutationRate; i++) {
			int j = r.Next(0, new_genom.Count);
			if (new_genom[j].Count == 0) { continue; }
			int k = r.Next(0, new_genom[j].Count);
			Vector3 c = new_genom[j][k];
			new_genom[j].RemoveAt(k);
			j = r.Next(0, new_genom.Count);
			k = r.Next(0, new_genom[j].Count);
			new_genom[j].Insert(k, c);
		}
		
		// finished !
		return new Individual(new_genom, this);
	}
	
	public float score(List<List<Vector3>> paths) {
		float max = 0f;
		
		foreach(List<Vector3> p in paths) {
			float tmp = 0f;
			for(int i=0; i< p.Count-1; i++) {
				tmp += (p[i+1]-p[i]).magnitude;
			}
			if(tmp>max) { max = tmp;}
		}
		return max;
	}
	
	public List<List<Vector3>> generic_rrt(List<List<Vector3>> mobiles_clients) {
		List<List<Vector3>> paths = new List<List<Vector3>>();
		
		for(int i = 0; i<mobiles_clients.Count; i++) {
			List<Vector3> waypoints = new List<Vector3>();
			Vector3 start = ((Transform) this.mobiles[i].GetComponent(typeof(Transform))).position;
			
			foreach(Vector3 client in mobiles_clients[i]) {
				RRTTree<Vector2> wps = DynamicCarRRTPathPlanning.MoveOrder(
					start,
					client,
					mobiles[i].transform.forward,
					mobiles[i].rigidbody.velocity.magnitude,
					acceleration,
					maxAngle,
					length,
					minx,
					miny,
					maxx,
					maxy
					);
				List<Vector3> nds = wps.cheapestInRadius(client,10).pathFromRoot();
				waypoints.AddRange(nds);
				start = waypoints[waypoints.Count-1];
			}
			
			paths.Add(waypoints);
		}
		
		return paths;
	}
}
