using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : SearchAlgorithm {


	protected PriorityQueue openSet;


	protected override void Begin()
	{
		// inits..
		startNode = GridMap.instance.NodeFromWorldPoint (startPos);
		targetNode = GridMap.instance.NodeFromWorldPoint (targetPos);
		openSet = new PriorityQueue();
		SearchState start = new SearchState (startNode, 0, GetHeuristic(startNode, targetNode));
		openSet.Add(start, 0);
	}

	protected override void Step()
	{
		if (openSet.Count > 0) {

			SearchState currentState = openSet.PopFirst();
			VisitNode (currentState);

			if (currentState.node == targetNode) {
				solution = currentState;
				finished = true;
				running = false;
				foundPath = true;
			} else {
				foreach (Node suc in GetNodeSucessors(currentState.node)) {
					SearchState new_node = new SearchState (suc, suc.gCost + currentState.g, GetHeuristic(suc, targetNode) , currentState);
					openSet.Add (new_node, (int) new_node.f);

				}
			}
		} else {
			finished = true;
			running = false;
		}
	}
		
	protected int GetHeuristic(Node nodeA, Node nodeB) {
		int distX = Mathf.Abs (nodeB.gridX - nodeA.gridX);
		int distY = Mathf.Abs (nodeB.gridY - nodeA.gridY);
		return distX + distY; 

	}
}
