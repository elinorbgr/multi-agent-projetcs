using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using R = ReservationMap;

public class GeneticSpaceTimeAstar : MonoBehaviour, ISynchroStart {

    public GameObject map_builder;
    public List<GameObject> mobiles;
    public List<GameObject> clients;

    public int poolSize;
    public int childrenCount;
    public int generations;

    private Graph graph;

	// Use this for initialization
	void Start () {
        IGraphBuilder builder = (IGraphBuilder) map_builder.GetComponent(typeof(IGraphBuilder));
        this.graph = builder.getGraph();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void ISynchroStart.prepare() {}

    void ISynchroStart.act() {
        IGraphBuilder builder = (IGraphBuilder) map_builder.GetComponent(typeof(IGraphBuilder));
        this.graph = builder.getGraph();
        Debug.Log("Gonna genalg !");
        List<List<Vector3>> paths = genalg();
        Debug.Log("genalged !");
        for(int i=0; i<= this.mobiles.Count; i++) {
            (
                (GeneticSTASMobile) this.mobiles[i].GetComponent(typeof(GeneticSTASMobile))
            ).setWaypoints(paths[i]);
        }
    }


    class Individual {
        public List<List<Vector3>> genom;
        public List<List<Vector3>> paths;
        public uint score;

        public Individual(List<List<Vector3>> genom, GeneticSpaceTimeAstar simulator) {
            this.genom = genom;
            this.paths = simulator.st_a_star(genom);
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
            pool.RemoveRange(0, this.childrenCount);
        }
        // now, return the best !
        return pool[pool.Count-1].paths;
    }

    private Individual breedChoose(List<Individual> pool) {
        System.Random r = new System.Random();
        double score = r.NextDouble();
        int M = pool.Count;
        for(int k=0; k<pool.Count; k++) {
            if (score < 2*((double)k)/(M*(M+1))) {
                return pool[k];
            } else {
                score -= 2*((double)k)/(M*(M+1));
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
            int j = r.Next(0, b.genom[i].Count);
            int k = r.Next(j, b.genom[i].Count);
            stolen.Add(b.genom[i].GetRange(j,k));
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

        // finished !
        return new Individual(new_genom, this);
    }

    public List<List<Vector3>> st_a_star(List<List<Vector3>> mobiles_clients) {

        R reservations = new ReservationMap(this.graph);

        int im = 0;
        List<List<Vector3>> paths = new List<List<Vector3>>();
        foreach(List<Vector3> clients in mobiles_clients) {
            List<Vector3> path = new List<Vector3>();
            uint time = 0;
            Vector3 start = ((Transform) this.mobiles[im].GetComponent(typeof(Transform))).position;
            foreach(Vector3 client in clients) {
                List<Graph.Node> nds = SpaceTimeAStar.spaceTimeAStar(
                    this.graph.NearestNodeOf(start),
                    this.graph.NearestNodeOf(client),
                    reservations,
                    time
                );
                time += (uint) nds.Count;
                foreach (Graph.Node n in nds) {
                    path.Add(n.pos);
                }
            }
            paths.Add(path);
            im++;
        }

        return paths;

    }

    public uint score(List<List<Vector3>> paths) {
        uint s = 0;
        foreach (List<Vector3> l in paths) {
            if (l.Count > s) {
                s = (uint)l.Count;
            }
        }
        return s;
    }
}
