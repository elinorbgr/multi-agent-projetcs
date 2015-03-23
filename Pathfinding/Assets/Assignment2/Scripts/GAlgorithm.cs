using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GAlgorithm : MonoBehaviour {
	
	private List<int> Solution;
	public int generations;
	private int num_nodes;
	private List<float> costList;

	private string[] map;
	private List<Vector3> vertices;
	
	private int num_mobile;
	private GameObject[] mobiles;
	private GameObject[] customers;
	public int popSize;
	public float mutationRate;
	private List<List<int>> population;
	private List<List<Vector3>> paths;
	private RRTTree<Vector3> tree;

	public float acceleration;
	public float minx;
	public float miny;
	public float maxx;
	public float maxy;

	private List<int> bestSol;
	private float bestVal;
	private float distance;
	
	void Start(){

		customers = GameObject.FindGameObjectsWithTag ("Customer");
		mobiles = GameObject.FindGameObjectsWithTag ("Mobile");
		num_mobile = mobiles.Length;
		num_nodes = customers.Length;
		tree = new RRTTree<Vector3>(new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f));
		
		population = new List<List<int>> ();
		costList = new List<float> ();
		for(int i=0;i<popSize;i++){population.Add(new List<int>());
			costList.Add(0);}

		
		if (popSize % 2 != 0) {popSize=popSize+1;}
		//Main cycle
		for (int run=0; run<num_nodes; run++) {
						for (int k=0; k<popSize; k++) {
								Solution = new List<int> ();
								//Randomize a starting solution
								//---------------------------------
								for (int i=1; i<=num_nodes; i++) {
										Solution.Add (i);					
								}
								Shuffle (Solution);
				
								int rand = UnityEngine.Random.Range (1, num_nodes - num_mobile + 2);
								int remain = num_nodes - rand;
								Solution.Add (rand);
				
								for (int i=1; i<num_mobile-1; i++) {
										rand = UnityEngine.Random.Range (1, remain - num_mobile + 2 + i);
										remain = remain - rand;
										Solution.Add (rand);
								}
				
								Solution.Add (remain);
								// --------------------------------
								population [k] = Solution;
						}
						//Generations
						for (int gen = 0; gen<generations; gen++) {
								//Do crossover
								for (int j=0; j<popSize; j+=2) {
										List<List<int>> children = crossover (population [j], population [j + 1]);
										List<int> child1 = children [0];
										List<int> child2 = children [1];
									

										float costold1;
										float costold2;
										float cost1 = cost (child1, mobiles, customers);
										float cost2 = cost (child2, mobiles, customers);

										if(costList[j]==0 || costList[j+1]==0){
										costold1 = cost (population [j], mobiles, customers);
										costold2 = cost (population [j + 1], mobiles, customers);
										}
										else{costold1 = costList[j];
											costold2 = costList[j+1];}
				
										if (cost1 < costold1) {
												population [j] = child1;
												costList[j] = cost1;
										}
										if (cost2 < costold2) {
												population [j + 1] = child2;
												costList[j+1]=cost2;
										}
				
								}
			
								//Do mutations for x% of population
								int rate = (int)mutationRate * popSize;
								for (int x = 0; x<rate; x++) {
										int random = UnityEngine.Random.Range (0, num_nodes);
										print ("random" + random);
										print (population.Count);
										int random2 = UnityEngine.Random.Range (0, 3);
										if (random2 == 0) {
												int start = UnityEngine.Random.Range (0, num_nodes);
												int len = UnityEngine.Random.Range (1, num_nodes - 1);
												population [random] = mutate_1 (population [random], start, len);
										}
										if (random2 == 1) {
												int a = UnityEngine.Random.Range (0, num_nodes);
												int b = UnityEngine.Random.Range (0, num_nodes);
												population [random] = mutate_2 (population [random], a, b);
										}
										if (random2 == 2) {
												int ins = UnityEngine.Random.Range (0, num_nodes);
												int pos = UnityEngine.Random.Range (0, num_nodes);
												population [random] = mutate_3 (population [random], ins, pos);
										}
								}
			
								//Find best solution in population
								bestVal = Mathf.Infinity;
								float value;
								int index = 0;
								foreach (List<int> sol in population) {
										value = costList[index];
										index++;
										if (value < bestVal) {
												bestSol = sol;
												bestVal = value;
										}
								}
			
			
						}
				}
		paths = new List<List<Vector3>> ();
		for(int x=0;x<mobiles.Length;x++){paths.Add(new List<Vector3>());}

		int st = 0;
		int end = 0;
		for(int i=0;i<mobiles.Length;i++){
			st = end;
			end = end + bestSol[i+num_nodes];
			for(int j=st;j<=end;j++){
				paths[i].Add(customers[bestSol[j]-1].transform.position);
			}
			((IMotionModel) this.mobiles[i].GetComponent(typeof(IMotionModel))).SetWaypoints(paths[i]);
		}

		print ("value = "+bestVal);
		string chi = "";
		
		foreach(int lol in bestSol){chi+=lol;}
		
		print ("chi = " + chi);
		
		
	}
	//This cost does not consider going to the goal afterwards
	//Only straight lines so far (Use RRT?)
	public float cost(List<int> sol, GameObject[] mob, GameObject[] cust){
		distance = 0;
		int start = 0;
		int end = 0;

		for(int i= 0;i<mob.Length;i++){
			start = end;
			end = end + sol[i+num_nodes];
			distance += (mob[i].transform.position-cust[sol[start]-1].transform.position).magnitude;
			for(int j= start; j < end; j++){
				Vector3 startPos = cust[sol[j]-1].transform.position;
				Vector3 goal = cust[sol[j+1]-1].transform.position;
				distance += (startPos-goal).magnitude;
			}
		}
		return distance;
	}
	
	public List<int> mutate_1(List<int> sol,int start,int len){
		List<int> sub = new List<int> ();
		if (start > num_nodes) {
			start = num_nodes;		
		}
		if (len + start > num_nodes) {
			len = num_nodes-start;		
		}
		if (start > 0) {
			for(int i = 0;i<start-1;i++){
				sub.Add(sol[i]);
			}
			for(int i=start+len-2;i>start-2;i--){
				sub.Add(sol[i]);
			}
			for(int i=start+len-1;i<num_nodes;i++){
				sub.Add(sol[i]);
			}
		}
		else if (start == 0) {
			for(int i=len-1;i>=0;i--){
				sub.Add(sol[i]);
			}
			for(int i=len;i<num_nodes;i++){
				sub.Add(sol[i]);
			}
		}
		
		for(int i= num_nodes;i<sol.Count;i++){
			sub.Add(sol[i]);
		}
		return sub;
	}
	public List<int> mutate_2(List<int> sol,int a, int b){
		List<int> sub = new List<int>();
		for(int i = 0;i<sol.Count;i++){
			sub.Add(sol[i]);
		}
		int med = sub [a - 1];
		sub [a - 1] = sub [b - 1];
		sub [b - 1] = med;
		return sub;
		
	}
	public List<int> mutate_3(List<int> sol,int ins, int pos){
		List<int> sub = new List<int>();
		for(int i = 0;i<sol.Count;i++){
			sub.Add(sol[i]);
		}
		int med = sub [pos - 1];
		sub.Remove (med);
		sub.Insert (ins-1,med);
		
		return sub;
	}
	public List<List<int>> crossover(List<int> mom, List<int> dad){
		List<List<int>> kids = new List<List<int>> ();
		List<int> child1 = new List<int>();
		List<int> child2 = new List<int>();
		string rnd = "";
		for (int i = 0; i< num_nodes; i++) {
			int rand = UnityEngine.Random.Range(0,2);
			rnd+= rand;
			
			if(rand >0.5){
				child1.Add(mom[i]);
				child2.Add(-1);
				
			}
			if(rand<0.5){
				child1.Add(-1);
				child2.Add(dad[i]);
			}
		}
		
		for (int m = 0;m<num_nodes;m++) {
			
			if(child1.Contains(dad[m])==false){child1[child1.IndexOf(-1)]=dad[m];}
			if(child2.Contains(mom[m])==false){child2[child2.IndexOf(-1)]=mom[m];}
		}
		
		for (int i = num_nodes; i<mom.Count; i++) {
			child1.Add(mom[i]);
			child2.Add(dad[i]);
		}
		kids.Add (child1);
		kids.Add (child2);
		
		return kids;
	}
	public static void Shuffle(List<int> list)  
	{   
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = UnityEngine.Random.Range(0,n+1);  
			int value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}
	
}
