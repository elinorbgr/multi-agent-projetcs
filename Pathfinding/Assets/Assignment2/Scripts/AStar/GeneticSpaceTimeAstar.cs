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

    public float mutationRate;

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
        for(int i=0; i< this.mobiles.Count; i++) {
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

    public List<List<Vector3>> st_a_star(List<List<Vector3>> mobiles_clients) {

        R reservations = new ReservationMap(this.graph);

        int im = 0;
        List<List<Vector3>> paths = new List<List<Vector3>>();
        foreach(List<Vector3> clients in mobiles_clients) {
            List<Vector3> path = new List<Vector3>();
            uint time = 0;
            Vector3 start = ((Transform) this.mobiles[im].GetComponent(typeof(Transform))).position;
            foreach(Vector3 client in clients) {
                List<Graph.Node> nds = spaceTimeAStar(
                    this.graph.NearestNodeOf(start),
                    this.graph.NearestNodeOf(client),
                    reservations,
                    time
                );
                time += (uint) nds.Count;
                foreach (Graph.Node n in nds.GetRange(1, nds.Count-1)) {
                    path.Add(n.pos);
                }
                start = path[path.Count-1];
            }
            // Then go back home
            List<Graph.Node> nnds = SpaceTimeAStar.spaceTimeAStar(
                this.graph.NearestNodeOf(start),
                this.graph.NearestNodeOf(((Transform) this.mobiles[im].GetComponent(typeof(Transform))).position),
                reservations,
                time
            );
            if(nnds.Count > 1) {
                foreach (Graph.Node n in nnds.GetRange(1, nnds.Count-1)) {
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

    static public List<Graph.Node> spaceTimeAStar(Graph.Node start, Graph.Node goal, R reservations, uint starting_time) {
        // init
        HashSet<R.Entry> closedset = new HashSet<R.Entry>();
        HashSet<R.Entry> openset = new HashSet<R.Entry>();
        R.Entry beginning = new R.Entry(start, starting_time);
        openset.Add(beginning);
        Dictionary<R.Entry, R.Entry> came_from = new Dictionary<R.Entry, R.Entry>();
        Dictionary<R.Entry, float> g_score = new Dictionary<R.Entry, float>();
        g_score[beginning] = 0;
        Dictionary<R.Entry, float> f_score = new Dictionary<R.Entry, float>();
        f_score[beginning] = SpaceTimeAStar.distance(start, goal);

        while(openset.Count > 0) {
            // choose current
            R.Entry current = null;
            float best_f = float.PositiveInfinity;
            foreach (R.Entry e in openset) {
                if (f_score[e] < best_f) {
                    best_f = f_score[e];
                    current = e;
                }
            }
            // stop ?
            if (current.node == goal) {
                // Okay, no one will pass, we can stay here
                List<Graph.Node> path = new List<Graph.Node>();
                path.Add(current.node);
                R.Entry old = current;
                while (came_from.ContainsKey(current)) {
                    current = came_from[current];
                    path.Add(current.node);
                    reservations.reserveMove(current.node, old.node, old.time);
                    old = current;
                }
                path.Reverse();
                return path;
            }
            // step
            openset.Remove(current);
            closedset.Add(current);
            foreach (Graph.Node n in current.node.neighbors) {
                R.Entry e = new R.Entry(n, current.time+1);
                if (closedset.Contains(e)) { continue; }
                if (!reservations.isFree(e)) { continue; }
                float tentative_g_score = g_score[current] + 1;

                if (!openset.Contains(e) || tentative_g_score < g_score[e]) {
                    came_from[e] = current;
                    g_score[e] = tentative_g_score;
                    f_score[e] = tentative_g_score + SpaceTimeAStar.distance(e.node, goal);
                    openset.Add(e);
                }
            }
            // also try not moving
            {
                R.Entry e = new R.Entry(current.node, current.time+1);
                if ((!closedset.Contains(e)) && reservations.isFree(e))
                {
                    float tentative_g_score = g_score[current] + 1;

                    if (!openset.Contains(e) || tentative_g_score < g_score[e]) {
                        came_from[e] = current;
                        g_score[e] = tentative_g_score;
                        f_score[e] = tentative_g_score + SpaceTimeAStar.distance(e.node, goal);
                        openset.Add(e);
                    }
                }
            }
        }

        return new List<Graph.Node>();
    }
}
