using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridfieldController : MonoBehaviour
{

	private bool isEmpty = true;
	public GameObject element = null;

	//Element Attributes
	public int totalConnections;
	public int leftConnections;
	public List<GameObject> connectedElements = new List<GameObject> ();
	public List<LineRenderer> lineRenderers = new List<LineRenderer> ();
	private Dictionary <LineRenderer, GameObject> lineDestinations = new Dictionary<LineRenderer, GameObject>();

	//Maximum amount of bonds with another neighbour
	private int maxBonds = 3;
	private float defaultBondWidth = 0.1f;

	private float maxLineLength = 3f;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		//Update LineRendererDestinations
		UpdateLineRenderers();
//		if(lineRenderers.Count > 0){
//			Debug.Log(gameObject.name + "'s Renderer List:");
//			for(int i = 0; i < lineRenderers.Count; i++){
//				Debug.Log("Item " + i + ": " + lineRenderers[i].name);
//			}
//			Debug.Log("///////////////////////////END////////////////////////////////////////");
//		}
	}

	void UpdateLineRenderers ()
	{
		List<LineRenderer> removables = new List<LineRenderer>();
		foreach (KeyValuePair<LineRenderer, GameObject> entry in lineDestinations) {
			Vector3 updatedNeighbourPos = new Vector3 (entry.Value.transform.position.x, entry.Value.transform.position.y, 0f);
			//Update Line Positions (in BondMode while User is dragging some Element to remove Bonds):
			//Update Neighbour Pos in case the neighbouring Element moved
			lineRenderers.Find (x => x.GetInstanceID () == entry.Key.GetInstanceID ()).SetPosition (1, updatedNeighbourPos);
			//Update My Element Pos in case my Element moved 
			lineRenderers.Find (x => x.GetInstanceID () == entry.Key.GetInstanceID ()).SetPosition (0, element.transform.position);
			//Check if line is too long and should be disconnected
//			Debug.Log ("Distance: " + Vector2.Distance (updatedNeighbourPos, element.transform.position));
			if (Vector2.Distance (updatedNeighbourPos, element.transform.position) > maxLineLength) {
				removables.Add (entry.Key);
			}
		}
		foreach(LineRenderer key in removables){
			foreach(GameObject neighbour in connectedElements){
				if(neighbour.GetComponent<GridfieldController>().GetElement().GetInstanceID() == lineDestinations[key].GetInstanceID()){
					//Found the neighbour which has this Element
					neighbour.GetComponent<GridfieldController> ().RemoveThisNeighbour (this.gameObject);
					RemoveThisNeighbour (neighbour);
					GameObject.FindWithTag("Gamefield").GetComponent<BondingControlller>().UpdateOverlayObject(neighbour.transform.position);
					break;
				}
			}

		}
		removables.Clear();
	}

	public void SetElement (GameObject _element)
	{
		element = _element;
		isEmpty = false;
		totalConnections = element.GetComponent<ElementInfo> ().GetNrOfBonds ();
		leftConnections = totalConnections;
	}

	public GameObject GetElement ()
	{
		return element;
	}

	public void DeclareEmpty ()
	{
		element = null;
		isEmpty = true;
		ResetAllConnections ();
	}

	public bool HasConnectionsLeft ()
	{
		if (leftConnections > 0)
			return true;
		return false;
	}

	public void SetNewConnection (GameObject neighbour)
	{
//		Debug.Log ("SetNewConnection Called! (Neighbour: " + neighbour + ")");
		if (neighbour.GetInstanceID () != this.gameObject.GetInstanceID ()) {
			if (HasConnectionsLeft ()) {
				if (neighbour.GetComponent<GridfieldController> ().HasConnectionsLeft ()) {
					if (!connectedElements.Contains (neighbour)) {
						connectedElements.Add (neighbour);
						LineRenderer lRenderer = new GameObject ("Line to " + neighbour.GetInstanceID ()).AddComponent<LineRenderer> ();
						lRenderer.transform.parent = this.transform;
						lRenderer.SetWidth (defaultBondWidth, defaultBondWidth);
						lRenderer.material = new Material (Shader.Find ("Sprites/Default"));
						lRenderer.SetColors(Color.black, Color.black);
						lRenderer.SetVertexCount (2);
						lRenderer.SetPosition (0, this.transform.position);
						lRenderer.SetPosition (1, neighbour.transform.position);
						lRenderer.sortingOrder = 2;
						lineRenderers.Add (lRenderer);
						lineDestinations.Add (lRenderer, neighbour.GetComponent<GridfieldController>().GetElement());
						neighbour.GetComponent<GridfieldController> ().AddThisAsNeighbour (this.gameObject);
						leftConnections--;
					} else {
						//Neighbour is at least once connected. Check for possible multiple bond.
						AddMultipleBondNeighbour (neighbour);
					}
				}
			} 
		}
	}

	public void AddMultipleBondNeighbour (GameObject neighbour)
	{
		//Get amount of bonds with this neighbour
		int nrOfBonds = 0;
		for (int i = 0; i < connectedElements.Count; i++) {
			if (connectedElements [i].GetInstanceID () == neighbour.GetInstanceID ()) {
				nrOfBonds++;
			}
		}

		//Update info and renderer if additional bond is possible
		if (nrOfBonds < maxBonds) {
			Debug.Log ("multiple Bond happening! NrOfBonds: " + nrOfBonds + ", MaxBonds: " + maxBonds);
			connectedElements.Add (neighbour);
			neighbour.GetComponent<GridfieldController> ().AddThisMultipleBondNeighbour (this.gameObject, nrOfBonds);
			leftConnections--;
			//Get the render
			bool newRenderer = false;
			LineRenderer lRenderer = new GameObject ("Line to " + neighbour.GetInstanceID () + nrOfBonds).AddComponent<LineRenderer> ();
			for(int i = 0; i < lineRenderers.Count; i++){
				if (lineRenderers[i].gameObject.name.Equals("Line to " + neighbour.GetInstanceID ())) {
					lRenderer.transform.parent = lineRenderers[i].transform.parent;
					lRenderer.material = lineRenderers[i].material;
					lRenderer.SetVertexCount (2);
					lRenderer.SetPosition(0, this.transform.position);
					lRenderer.SetPosition (1, neighbour.transform.position);
					lRenderer.sortingOrder = 2;
					//Change the render to display the multiple bond
				
					switch(nrOfBonds){
					case 1:
						//double bond
						lRenderer.SetColors(new Color(174f/255f, 174f/255f, 174f/255f), new Color(174f/255f, 174f/255f, 174f/255f));
						lRenderer.sortingOrder++;
						lRenderer.SetWidth (defaultBondWidth, defaultBondWidth);
						newRenderer = true;
						lineRenderers[i].SetWidth(defaultBondWidth*3, defaultBondWidth*3);
						break;
					case 2:
						//triple bond
						lRenderer.SetColors(Color.black, Color.black);
						lRenderer.sortingOrder += 2;
						lRenderer.SetWidth (defaultBondWidth/2, defaultBondWidth/2);
						newRenderer = true;
						lineRenderers[i].SetWidth(defaultBondWidth*2, defaultBondWidth*2);
						break;
					}
				}
			}
			if(newRenderer){
				lineRenderers.Add (lRenderer);
				lineDestinations.Add (lRenderer, neighbour.GetComponent<GridfieldController>().GetElement());
			}
		} else {
			Debug.Log (this.gameObject.name + "'s nr of bonds: " + nrOfBonds + "; Max bonds: " + maxBonds);
			//Max bonds alredy reached, reset all connections
			neighbour.GetComponent<GridfieldController> ().RemoveThisNeighbour (this.gameObject);
			RemoveThisNeighbour (neighbour);
		}
	}

	void AddThisMultipleBondNeighbour (GameObject neighbour, int nrOfBonds)
	{
		connectedElements.Add (neighbour);
		leftConnections--;
		foreach (LineRenderer renderer in lineRenderers) {
			if (renderer.gameObject.name.Contains (neighbour.GetInstanceID ().ToString ())) {
				//Change the render to display the multiple bond
				renderer.SetWidth (defaultBondWidth * (nrOfBonds + 1), defaultBondWidth * (nrOfBonds + 1));
			}
		}

	}

	/// <summary>
	/// Adds neighbour to this connectedElements-List but draws no line since the other neigbour did already.
	/// </summary>
	/// <param name="neighbour">Neighbour.</param>
	public void AddThisAsNeighbour (GameObject neighbour)
	{
		connectedElements.Add (neighbour);
		leftConnections--;
	}

	/// <summary>
	/// Removes neighbour from list and removes lineRenderer if available.
	/// </summary>
	/// <param name="neighbour">Neighbour.</param>
	public void RemoveThisNeighbour (GameObject neighbour)
	{
		//Neighbours can be multiple due to multiple bonds
		for (int i = connectedElements.Count-1; i >=0; i--) {
			if (connectedElements [i].GetInstanceID () == neighbour.GetInstanceID ()) {
				connectedElements.RemoveAt(i);
				leftConnections++;
			}
		}
		Debug.Log("RemoveCheck at " + gameObject.name+": Count is " + lineRenderers.Count);
		for(int i = lineRenderers.Count -1; i >= 0; i--){
			Debug.Log("RemoveCheck: " + lineRenderers[i].gameObject.name + " contains "+ neighbour.GetInstanceID().ToString() + ": " 
			          + lineRenderers[i].gameObject.name.Contains (neighbour.GetInstanceID ().ToString ()));
			if (lineRenderers[i].gameObject.name.Contains (neighbour.GetInstanceID ().ToString ())) {
				Debug.Log("Removing " + lineRenderers[i].name);
				GameObject.Destroy (lineRenderers[i].gameObject);
				lineDestinations.Remove(lineRenderers[i]);
				lineRenderers.Remove (lineRenderers[i]);
			}
		}
	}

	/// <summary>
	/// Resets all connectedElements.
	/// This should be called when an Element is dragged away from a Gridfield.
	/// </summary>
	public void ResetAllConnections ()
	{
		for (int i = 0; i < lineRenderers.Count; i++) {
			GameObject.Destroy (lineRenderers [i].gameObject);
		}
		lineRenderers.Clear ();
		lineDestinations.Clear();
		foreach (GameObject neighbour in connectedElements) {
			neighbour.GetComponent<GridfieldController> ().RemoveThisNeighbour (this.gameObject);
		}
		connectedElements.Clear ();
	}

	public List<GameObject> GetAllConnectedElements ()
	{
		return connectedElements;
	}

	public int GetAmountOfBondsWithNeighbour (GameObject _neighbour)
	{
		int amount = 0;
		foreach (GameObject neighbour in connectedElements) {
			if (neighbour.GetInstanceID () == _neighbour.GetInstanceID ())
				amount++;
		}
		return amount;
	}
}
