using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PriorityQueue {
	private int size;
	private SortedDictionary<int, Queue<SearchState>> data;

	public int Count { get { return size; } }

	public PriorityQueue()
	{
		size = 0;
		data = new SortedDictionary<int, Queue<SearchState>> ();
	}

	public void Add(SearchState s, int n)
	{
		if (data.ContainsKey (n)) {
			data [n].Enqueue (s);
		} else {
			Queue<SearchState> q = new Queue<SearchState> ();
			q.Enqueue (s);
			data [n] = q;
		}

		size++;
	}

	public SearchState PopFirst()
	{
		foreach (Queue<SearchState> q in data.Values) {
			if (q.Count > 0) {
				size--;
				return q.Dequeue ();
			}
		}

		return null;
	}
}
