using UnityEngine;
using System.Collections;
using System;

public class BondingControlller : MonoBehaviour
{

	private bool bondModeActive = false;
	private GameObject activeGridfield;
	public GameObject bondModeBackground;
	private SpriteRenderer bondModeRenderer;
	private GameObject gamefield;
	private GameObject originGridfield;
	private GameObject originOverlayMask;
	private Vector3 v3 = Vector3.zero;
	private float dist;
	private bool isDrag = false;
	private bool interactionStarted = false;
	private Transform draggingElement;
	private Vector3 offset;
	private int timeTillLongClick = 5;
	private float timer;

	//Sprites for BondMode
	[SerializeField]
	private Sprite
		mainElementSprite;
	[SerializeField]
	private Sprite
		possibleConnElementSprite;
	[SerializeField]
	private Sprite
		impossibleConnElementSprite;
	[SerializeField]
	private Sprite
		activatedConnElementSprite;
	[SerializeField]
	private GameObject
		bondOverlayMask;
	private GameObject touchedOverlay;
	private Vector3 tempMousPos;

	// Use this for initialization
	void Start ()
	{
		gamefield = GameObject.FindGameObjectWithTag ("Gamefield");
		bondModeRenderer = bondModeBackground.GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update ()
	{

		if (bondModeActive) {
			//------------------Code for mobile phones and tablets------------------
			if (SystemInfo.deviceType == DeviceType.Handheld) {
				
				Touch touch = Input.touches [0];
				Vector3 touchPos = touch.position;
				
				if (touch.phase == TouchPhase.Began) {
					OnTouchDown (touchPos);
				}

				if (touch.phase == TouchPhase.Moved) {
					OnTouchContinued (touchPos);
				}

				if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) {
					OnTouchUp (touchPos);
				}
			}
			
			//------------------Code for Desktop or laptop computers (Unity Editor)------------------
			if (SystemInfo.deviceType == DeviceType.Desktop) {
				if (Input.GetMouseButtonDown (0)) {
					OnTouchDown (Input.mousePosition);
				}

				if (interactionStarted && Input.GetMouseButton (0)) {
						OnTouchContinued (Input.mousePosition);
				}

				if (interactionStarted && Input.GetMouseButtonUp (0)) {
					OnTouchUp (Input.mousePosition);
				}
			}
		}
	}

	void OnTouchDown (Vector3 pos)
	{
		timer = 0;
		Ray ray = Camera.main.ScreenPointToRay (pos);
		Collider2D[] colls = Physics2D.OverlapPointAll (ray.origin);
		if (colls != null) {
			//Case1: Click ActiveElement
			//Case2: Click Element with OverlayMask (not ActiveElement)

			//Check for click on overlayMask
			for (int i = 0; i < colls.Length; i++) {
				if (colls [i].tag.Equals ("BondModeOverlay") && colls [i].GetComponent<SpriteRenderer> ().sprite != null) {
					//Touch Down on Element with OverlayMask-Element - this can be the ActiveElement or a bondable neighbour of it
					//We need the origin gridfield the user clicked for future testing if user dragged the element away
					interactionStarted = true;
					originGridfield = colls [Array.FindIndex (colls, x => x.tag.Equals ("Gridfield"))].gameObject;
					//Get corresponding OverlayMask to hide it in case this element is dragged
					originOverlayMask = colls[i].gameObject;
					dist = originGridfield.GetComponent<GridfieldController>().GetElement().transform.position.z - Camera.main.transform.position.z;
					return;
				}
			}
			//If you got here, no BondableElement was clicked
			Debug.Log("No OverlayMask Found!");
			SetBondMode(false, null);
		}
	}

	void OnTouchContinued (Vector3 currPos)
	{
//		Debug.Log("Timer: " + timer);
		///////////////////////////////////////////////////////////////
		//For now LONGCLICK IS OUT!
//		if(timer > timeTillLongClick)
//		{
//			Ray ray = Camera.main.ScreenPointToRay (pos);
//			Collider2D[] colls = Physics2D.OverlapPointAll (ray.origin);
//			if (colls != null) {
//				for (int i = 0; i < colls.Length; i++) {
//					if (colls [i].tag.Equals ("BondModeOverlay") && colls[i].GetComponent<SpriteRenderer>().sprite != null) {
//						OnBondableLongClick (colls, colls[i].gameObject);
//					}
//				}
//			}
//		} else {
//			timer += timeTillLongClick * Time.deltaTime;
//		}
		///////////////////////////////////////////////////////////////
		
		//Case1: User is dragging an Element
		//Case1: User just wants to click an element and this part here is irrelevant

		v3 = new Vector3 (currPos.x, currPos.y, dist);
		v3 = Camera.main.ScreenToWorldPoint (v3);

		//Check if user dragged element out of Bounds of Gridfield
		if (!isDrag && !originGridfield.GetComponent<BoxCollider2D> ().bounds.Contains (new Vector3 (v3.x, v3.y, originGridfield.transform.position.z))) {
			//User dragged the Element too far away, its a drag, not a click!
			isDrag = true;
			//Hide OverlayMask (because it looks better)
			originOverlayMask.GetComponent<SpriteRenderer>().enabled = false;
			//Get the element for the user to drag
			draggingElement = originGridfield.GetComponent<GridfieldController> ().GetElement ().transform;
			offset = draggingElement.position - v3;
		} else if (isDrag) {
			//Just update Elements position during the drag
			draggingElement.position = v3 - offset;
		}
	}

	void OnTouchUp (Vector3 pos)
	{
//		Debug.Log("Timer: " + timer);

		//Case1: Drag of an Element has ended
		//Case2: User clicked on bondable Element to establish a bond
		//Case3: User clicked activeElement to exit bondMode
		if(isDrag){
			//Drag ended
			//Reset dragged Element back to its origin position
			draggingElement.position = new Vector3 (originGridfield.transform.position.x, originGridfield.transform.position.y, dist);
			//Unhide OverlayMask
			originOverlayMask.GetComponent<SpriteRenderer>().enabled = true;
			//Reset Variable
			isDrag = false;
		} else {
			//It was a click
			Ray ray = Camera.main.ScreenPointToRay (pos);
			Collider2D[] colls = Physics2D.OverlapPointAll (ray.origin);
			if (colls != null) {
				for (int i = 0; i < colls.Length; i++) {
					if (colls [i].tag.Equals ("BondModeOverlay") && colls [i].GetComponent<SpriteRenderer> ().sprite != null) {
						if(colls[i].gameObject.name.Contains("Center")){
							//User clicked activeElement to exit BondMode
							SetBondMode(false, null);
						} else {
							OnBondableTouched (colls, colls [i].gameObject);
						}
					}
				}
			}
		}
		interactionStarted = false;
	}

	public void SetBondMode (bool state, GameObject gridfield)
	{
//		Debug.Log("Bond Mode Set: " + state + " (Element: " + gridfield);
		bondModeActive = state;
		if (state) {
			bondModeRenderer.enabled = true;
		} else {
			bondModeRenderer.enabled = false;
		}

		if (state) {
			activeGridfield = gridfield;
			//Add overlays to gridfield and surrounding elements
			bondOverlayMask.transform.position = activeGridfield.transform.position;

			//Check BondModeMask-Objects for there appearance
			foreach (Transform child in bondOverlayMask.transform) {
				child.GetComponent<SpriteRenderer> ().enabled = true;
				UpdateOverlayObject (child.position);
			}
		} else {
			activeGridfield = null;
			//Disable all renderer from all BondModeMask-Objects except the CenterOverlay
			foreach (Transform child in bondOverlayMask.transform) {
				if (!child.name.Contains ("Center"))
					child.GetComponent<SpriteRenderer> ().sprite = null;
				;
				child.GetComponent<SpriteRenderer> ().enabled = false;
			}
		}
	}

	public void UpdateOverlayObject (Vector3 position)
	{
		Ray ray = new Ray (position, Vector3.forward);
		Collider2D[] colls = Physics2D.OverlapPointAll (ray.origin);
		GameObject overlayObject = colls[Array.FindIndex (colls, x => x.tag.Equals ("BondModeOverlay"))].gameObject;
		foreach (Collider2D coll in colls) {
			if (coll.gameObject.tag.Equals ("Gridfield")) {
				//Found a gridfield
				GameObject element = coll.GetComponent<GridfieldController> ().GetElement ();
				if (element != null) {
					//Found a gridfield with actual element
//					overlayObject.GetComponent<SpriteRenderer> ().enabled = true;
					if (activeGridfield.GetComponent<GridfieldController> ().GetAllConnectedElements ().Contains (coll.gameObject)) {
						//Element is already connected at least with single bond
						if (coll.GetComponent<GridfieldController> ().HasConnectionsLeft ()) {
							//Found a gridfield with already bonded element, further bonding is possible
//							Debug.Log("Element " + coll.name + " has connections left and is bonded.");
							overlayObject.GetComponent<SpriteRenderer> ().sprite = activatedConnElementSprite;
//							Debug.Log("Element " + element.GetComponent<ElementInfo>().GetName() + " is bonded, bonding not possible!");
						} else {
							//Found a gridfield with already bonded element, further bonding is not possible
							overlayObject.GetComponent<SpriteRenderer> ().sprite = impossibleConnElementSprite;
//							Debug.Log("Element " + element.GetComponent<ElementInfo>().GetName() + " is bonded, bonding possible!");
						}
					} else if (activeGridfield.GetInstanceID () == coll.gameObject.GetInstanceID ()) {
						//User clicked on active Gridfield for bonds
						//End bond mode

					} else {
						//Element is not connected
						if (coll.GetComponent<GridfieldController> ().HasConnectionsLeft ()) {
							//Found a gridfield with not bonded element, but bonding is possible
							overlayObject.GetComponent<SpriteRenderer> ().sprite = possibleConnElementSprite;
//							Debug.Log("Element " + element.GetComponent<ElementInfo>().GetName() + " is not bonded, bonding possible!");
						} else {
							//Found a gridfield with not bonded element, bonding is not possible
							overlayObject.GetComponent<SpriteRenderer> ().enabled = false;
//							Debug.Log("Element " + element.GetComponent<ElementInfo>().GetName() + " is not bonded, bonding not possible!");
						}
					}
				}
				//Gridfield was found, action was taken
				return;
			}
		}
		//No Gridfield was found
		overlayObject.GetComponent<SpriteRenderer> ().enabled = false;
	}

	private void UpdateBondOverlayMask (GameObject gridfield)
	{

	}

	public bool GetBondMode ()
	{
		return bondModeActive;
	}

	void OnBondableTouched (Collider2D[] colliders, GameObject overlayObject)
	{
		//Check if there is a gridfield at the overlays position
		for (int i = 0; i < colliders.Length; i++) {
			if (colliders [i].tag.Equals ("Gridfield")) {
				activeGridfield.GetComponent<GridfieldController> ().SetNewConnection (colliders [i].gameObject);
				UpdateOverlayObject (overlayObject.transform.position);
				gamefield.GetComponent<GamefieldController> ().updateGameInfo ();
			}
		}

	}

	void OnBondableLongClick (Collider2D[] colliders, GameObject overlayObject)
	{
		//Check if there is a gridfield at the overlays position
		for (int i = 0; i < colliders.Length; i++) {
			if (colliders [i].tag.Equals ("Gridfield")) {
				if (activeGridfield.GetComponent<GridfieldController> ().GetAmountOfBondsWithNeighbour (colliders [i].gameObject) > 0) {
					activeGridfield.GetComponent<GridfieldController> ().RemoveThisNeighbour (colliders [i].gameObject);
					colliders [i].gameObject.GetComponent<GridfieldController> ().RemoveThisNeighbour (activeGridfield);
					UpdateOverlayObject (overlayObject.transform.position);
					gamefield.GetComponent<GamefieldController> ().updateGameInfo ();
				}
			}
		}
	}
}
