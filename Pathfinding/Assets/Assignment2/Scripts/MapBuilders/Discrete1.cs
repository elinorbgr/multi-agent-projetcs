using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Discrete1 : MonoBehaviour {
	
	private List<string> terrain;
	private int count;
	private GameObject GraphEn;
	// Use this for initialization
	void Start () {
		
		GraphEn = GameObject.Find("GraphBuilder");
		
		
		terrain = new List<string>();
		terrain.Add("00000000000");
		terrain.Add("01111111110");
		terrain.Add("00111111100");
		terrain.Add("00111111100");
		terrain.Add("00111011100");
		terrain.Add("00000000000");
		terrain.Add("00111111100");
		terrain.Add("01111111110");
		terrain.Add("01111111110");
		terrain.Add("01111111110");
		terrain.Add("00000000000");

		for (int i = 0; i<11;i++) {
			count = -1;
			foreach(char c in terrain[i]){
				count++;
				print ((int)Char.GetNumericValue(c)==1);
				if((int)Char.GetNumericValue(c)==1){
					GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.AddComponent<Rigidbody>();             
					cube.rigidbody.drag=0;
					cube.rigidbody.isKinematic=true;
					cube.transform.position = new Vector3(i, 0.5f, count);
				}
			}
		}
		
		GraphEn.GetComponent<FullConnectedGridGraphBuilder> ().enabled = false;
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyUp(KeyCode.Space))
		{
			GraphEn.GetComponent<FullConnectedGridGraphBuilder>().enabled = true;
		}
	}
}
