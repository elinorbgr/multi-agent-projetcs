using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DiscreteTerrain : MonoBehaviour {

    private List<string> terrain;
    private int count;
    private GameObject GraphEn;
    // Use this for initialization
    void Start () {

        GraphEn = GameObject.Find("GraphBuilder");


        terrain = new List<string>();
        terrain.Add("10101101000001000000");
        terrain.Add("10011100010100101100");
        terrain.Add("00000001001100100110");
        terrain.Add("01000010001000110100");
        terrain.Add( "10011001110000011000");
        terrain.Add( "00100001000000010100");
        terrain.Add( "10001000010000000110");
        terrain.Add( "00000000101100101000");
        terrain.Add( "10000011010100010001");
        terrain.Add( "01000000011000000000");
        terrain.Add( "00011101100011010100");
        terrain.Add( "10010010001010011010");
        terrain.Add( "00100000110001001001");
        terrain.Add( "01000110001010000000");
        terrain.Add( "10000000011010100000");
        terrain.Add( "00000000000001010000");
        terrain.Add( "00010001010011001000");
        terrain.Add( "10001100010000000011");
        terrain.Add( "01001000000001100001");
        terrain.Add( "00111000001011000010");


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
