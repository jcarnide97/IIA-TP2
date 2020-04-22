using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SearchAlgorithm : MonoBehaviour {

	public int stepsPerFrame = 500;
	[HideInInspector] public int pathCost;
	public int numberOfExpandedNodes = 0;
	public int numberOfVisited = 0;
	[HideInInspector] public Vector3 startPos;
	[HideInInspector] public Vector3 targetPos;



	protected Node startNode;
	protected Node targetNode;
	protected int numberOfSteps = 0;
	protected List<Node> path;
	protected int pathSize = 0;
	protected SearchState solution = null;
	protected bool foundPath = false;
	protected bool running = false;
	protected bool finished = false;



	public void StartRunning() {
		running = true;
		finished = false;
		numberOfSteps = 0;
		pathSize = 0;
		foundPath = false;
		numberOfExpandedNodes = 0;
		numberOfVisited = 0;
		pathCost = 0;
		solution = null;
		Begin ();
	}

	// Update is called once per frame
	void Update () {
		if (running && !finished) {
			for (int i = 0; i < stepsPerFrame; i++) {
				if (!finished) {
					Step ();
					numberOfSteps++;
					//if (numberOfExpandedNodes > maxNumberOfExpanded || maxListSize > listSizeLimit) {
					//	break;
					//}
				} else {
					break;
				}
			}
		}
	}

	public bool FoundPath() {
		return foundPath;
	}

	public int GetNumberOfSteps() {
		return numberOfSteps;
	}


	public  int GetNumberOfVisitedNodes() {
		return numberOfVisited;
	}

	public int GetNumberOfNodesExpanded() {
		return numberOfExpandedNodes;
	}


	public List<Node> RetracePath() {
		path = null;
		if (finished && foundPath) {
			path = new List<Node> ();
			// debug
			//List<SearchState> lstates = new List<SearchState> ();

			SearchState state = solution;
			pathCost = (int)solution.f;
			//print ("Path cost " + pathCost);
			while (state.parent != null) {
				path.Insert (0, state.node);
				//lstates.Insert (0, state);
				state = state.parent;
			}
		}
			
		return path;
	}



	public int GetPathCost() {
		return pathCost;
	}

	public bool Finished() {
		return finished;
	}
		

	public int GetPathSize() {
		return pathSize;
	}


	public bool GetRunning(){
		return running;
	}

	public void setRunning(bool state){
		running = state;
	}

	public void setFinished(bool state){
		finished = state;
	}

	public void VisitNode(SearchState state) {
		GridMap.instance.ColorNode(state.node,Color.blue);
		//nodesVisited.Add (state.node); // jncor é solucao.. fica a questao das cores por agente para resolver..
		numberOfVisited++;

	}

	protected List<Node> GetNodeSucessors(Node currentNode) {
		List<Node> neighbours = GridMap.instance.GetNeighbours (currentNode);
		numberOfExpandedNodes += neighbours.Count;
		return neighbours;
	}




	// These methods should be overriden on each specific search algorithm.
	protected abstract void Begin ();
	protected abstract void Step ();
	//NOTE: You have to implement this method if your algorithm requires an heuristic
	protected virtual int GetHeuristic() { return 0; }

}
