using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchState {

	public Node node;
	public float g;
	public float h;
	public float f;
	public SearchState parent;
	public int depth;

	public SearchState(Node state, float g, SearchState parent=null) {
		this.node = state;
		this.g = g;
		this.h = 0;
		this.f = g;
		this.parent = parent;
		if (parent != null) {
			this.depth = parent.depth + 1;
		} else {
			this.depth = 0;
		}
	}

	public SearchState(Node state, float g, float h, SearchState parent=null) {
		this.node = state;
		this.g = g;
		this.h = h;
		this.f = g + h;
		this.parent = parent;
		if (parent != null) {
			this.depth = parent.depth + 1;
		} else {
			this.depth = 0;
		}
	}

	public override string ToString () {
		return string.Format ("[SearchState] g={0}, h={1}, f={2}, node.x={3}, node.y={4}", g, h, f, node.gridX, node.gridY);
	}
}
