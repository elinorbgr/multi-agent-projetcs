using UnityEngine;
using System.Collections;

public class DuodecGridGraphBulder : MonoBehaviour, IGraphBuilder {

    private Graph g;
    public float space;
    public float top;
    public float bottom;
    public float left;
    public float right;
    
    // Use this for initialization
    void Start () {
        g = new Graph();
        
        for(float i = top; i <= bottom; i += space){
            for(float j = left; j <= right; j += space){
                g.nodes.Add(new Graph.Node(new Vector3(i, 0.5F,j)));
            }
        }

        foreach(Graph.Node n in g.nodes){
            foreach(Graph.Node m in g.nodes){
                Vector3 direction = m.pos - n.pos;
                float distance = direction.magnitude;
                if((distance <= space*1.5 ||
                    distance == Mathf.Sqrt(5f)) &&
                    !( Physics.Raycast(n.pos, direction, distance+0.25f) ||
                       Physics.Raycast(m.pos, -direction, distance+0.25f) )
                    ){
                    //the "distance <= space" tells the nodes to connect only their adjacent nodes
                    n.connect(m);
                }
            }
        }
        
        g.CreateDisplayers();
    }

    Graph IGraphBuilder.getGraph() {
        return this.g;
    }
}
