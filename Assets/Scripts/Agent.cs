using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Agent : MonoBehaviour {

	

	public bool reachedTargetPoint;
	public Color agentColor = Color.white;
	public List<GameObject> targets;
	public bool debugPath = true;
	public bool debugExpandedNodes = true;
	public int nodesExpanded = 0;
	public int nodesVisited = 0;
	public int pathCost = 0;
	public int totalCost = 0;


	private Text uniText; 
	private GameObject currentTarget;
	private SearchAlgorithm search;
    private OptimisationAlgorithm TargetOptmisation;
    private bool moveToNext;
	private bool isMoving;
	private bool costsNotComputed = false;
	private bool isAtTarget;
	private int currentCost;
	private bool agentRunning = false;
	protected List<Node> path = null;
	public TextAsset costTableFile= null;
	[HideInInspector] protected bool algorithmFinished = false;
	private int currentStartPos;
	private int currentTargetPos;
	[HideInInspector] private List<GameObject> allTargets;
	[HideInInspector] private List<string> keys;
	[HideInInspector] private Dictionary<string, Dictionary<string, int>> dMatrix;

	// Use this for initialization
	void Start () {

		

		search = GetSearchAlgorithm(); // 		
		TargetOptmisation = GetOptimisationAlgorithm();
        targets = null;



		//Node start_pos = GridMap.instance.NodeFromWorldPoint (transform.position);
        //transform.position = start_pos.worldPosition + new Vector3(0f,0.5f,0f);

		currentCost = 0;
		moveToNext = false;
		isMoving = false;
		costsNotComputed = true;
		isAtTarget = false;

		gameObject.GetComponent<Renderer> ().material.color = agentColor;

		getCostTable();

        Debug.Log("Done");

        TargetOptmisation.setDistanceMatrix(dMatrix);

	}

	private void getCostTable()
	{
		
		//Debug.Log("get cost table: ");

		// get all pos in a list
		//Debug.Log("start pos key: " + GridMap.instance.NodeFromWorldPoint(transform.position));
		allTargets = GetAllTargets();
		allTargets.Insert(0, this.gameObject); // put the starting position
		keys = new List<string>();

		dMatrix = new Dictionary<string, Dictionary<string, int>>();

		foreach (GameObject tobj in allTargets)
		{
			keys.Add(GridMap.instance.NodeFromWorldPoint(tobj.transform.position).ToString());
		}

		string tempCostsfn = GridMap.instance.mapCosts.name + "costs";
		if (costTableFile != null &&  tempCostsfn.Equals(costTableFile.name))
		{
			// read data
			Debug.Log("Reading: "+ costTableFile.name);
			string[] strCostTable = costTableFile.text.Split('\n');
			string[] header = strCostTable[0].Split(',');
			costsNotComputed = false;

			// 1. check lengths
			if (header.Length-1 != keys.Count)
			{
				Debug.Log("file has different number of elements than the cenario " + (header.Length - 1) + " vs " + keys.Count);
				costsNotComputed = true;
			}

			if(!costsNotComputed)
			{ 
				//2. compare keys with header (string with string)
				costsNotComputed = false;
				foreach (string tmpk in header)
				{
					//Debug.Log(tmpk);
					if (tmpk == "-")
						continue; // skip

					if(!keys.Contains(tmpk))
					{
						// if a key from the file does not exist... we must compute it
						Debug.Log("key not found! (going to compute) " + tmpk);
						costsNotComputed = true; // for now it computes everything again
						break;
					}
					else
					{
						dMatrix.Add(tmpk, new Dictionary<string, int>());
					}
				}
			}
			if (!costsNotComputed) // cost are computed.. lets load !
			{
				// construct the dMatrix
				for (int i = 1; i < strCostTable.Length; i++)
				{
					string[] cost_line = strCostTable[i].Split(',');
					string tkey = cost_line[0];
					for(int j = 1; j < cost_line.Length; j++)
					{
						dMatrix[tkey].Add(header[j], int.Parse(cost_line[j]));
					}
				}
				// debug!
				//foreach(KeyValuePair<string, Dictionary<string, int>> key in dMatrix)
				//{
				//	foreach(KeyValuePair<string, int> subkey in dMatrix[key.Key])
				//	{
				//		Debug.Log(key.Key + " -> " + subkey.Key + " dist: " + dMatrix[key.Key][subkey.Key]);
				//	}
				//}
				Debug.Log("Costs Loaded!");
			}	
		}

		if (costsNotComputed)
		{
			Debug.Log("Compute Costs");
			currentStartPos = 0;
			currentTargetPos = 0;
			search.startPos = allTargets[currentStartPos].transform.position;
			search.targetPos = allTargets[currentTargetPos].transform.position;
			search.setRunning(false);
			search.setFinished(false);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}


	// Update is called once per frame
	void Update () {
		if (!costsNotComputed) {
			if (Input.GetKeyDown (KeyCode.Space) && !agentRunning) {

                if (targets == null && !TargetOptmisation.Finished())
                {
                    Node start = GridMap.instance.NodeFromWorldPoint(this.transform.position);
                    uniText = GameObject.Find(GridMap.instance.prefixUiText + this.name).GetComponent<Text>();
                    uniText.text = this.name + ": Searching for Path Sequence... ";
                    uniText.text += "\n" + TargetOptmisation.GetCurrentSolution();
                    TargetOptmisation.StartRunning(start);

                }
            }

			if (TargetOptmisation.Finished() && !agentRunning)
			{
				targets = TargetOptmisation.GetBestSequenceFound();
				Debug.Log("Best Solution Found: " + TargetOptmisation.Evaluate(targets));
				agentRunning = true;
				UpdateStartTargetPositions();
				search.StartRunning();
				uniText.text = this.name + ": Search for Path Sequence... ";
				//print (GridMap.instance.NodeFromWorldPoint(search.startPos));
				// jncor: na verdade so deve ser chamado qdo terminar a procura da sequencia.. e executar agora a procura com caminhos etc..
			}
			else
			{
				executeSearchAlgorithm();
			}
		}
		else
		{
            computeCosts();
        }

	}

	private void computeCosts()
	{

		if (!search.GetRunning() && (!search.Finished() && !search.FoundPath()))
		{
			Debug.Log("[ComputeCosts] Start search for: " +keys[currentStartPos] + " -> " + keys[currentTargetPos]);
			search.StartRunning();
			uniText = GameObject.Find(GridMap.instance.prefixUiText + this.name).GetComponent<Text>();
		}

		if (search.GetRunning())
		{
			uniText.text = this.name + ": Searching... ";
			uniText.text += "expanded: " + nodesExpanded + " visited: " + nodesVisited;
		}

		// generate list of lists of costs
		if (search.FoundPath())
		{

			Dictionary<string, int> novo = new Dictionary<string, int>();
			
			path = search.RetracePath();

			novo.Add(keys[currentTargetPos], 0 + search.GetPathCost());

			if (dMatrix.ContainsKey(keys[currentStartPos]))
			{
				dMatrix[keys[currentStartPos]].Add(keys[currentTargetPos], 0 + search.GetPathCost());
			}
			else
			{
				dMatrix.Add(keys[currentStartPos], novo);
			}
			
			//Debug.Log(dMatrix);
			//Debug.Log("Ended! cost : "+ dMatrix[keys[currentStartPos]][keys[currentTargetPos]] + 
			//	" search for: " + keys[currentStartPos] + "-> " + keys[currentTargetPos]);
			
			currentTargetPos++;
			if (currentTargetPos == keys.Count)
			{
				currentTargetPos = 0;

				currentStartPos++;
				if (currentStartPos == keys.Count)
				{
					costsNotComputed = false; // computed!
					exportCosts();
					search.setRunning(false);
					search.setFinished(false);
					path = null;
					Debug.Log("Computed and exported to: " +
						(Application.dataPath + "/Resources/Maps/" + GridMap.instance.mapCosts.name + "costs.csv"));

					return;
				}
				
			}

			search.startPos = allTargets[currentStartPos].transform.position;
			search.targetPos = allTargets[currentTargetPos].transform.position;
			Debug.Log("[ComputeCosts] Start search for: " + keys[currentStartPos] + " -> " + keys[currentTargetPos]);
			search.StartRunning();
		}

	}

	private void exportCosts()
	{
		String costs = "-";
		foreach(string k in keys)
		{
			costs += "," + k;
		}
		costs += "\n";

		foreach (string k in keys)
		{
			costs += k;
			foreach (string ok in keys)
			{
                if(ok.Equals(k)) {
					costs += ",0";
					continue; 
				}
                //Debug.Log(k + " -> " + ok);
				costs += "," + dMatrix[k][ok];
			}
			costs += "\n";
		}
		File.WriteAllText(Application.dataPath + "/Resources/Maps/" + GridMap.instance.mapCosts.name + "costs.csv", costs);
	}

	private void executeSearchAlgorithm()
	{

		if (debugExpandedNodes && search.GetRunning() && !search.Finished())
		{
			//GridMap.instance.ColorNodes(search.GetVisitedNodes(), agentColor);//jncor
			nodesExpanded = search.GetNumberOfNodesExpanded();
			nodesVisited = search.GetNumberOfVisitedNodes();
			pathCost = search.pathCost;
			uniText.text = this.name + ": Searching... ";
			uniText.text += "expanded: " + nodesExpanded + " visited: " + nodesVisited;
		}

		if (search.Finished() && !algorithmFinished)
		{
			//uniText.text = this.name + ": Moving... ";
			nodesExpanded = search.GetNumberOfNodesExpanded();
			nodesVisited = search.GetNumberOfVisitedNodes();
			pathCost = search.pathCost;
			
			if (path == null)
			{
				if (search.FoundPath())
				{
					
					path = search.RetracePath();
					if (debugPath)
					{

						GridMap.instance.ColorNodes(path, agentColor);
					}
				}
				else
				{
					
					algorithmFinished = true;
				}
			}
			uniText.text = "D31 - Configuration Final Path Cost: " + TargetOptmisation.GetCost();
            string temp = "";
            foreach(GameObject obj in TargetOptmisation.GetBestSequenceFound())
            {
                temp += obj.name + ", ";
            }

            uniText.text += "\n" + temp;

            if (targets.Count > 0)
			{
				if (moveToNext)
				{
					//// clear visited nodes
					//if (debugExpandedNodes)
					//{
					//	GridMap.instance.ClearColorNodes(search.GetVisitedNodes());
					//}
					//move to next target
					GridMap.instance.ClearColorNode(GridMap.instance.NodeFromWorldPoint(search.targetPos));
					UpdateStartTargetPositions();
					search.StartRunning();
					path = null;
					currentCost = 0;
					moveToNext = false;
					isAtTarget = false;
				}
			}
		}
		if (!isMoving)
		{
			if (isAtTarget)
				moveToNext = true;
		}
	}

	public void FixedUpdate() {
		Time.timeScale = 0.1f;
		if(path != null && !costsNotComputed) {
			Move ();
		}
		
	}

	/// <summary>
	/// Rotates the agent towards the next position.
	/// </summary>
	/// <param name="nextPos">Next position.</param>
	public void rotateAgent(Node nextPos)
	{
		if (nextPos.worldPosition.x > transform.position.x) {
			transform.forward = new Vector3 (0f, 0f, -1f);
		}else {
			if (nextPos.worldPosition.x < transform.position.x) {
				transform.forward = new Vector3 (0f, 0f, 1f);
			}
		}
		if (nextPos.worldPosition.z > transform.position.z){
			transform.forward = new Vector3 (1f, 0f, 0f);
		}else{
			if (nextPos.worldPosition.z < transform.position.z)
				transform.forward = new Vector3 (-1f, 0f, 0f);
		}
	}
	 
	public void Move() {
		if (path.Count > 0) {
			isMoving = true;
			GridMap.instance.ClearColorNode (GridMap.instance.NodeFromWorldPoint (search.startPos));
			Node nextPos = path [0];
			rotateAgent (nextPos);
			transform.position = nextPos.worldPosition + new Vector3(0, 1f, 0);
			if (debugPath) {
				//Destroy (GameObject.Find (name + " " + nextPos.gridX + "," + nextPos.gridY));
				GridMap.instance.ClearColorNode (nextPos);
			}
			currentCost += nextPos.gCost;
			path.Remove (nextPos);
		} else {
			if (isMoving) {
				// just update once
				currentTarget.GetComponent<Renderer> ().material.color = new Color (
					currentTarget.GetComponent<Renderer> ().material.color.r - .5f,
					currentTarget.GetComponent<Renderer> ().material.color.g - .5f,
					currentTarget.GetComponent<Renderer> ().material.color.b - .5f
				);

				totalCost += currentCost; // custo total
			}
			isAtTarget = true;
			isMoving = false;
		}
	}

	void UpdateStartTargetPositions() {
		search.startPos = transform.position;
		GridMap.instance.ColorNode (GridMap.instance.NodeFromWorldPoint (search.startPos), agentColor, 1);
		currentTarget = (GameObject) targets[0];
		targets.RemoveAt (0);
		search.targetPos = currentTarget.transform.position;
		//currentTarget.GetComponent<Renderer> ().material.color = Color.red; 
		currentTarget.GetComponent<Renderer> ().material.color = new Color (
			currentTarget.GetComponent<Renderer> ().material.color.r + .15f,
			currentTarget.GetComponent<Renderer> ().material.color.g + .15f,
			currentTarget.GetComponent<Renderer> ().material.color.b + .15f
		);
		GridMap.instance.ColorNode (GridMap.instance.NodeFromWorldPoint (search.targetPos), Color.black, 1);
	}

	public SearchAlgorithm GetSearchAlgorithm() {
		Component[] allAlgorithms = GetComponents<SearchAlgorithm> ();
		SearchAlgorithm firstActiveAlgorithm = null;
		foreach (SearchAlgorithm alg in allAlgorithms)
		{
			if (alg.isActiveAndEnabled) {
				firstActiveAlgorithm = alg;
				break;
			}
		}
		return firstActiveAlgorithm;
	}


    public OptimisationAlgorithm GetOptimisationAlgorithm()
    {
        Component[] allAlgorithms = GetComponents<OptimisationAlgorithm>();
        OptimisationAlgorithm firstActiveAlgorithm = null;
        foreach (OptimisationAlgorithm alg in allAlgorithms)
        {
            if (alg.isActiveAndEnabled)
            {
                firstActiveAlgorithm = alg;
                break;
            }
        }
        return firstActiveAlgorithm;
    }

	protected List<GameObject> GetAllTargets()
	{
		return new List<GameObject>(GameObject.FindGameObjectsWithTag("Pickup"));
	}

}
