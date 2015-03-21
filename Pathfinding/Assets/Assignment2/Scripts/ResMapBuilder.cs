using UnityEngine;
using System.Collections;

public class ResMapBuilder : MonoBehaviour {

    public GameObject graphBuilder;

    private ReservationMap map;

	// Use this for initialization
	void Start () {
        IGraphBuilder builder = (IGraphBuilder) graphBuilder.GetComponent(typeof(IGraphBuilder));
        this.map = new ReservationMap(builder.getGraph());
	}

    public ReservationMap getMap() {
        return this.map;
    }
}
