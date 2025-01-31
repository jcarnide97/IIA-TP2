﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node  {
	public bool walkable;
	public Vector3 worldPosition;
	public int gCost;
	public int gridX;
	public int gridY;
	public Node parent = null;

	public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY) {
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
	}

	public override string ToString()
	{
		return "(" + this.gridX + ";" + this.gridY + ")";
	}
}
