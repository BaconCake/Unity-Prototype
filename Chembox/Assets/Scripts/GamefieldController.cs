using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Script for the wrapper of all Gridfields.
/// Contains methods to collect all Chemical Elements used on grid and its interpretation (checks for completed molecules).
/// </summary>
using System.Linq;
using System.IO;
using System.Text;


public class GamefieldController : MonoBehaviour
{

	private List<string> usedElements = new List<string> ();
	private string formula;
	private TextMesh formulaText;
	private SpriteRenderer formulaTextEffect;
	private string defaultText;
	private bool isMoleculeComplete = true;
	private Dictionary<GameObject, List<string>> elementNodes = new Dictionary<GameObject, List<string>> ();
	private string graphDescription;
	public  List<List<GameObject>> elementGroups = new List<List<GameObject>> ();
	SqliteDatabase sqlDB;

	public GameObject moleculeInfoBox;

	void Awake ()
	{
		//Database implementation
		string dbPath = System.IO.Path.Combine (Application.persistentDataPath, "game.db");
		string dbTemplatePath = System.IO.Path.Combine (Application.streamingAssetsPath, "Chembox.db");
		
//		if(!System.IO.File.Exists(dbPath)){
		//game database does not exist, copy default db as template
		if (Application.platform == RuntimePlatform.Android) {
			//Must use WWW for streaming asset
			WWW reader = new WWW (dbTemplatePath);
			while (!reader.isDone) {

			}
			System.IO.File.WriteAllBytes (dbPath, reader.bytes);
		} else {
			System.IO.File.Copy(dbTemplatePath, dbPath, true);
		}
		sqlDB = new SqliteDatabase (dbPath);

//		DataTable result = GetDatabase();
//		if(Convert.ToBoolean(result.Rows[0]["Unlocked"]))
//		{
//			Debug.Log(result.Rows[0]["Name"] + " is unlocked.");
//		} else
//			Debug.Log(result.Rows[0]["Name"] + " is not unlocked.");


	}

	// Use this for initialization
	void Start ()
	{
		formulaText = transform.GetComponentInChildren<TextMesh> ();
		formulaTextEffect = formulaText.gameObject.GetComponentInChildren<SpriteRenderer> ();
		defaultText = formulaText.text;
	}
	
	// Update is called once per frame
	void Update ()
	{

	}

	/// <summary>
	/// Returns all data from the database (Chembox.db)
	/// </summary>
	/// <returns>The database.</returns>
	public DataTable GetDatabase(){
		DataTable result = sqlDB.ExecuteQuery ("SELECT * FROM Molecules");
		if(result.Rows.Count < 1)
			Debug.LogError("Database returned empty DataTable.");
		if(result == null)
			Debug.LogError("Database returned null.");
		return result;
	}

	/// <summary>
	/// Gets called when an element was dropped.
	/// Calls helper methods to check for completed molecule.
	/// </summary>
	public void updateGameInfo ()
	{
		getElementGroups ();
		checkForCompleteMolecules ();
		formulaText.text = formula;
		//Reset Formula
		formula = "";
//		Debug.Log ("Molecule Complete: " + isMoleculeComplete);
		if (formulaText.text == "")
				formulaText.text = defaultText;
		updateResultPanel (isMoleculeComplete);
	}

	/// <summary>
	/// Fills elementGroups-List with all currently used elements seperated in groups based on their bonds.
	/// </summary>
	void getElementGroups ()
	{
		bool elementAlreadyKnown;
		elementGroups.Clear ();
		foreach (Transform t in transform) {
			GameObject child = t.gameObject;
			if (child.tag.Equals ("Gridfield") &&
				child.GetComponent<GridfieldController> ().element != null) {
				GetElementInfo (child, elementGroups, out elementAlreadyKnown);
				if (elementAlreadyKnown) {
					//Current Element is part of some list already. Ignore.
				} else {
					//Current element is not in one of elementGroups-List's Lists. Generate new list with this element and all its bonded neighbours.
					List<GameObject> newElementGroup = new List<GameObject> ();
					//Find all bonded elements and add them using DFS
					depthFirstSearch (child, newElementGroup);
					elementGroups.Add (newElementGroup);
				}
			}
		}

		//Debug Output
//		Debug.Log ("Output Element Groups:");
//		int index = 0;
//		foreach (List<GameObject> group in elementGroups) {
//			Debug.Log ("Group " + index);
//			foreach (GameObject element in group) {
//				Debug.Log (element.GetComponent<GridfieldController> ().element.GetComponent<ElementInfo> ().GetSymbol ());
//			}
//			Debug.Log ("______");
//			index++;
//		}

	}

	void depthFirstSearch (GameObject currNode, List<GameObject> currGroup)
	{
		currGroup.Add (currNode);
		foreach (GameObject child in currNode.GetComponent<GridfieldController>().GetAllConnectedElements()) {
			if (!currGroup.Contains (child)) {
				depthFirstSearch (child, currGroup);
			}
		}
	}

	/// <summary>
	/// Gets infos about wether this element is already part of a group and the index of this group.
	/// </summary>
	/// <param name="element">Element.</param>
	/// <param name="currInstanceOfElementGroups">Curr instance of element groups.</param>
	/// <param name="returnBool">Return bool.</param>
	/// <param name="returnIndex">Return index.</param>
	void GetElementInfo (GameObject element, List<List<GameObject>> currInstanceOfElementGroups, out bool returnBool)
	{
		bool isPartOf = false;
		foreach (List<GameObject> group in currInstanceOfElementGroups) {
			if (group.Contains (element)) {
				isPartOf = true;
			}
		}
		returnBool = isPartOf;
	}

	void checkForCompleteMolecules ()
	{
		if (elementGroups.Count == 0) {
			//No elements on screen. No need for a molecule check
			isMoleculeComplete = false;
			return;
		}
		foreach (List<GameObject> elementGroup in elementGroups) {
			isMoleculeComplete = true;
			foreach (GameObject element in elementGroup) {
				if (element.GetComponent<GridfieldController> ().HasConnectionsLeft ()) {
					isMoleculeComplete = false;
					break;
				}
			}
			if (isMoleculeComplete) {
				//Found a complete Molecule. Stop the check.
				makeMolecularFormula (elementGroup);
				makeGraphDescription (elementGroup);
				checkDatabase();
				break;
			}
		}
	}

	void makeMolecularFormula (List<GameObject> completedMolecule)
	{
		usedElements.Clear ();
		foreach (GameObject element in completedMolecule) {
			usedElements.Add (element.GetComponent<GridfieldController> ().element.GetComponent<ElementInfo> ().GetSymbol ());
		}
		formula = "";
		int nrOfC = 0;
		int nrOfH = 0;
		string otherAtoms = "";
		usedElements.Sort ();
		int elementCount = 1;
		for (int i = 0; i < usedElements.Count; i++) {
			if (usedElements [i].Equals ("C")) {
				nrOfC++;
			} else if (usedElements [i].Equals ("H")) {
				nrOfH++;
			} else {
				if (!otherAtoms.Contains (usedElements [i])) {
					if (elementCount > 1) {
						otherAtoms += elementCount.ToString ();
						elementCount = 1;
					}
					otherAtoms += usedElements [i];
				} else {
					elementCount++;
				}
			}
		}
		if (elementCount > 1)
			otherAtoms += elementCount.ToString ();
		//Hill-System
		if (nrOfC > 0) {
			formula += "C";
			if (nrOfC > 1)
				formula += nrOfC.ToString ();
		}
		if (nrOfH > 0) {
			formula += "H";
			if (nrOfH > 1)
				formula += nrOfH.ToString ();
		}
		formula += otherAtoms;

	}

	void makeGraphDescription (List<GameObject> completedMolecule)
	{
		graphDescription = "";
		//Fetching all elements and its neighbours
		elementNodes.Clear ();
		foreach (GameObject child in completedMolecule) {
			GameObject element = child.GetComponent<GridfieldController> ().GetElement ();
			List<string> neigbours = new List<string> ();
			foreach (GameObject connectedElement in child.GetComponent<GridfieldController>().GetAllConnectedElements()) {
				neigbours.Add (connectedElement.GetComponent<GridfieldController> ().GetElement ().GetComponent<ElementInfo> ().GetSymbol ());
			}
			//sort neighbours by alphabet
			neigbours.Sort ();
			elementNodes.Add (element, neigbours);
		}
		//Sort elementNodes-Dictionary by key
		Dictionary<GameObject, string> sortedElementNodes = new Dictionary<GameObject, string> ();
		List<GameObject> sortedList = elementNodes.Keys.ToList ();
		sortedList = sortedList.OrderBy (g => g.GetComponent<ElementInfo> ().GetSymbol ()).ToList ();
		for (int i = 0; i < sortedList.Count; i++) {
			sortedElementNodes.Add (sortedList [i], string.Join ("", elementNodes [sortedList [i]].ToArray ()));
		}

		//Sort elements by its values
		List<KeyValuePair<GameObject, string>> sorted = (from kv in sortedElementNodes orderby kv.Value select kv).ToList ();

		//put together the graphDescription
//		foreach(var pair in sortedElementNodes){
		foreach (KeyValuePair<GameObject, string> kvPair in sorted) {
			graphDescription += kvPair.Key.GetComponent<ElementInfo> ().GetSymbol () + kvPair.Value;
		}


		//Debug output
		Debug.Log ("GraphDescription: " + graphDescription);
	}


	void checkDatabase ()
	{
		bool dbEntryFound = false;
		DataTable result = sqlDB.ExecuteQuery ("SELECT * FROM Molecules WHERE Formula = '" + formula +"'");
		if(result.Rows.Count > 1){
			//More than one molecule with this formula, use uniqueCode
			for(int i = 0; i < result.Rows.Count; i++) {
				if(graphDescription.Equals ((string)result.Rows[i]["UniqueCode"])){
					string moleName = (string)result.Rows [i] ["Name"];
					Debug.Log ("Glückwunsch! Du hast " + moleName + " geschaffen!");
					formulaText.text = moleName + "\n" + formula;
					dbEntryFound = true;
					updateAndShowInfoBox(result.Rows[i]);
				}
			}
		} else if(result.Rows.Count == 1){
			//Exactly one result, we found our molecule
			string moleName = (string)result.Rows [0] ["Name"];
			Debug.Log ("Glückwunsch! Du hast " + moleName + " geschaffen!");
			formulaText.text = moleName + "\n" + formula;
			dbEntryFound = true;
			updateAndShowInfoBox(result.Rows[0]);
		} else {
			//Molecule not found in Database
			Debug.Log("Molecule not in Database!");
			updateAndShowInfoBox(null);
		}
	}

	void updateResultPanel (bool setState)
	{
		if (setState) {
			formulaTextEffect.GetComponent<AudioSource> ().Play ();
		}
		formulaTextEffect.enabled = setState;
		moleculeInfoBox.SetActive(setState);
	}

	void updateAndShowInfoBox(DataRow info){
			foreach(Transform child in moleculeInfoBox.transform){
				switch(child.gameObject.name){
				case "Image":
				if(info!=null){
					child.GetComponent<SpriteRenderer>().sprite = Resources.Load <Sprite> ("Molecules/" + info["Name"]);
				} else {
					child.GetComponent<SpriteRenderer>().sprite = null;
				}
					break;
				case "Canvas":
				if(info != null){
					string testValue = (string)info["FunFact"];
					Debug.Log("InfoBox:\nText from Database: " + testValue);
					child.FindChild("InfoText").GetComponent<Text>().text = (string)info["FunFact"];
//					string testText = "Dies ist nur ein Test Text der meeeeega lang ist und Umlaute enthält! ÄÜÖ.";
//					Debug.Log("Test Text: " + testText);
//					child.FindChild("InfoText").GetComponent<Text>().text = testText;
					child.FindChild("InfoText").GetComponent<Text>().SetAllDirty();
					child.FindChild("MoleculeName").GetComponent<Text>().text = (string)info["Name"];
				} else {
//					string testValue = (string)info["FunFact"];
//					Debug.Log("InfoBox:\nText from Database: " + testValue);
					child.FindChild("InfoText").GetComponent<Text>().text = "You discovered a new Molecule!";
					child.FindChild("InfoText").GetComponent<Text>().SetAllDirty();
					child.FindChild("MoleculeName").GetComponent<Text>().text = "New Molecule";
				}
					break;
				}
			}

	}
}
