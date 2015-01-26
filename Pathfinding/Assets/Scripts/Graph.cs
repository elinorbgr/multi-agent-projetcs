using System.Collections.Generic;

public class Graph {

	public class Node {
        public Vector3 pos;
        public List<Node> neighbors;

        public Node(Vector3 pos) {
            this.pos = pos;
            this.neighbors = new List();
        }

        public connect(Node other) {
            this.neighbors.add(other);
            other.neighbors.add(this);
        }
    }

}
