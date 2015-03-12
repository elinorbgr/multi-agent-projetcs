using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Discrete2 : MonoBehaviour {
	
	private List<string> terrain;
	private int count;
	private GameObject GraphEn;
	// Use this for initialization
	void Start () {
		
		GraphEn = GameObject.Find("GraphBuilder");
		
		
		terrain = new List<string>();
		terrain.Add("10001110110001101010");
		terrain.Add("01001101000101001101");
		terrain.Add("00010000011001100100");
		terrain.Add("11100011000101010001");
		terrain.Add("11100100001101010001");
		terrain.Add("00000000000000101000");
		terrain.Add("10101000101001100101");
		terrain.Add("01000110011000000001");
		terrain.Add("00100100010100110001");
		terrain.Add("00011110000101100001");
		terrain.Add("10101111001011000000");
		terrain.Add("10011100010100101100");
		terrain.Add("00010001001111100110");
		terrain.Add("01000011011000110101");
		terrain.Add("10011001110100111100");
		terrain.Add("00101001000000010101");
		terrain.Add("10001000010000000111");
		terrain.Add("01000000101100101000");
		terrain.Add("10000011010100010001");
		terrain.Add("01100001011000000000");
		
		for (int i = 0; i<20;i++) {
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
