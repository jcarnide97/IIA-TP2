using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GridMap : MonoBehaviour {

	public LayerMask unwalkableMask;
	public LayerMask everything;
	public Vector2 gridWorldSize; // the size of the grid
	public float nodeRadius; //how much space the individual space covers
	public GameObject tile;
	public GameObject tileText;
	public bool showCosts = false;
	public TextAsset mapCosts = null;
	public bool randomCosts;

	protected float nodeDiameter;
	protected int gridSizeX, gridSizeY;
	private Node[,] grid;
	private GameObject tiles;
	private GameObject[,] gridTiles;
	private GameObject[,] gridCosts;
	private Color[,] gridColors;
	private bool last_state;
	private GameObject canvasObj;
	private GameObject rocks;
	[HideInInspector]
	public string prefixUiText = "Text-";

	//private string[] res = {"tree_a", "tree_b", "rock_b", "rock_c", "rock_d"};


	public static int SortByName(GameObject o1, GameObject o2) {
		return o1.name.CompareTo(o2.name);
	}
	public static GridMap instance = null;


	void Awake() {
		// Singleton pattern
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);   
		Init ();
	}

	/// <summary>
	/// Init this instance.
	/// </summary>
	void Init(){

		this.canvasObj = GameObject.Find("Canvas");
		last_state = showCosts;// saves last option on this field
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt( gridWorldSize.x / nodeDiameter); 
		gridSizeY = Mathf.RoundToInt( gridWorldSize.y / nodeDiameter); 
		tiles = GameObject.Find ("Tiles");
		CreateGrid ();
		Random.InitState (0); // reset for random algorithms

	}

	void Start()
	{
		InitTextUI ();
	}

	void InitTextUI()
	{
		GameObject[] units = GameObject.FindGameObjectsWithTag ("Unit");

		float next_pos = 0;

		foreach (GameObject unit in units) {
			GameObject go = Instantiate (Resources.Load ("Prefabs/CText")) as GameObject;
			go.name = this.prefixUiText + unit.name;
			RectTransform trans = go.GetComponent<RectTransform> ();
			trans.SetPositionAndRotation(new Vector3(trans.position.x,  -20f+ next_pos, trans.position.z),Quaternion.identity);
			Text text = go.GetComponent<Text> ();
			text.text = "-";
			text.color = Color.white;
			go.transform.SetParent (canvasObj.transform, false);
			next_pos -= 20f;

		}

		List<GameObject> targets = new List<GameObject> (GameObject.FindGameObjectsWithTag ("Pickup"));
		targets.Sort(SortByName);

		GameObject a_tile_text = null;
		int t = 1;
		foreach (GameObject target in targets) {

			// snap to grid !
			target.transform.position = GridMap.instance.NodeFromWorldPoint (target.transform.position).worldPosition;

			a_tile_text = Instantiate (tileText, target.transform.position + new Vector3(0,0.8f,0.2f), Quaternion.identity) as GameObject; 
			Node n = NodeFromWorldPoint (target.transform.position);
			a_tile_text.transform.Rotate(new Vector3(80f,0f,0f));
			TextMesh tmp = a_tile_text.GetComponent<TextMesh> ();
			tmp.text = "-"+ target.name + "-";
			tmp.color = Color.green;
			tmp.fontStyle = FontStyle.Bold;
			a_tile_text.name = "Target "+ t +" :(" + n.gridX + " , " + n.gridY + ") g: " + grid [n.gridX, n.gridY].gCost.ToString();
			t++;
		}

		// origin
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y/2;
		a_tile_text = Instantiate (tileText, worldBottomLeft + new Vector3(-0.75f,0f,0f), Quaternion.identity) as GameObject; 
		a_tile_text.transform.Rotate(new Vector3(80f,0f,0f));
		Node np = NodeFromWorldPoint (worldBottomLeft);
		TextMesh tmporig = a_tile_text.GetComponent<TextMesh> ();
		tmporig.text = "(0,0)";
		tmporig.color = Color.black;
		tmporig.fontStyle = FontStyle.Bold;
		a_tile_text.name = "Grid Origin  : " + np.gridX + " , " + np.gridY;
	}



	/// <summary>
	/// Update this instance, managing GUI artifacts based on the inspector options.
	/// </summary>
	void Update(){
		if(last_state != showCosts){
			last_state = showCosts;
			for (int x = 0; x < gridSizeX; x++) {
				for (int y = 0; y < gridSizeY; y++) {
					if (gridCosts [x, y] != null) {
						gridCosts [x, y].SetActive (!(gridCosts [x, y].activeSelf));
					}
				}
			}
		}
	}

	/// <summary>
	/// Creates the grid and initializes all the tiles and labels of walkable objects.
	/// </summary>
	void CreateGrid() {
		gridTiles = new GameObject[gridSizeX, gridSizeY];
		gridCosts = new GameObject[gridSizeX, gridSizeY];
		gridColors  = new Color[gridSizeX,gridSizeY];
		grid = new Node[gridSizeX, gridSizeY];

		string[] mapString = null;
		string[] cost_line = null;
		if (!randomCosts)
			mapString = mapCosts.text.TrimEnd('\n').Split('\n');

		GameObject targets_hier = GameObject.Find("Targets");
		bool loadAllFromFile = false;
		if(mapCosts.text.Contains("p") && mapCosts.text.Contains("r"))
		{
			Debug.Log("loading all from csv");
			loadAllFromFile = true;
		}
		else
		{
			Debug.Log("using scenario placement");
		}

		GameObject player = GameObject.Find("D31");
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y/2;
		int count_targets = 0;
		for (int y = 0; y < gridSizeY; y++) {
			if (!randomCosts)
				cost_line = mapString [gridSizeY - 1 - y].Split (',');
			
			for (int x = 0; x < gridSizeX; x++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere (worldPoint, nodeRadius, unwalkableMask));
				grid [x, y] = new Node (walkable, worldPoint, x, y);

				if (randomCosts) {
					grid [x, y].gCost = (int)(Random.value * 9f) + 1;
				}else if(mapString !=null) {
					int value = 1; // default
					if (cost_line[x].Equals("p"))
					{
						player.transform.position = grid[x, y].worldPosition;
					}
					else
					{
						if (cost_line[x].Equals("r"))
						{
							GameObject target = Instantiate(Resources.Load("Prefabs/Resource")) as GameObject;
							target.transform.position = grid [x, y].worldPosition;
							target.name = ""+count_targets;
							count_targets++;
						}
						else
						{// normal path
							value = int.Parse(cost_line[x]);
						}
					}
					
					if (value < 0)
					{
						walkable = false;
						grid[x, y].walkable = walkable;
						grid[x, y].gCost = -1;

						if (!Physics.CheckSphere(worldPoint, nodeRadius / 2, everything))
						{
							// if not added using inspector.. places a random asset instead
							PlaceUnWalkable(grid[x, y]);
						}
					}
					else
					{
						grid[x, y].gCost = value;
						
					}
				}

				if (!walkable) {
					continue;
				}

				// tiles
				GameObject a_tile = Instantiate (tile, worldPoint, Quaternion.identity) as GameObject; 
				a_tile.transform.SetParent (tiles.transform);
				a_tile.name = "T:(" + x + " , " + y+ ") g: " + grid [x, y].gCost;
				Color altColor = Color.black;
				altColor.g = 1 - ((float)grid [x, y].gCost /10.0f);
				a_tile.GetComponent<Renderer> ().material.color = altColor; 
				gridTiles[x, y] = a_tile;
				gridColors [x, y] = new Color(altColor.r, altColor.g, altColor.b);

				worldPoint.z += 0.4f;
				GameObject a_tile_text = Instantiate (tileText, worldPoint, Quaternion.identity) as GameObject; 
				a_tile_text.transform.SetParent (tiles.transform);
				a_tile_text.transform.Rotate(new Vector3(90f,0f,0f));
				//a_tile_text.GetComponent<TextMesh> ().text = grid [x, y].gCost.ToString ();
				a_tile_text.GetComponent<TextMesh>().text = "";
				a_tile_text.name = "C:(" + x + " , " + y+ ") g: " +grid [x, y].gCost;
				gridCosts[x, y] = a_tile_text;

			}
		}

		if (loadAllFromFile)
		{
			
			// enable unit 
			Debug.Log("Activate Unit");
			player.SetActive(true);
		}
	}

	public void PlaceUnWalkable(Node node) {
		//rocks = Instantiate (Resources.Load ("Models/"+res[(int) (Random.value * 4f)])) as GameObject;
		rocks = Instantiate(Resources.Load("Prefabs/Block")) as GameObject;
		rocks.transform.position = node.worldPosition;
		//rocks.transform.localScale = new Vector3 (1, 1, 1);
		rocks.layer = 8;
		rocks.transform.SetParent(GameObject.Find ("UnWalkable").transform);
		
	}



	public void ClearColorNode(Node n){
		this.gridTiles [n.gridX, n.gridY].GetComponent<Renderer> ().material.color = gridColors [n.gridX, n.gridY];
	}

	public void ClearColorNodes(List<Node> nodes){
		//foreach (Node n in nodes) {
		//	ClearColorNode (n);
		//}
	}

	public void ColorNode(Node n, Color c, float lerpfactor = 0.05f){
		//naml: COMENTEI ISTO PARA ADICONAR A AREIA!! 
		//this.gridTiles[n.gridX,n.gridY].GetComponent<Renderer> ().material.color = Color.Lerp(
		//	this.gridTiles[n.gridX,n.gridY].GetComponent<Renderer> ().material.color,
		//	c,
		//	lerpfactor
		//);
	}
		
	public void ColorNodes(List<Node> path, Color c){
		foreach (Node n in path) {
			ColorNode (n, c);
		}
	}


	/////////////////////////////////////// Game

	/// <summary>
	/// Get the Neighbours of a Node.
	/// </summary>
	/// <returns>The neighbours.</returns>
	/// <param name="node">Node.</param>
	public List<Node> GetNeighbours(Node node) {
		//                                     N                    O                 S                     E
		Vector2[] neighbourhood = { new Vector2 (0, 1), new Vector2 (1, 0), new Vector2 (0, -1), new Vector2 (-1, 0) };

		List<Node> neighbours = new List<Node> ();
		foreach (Vector2 pos in neighbourhood) {
			int checkX = node.gridX + (int) pos.x;
			int checkY = node.gridY + (int) pos.y;
			if(checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
				Node neighbour = grid [checkX, checkY];
				if(neighbour.walkable)
					neighbours.Add (neighbour);
			}
		}
		return neighbours;
	}

	/// <summary>
	/// Get a Node from world point.
	/// </summary>
	/// <returns>The from world point.</returns>
	/// <param name="worldPosition">World position.</param>
	public Node NodeFromWorldPoint(Vector3 worldPosition) {
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01 (percentX);
		percentY = Mathf.Clamp01 (percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

		return grid [x, y];
	}

	public Node[,] GetGrid() {
		return grid;
	}




}
