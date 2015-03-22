using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReservationMap {

    public class Entry {
        public Graph.Node node;
        public uint time;
        public Entry(Graph.Node node, uint time) {
            this.node = node;
            this.time = time;
        }
        public override bool Equals(object obj)
        {
            Entry o = obj as Entry;
            return (this.node.pos == o.node.pos) && (this.time == o.time);
        }
        public override int GetHashCode()
        {
            int h = this.node.pos.GetHashCode();
            h *= 256;
            h += (int) this.time;
            return h;
        }
    }

    public Graph graph;
    private Dictionary<Entry, bool> reservations;
    private Dictionary<Graph.Node, uint> reserved_since;
    private uint maxtime;

    public ReservationMap(Graph g) {
        this.graph = g;
        this.reservations = new Dictionary<Entry, bool>();
        this.reserved_since = new Dictionary<Graph.Node, uint>();
        maxtime = 0;
    }

    public bool isFree(Entry e) {
        if (this.reservations.ContainsKey(e)) {
            return !this.reservations[e];
        } else if (this.reserved_since.ContainsKey(e.node)){
            return this.reserved_since[e.node] > e.time;
        } else {
            return true;
        }
    }

    public bool isFutureProof(Entry e) {
        for (uint i = e.time; i <= maxtime; i++) {
            if (this.reservations.ContainsKey(new Entry(e.node, i))) {
                return false;
            }
        }
        return true;
    }

    public void reserveMove(Graph.Node from, Graph.Node to, uint time) {
        if (time > maxtime) { maxtime = time; }
        this.reservations[new Entry(from, time)] = true;
        this.reservations[new Entry(to, time)] = true;
    }

    public void reserveLocation(Graph.Node at, uint time) {
        if (time > maxtime) { maxtime = time; }
        this.reservations[new Entry(at, time)] = true;
    }
    public void unreserveLocation(Graph.Node at, uint time) {
        if (time > maxtime) { maxtime = time; }
        this.reservations[new Entry(at, time)] = false;
    }

    public void reserveForever(Graph.Node at, uint since) {
        if (since > maxtime) { maxtime = since; }
        this.reserved_since[at] = since;
    }

}
