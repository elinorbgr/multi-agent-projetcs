using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MapReader : MonoBehaviour {
	
	private string[] map;
	private List<Vector3> vertices;
	public GameObject Mobile;
    public GameObject Goal;
	private Vector3 p1;
	private Vector3 p2;
	
	string[] readFile(string file){
		string text = System.IO.File.ReadAllText(file);
		string[] lines = Regex.Split(text,"\n");
		return lines;
	}
	
	public void makeWalls(List<Vector3> vertices)
	{
		for(int i=0;i<vertices.Count;i++){
			if(i==vertices.Count-1){
				p1 = vertices[0];
				p2 = vertices[vertices.Count-1];
			}
			if(i<vertices.Count-1){
				p2 = vertices[i+1];
				p1 = vertices[i];
			}
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Vector3 direction = p2-p1;
			float length = direction.magnitude;
			cube.transform.localScale = new Vector3(1f,2f,length);
			cube.transform.rotation = Quaternion.LookRotation(direction);
			cube.transform.position = p1 + (direction/2);
			cube.renderer.material.color = Color.blue;
			
		}
		
		
		
	}
	
	void Start(){
		map = readFile ("Assets\\map_2.txt");
		Mobile.transform.position = new Vector3 (Single.Parse (map[1]), 0.5f, Single.Parse(map[2]));
        Goal.transform.position = new Vector3 (Single.Parse (map[4]), 0.5f, Single.Parse(map[5]));
        vertices = new List<Vector3>();
		for(int i = 6; i < map.Length-1;i+=2){
			if(map[i] == "End"){
				makeWalls(vertices);
				vertices = new List<Vector3>();
				i++;
			}
			if(i == map.Length-1){return;}
			if(map[i+1] != "End" && map[i] != "End"){
				vertices.Add(new Vector3(Single.Parse(map[i]),0.5f,Single.Parse(map[i+1])));
			}
			
		}
	}
}

