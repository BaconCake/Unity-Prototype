using UnityEngine;
using System.Collections;

public class DragController : MonoBehaviour
{

	private float dist;
	public bool dragging = false;
	private Vector3 offset;
	private Transform draggingObject;
	private Vector3 elementOrigin;
	private bool elementFromPse = false;
	private Vector3 v3 = Vector3.zero;
	private GameObject gamefield;
	private BondingControlller bondController;
	private LineRenderer connLineRenderer;
	private GameObject originGridfield;
	private bool startetMoving = false;


	// Use this for initialization
	void Start ()
	{
		Debug.Log ("Current System: " + SystemInfo.deviceType);
		gamefield = GameObject.FindGameObjectWithTag ("Gamefield");

		bondController = GameObject.FindGameObjectWithTag ("Gamefield").GetComponent<BondingControlller> ();
	}
	
	// Update is called once per frame
	void Update ()
	{

		//------------------Code for mobile phones and tablets------------------
		if (SystemInfo.deviceType == DeviceType.Handheld) {
			if (Input.touchCount != 1) {
				dragging = false;
				return;
			}
			
			Touch touch = Input.touches [0];
			Vector3 touchPos = touch.position;
			
			
			if (touch.phase == TouchPhase.Began) {
				if (!bondController.GetBondMode ()) {
					dragStart (v3, touchPos);
				}
			}
			
			if (dragging && touch.phase == TouchPhase.Moved) {
				if (!bondController.GetBondMode ()) {
					dragContinued (v3, touchPos);
				}

			}
			
			if (dragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)) {
				if (!bondController.GetBondMode ()) {
					dragExited (touchPos);
				}
			}

		}

		//------------------Code for Desktop or laptop computers (Unity Editor)------------------
		if (SystemInfo.deviceType == DeviceType.Desktop) {
			if (Input.GetMouseButtonDown (0)) {
				if (!bondController.GetBondMode ()) {
				dragStart (v3, Input.mousePosition);
				}
			}
			if (dragging && Input.GetMouseButton (0)) {
				if (!bondController.GetBondMode ()) {
				dragContinued (v3, Input.mousePosition);
				}

			}
			if (dragging && Input.GetMouseButtonUp (0)) {
				if (!bondController.GetBondMode ()) {
				dragExited (Input.mousePosition);
				}
			}
		}
	}

	void dragStart (Vector3 v3, Vector3 currPos)
	{
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		Collider2D[] colls = Physics2D.OverlapPointAll (ray.origin);
		bool gridfieldFound = false;
		if (colls != null) {
			for (int i = 0; i < colls.Length; i++) {
				if (colls [i].tag == "Draggable") {
					//save original position in case, this element does not land on a gridfield
//					Debug.Log ("Collider found! (" + colls [i].transform.name + ")");
					draggingObject = colls [i].transform;
					draggingObject.gameObject.GetComponent<SpriteRenderer> ().sprite = colls [i].GetComponent<ElementInfo> ().GetDragSprite ();
					elementOrigin = draggingObject.position;
					dist = colls [i].transform.position.z - Camera.main.transform.position.z;
					v3 = new Vector3 (currPos.x, currPos.y, dist);
					v3 = Camera.main.ScreenToWorldPoint (v3);
					offset = draggingObject.position - v3;
					dragging = true;
				} else
					if (colls [i].tag == "Gridfield") {
					//Picking up an element from the gridfield
					gridfieldFound = true;
					originGridfield = colls [i].gameObject;
				}
			}
			if (dragging && !gridfieldFound) {
				//dragging an object, but no gridfield found -> means, we took an element from the PSE
				gameObject.GetComponent<BehaviorController> ().switchToRight ();
				elementFromPse = true;
			}
		}
	}

	void dragContinued (Vector3 v3, Vector3 currPos)
	{
		v3 = new Vector3 (currPos.x, currPos.y, dist);
		v3 = Camera.main.ScreenToWorldPoint (v3);
		//Only drag object if touch is out of range of objectsize or object came from PSE
		//This test allows to use click on Element

		if (!startetMoving && !elementFromPse) {
			if (!originGridfield.GetComponent<BoxCollider2D> ().bounds.Contains (new Vector3 (v3.x, v3.y, originGridfield.transform.position.z))) {
				startetMoving = true;
				originGridfield.GetComponent<GridfieldController> ().DeclareEmpty ();
				gamefield.GetComponent<GamefieldController> ().updateGameInfo ();
			}
		}
		if (startetMoving || elementFromPse) {
			draggingObject.position = v3 - offset;
		}
	}

	void dragExited (Vector3 currPos)
	{
		dragging = false;
		//Drag exited while bondMode is not active
		Ray ray = Camera.main.ScreenPointToRay (currPos);
		Collider2D[] colliders = Physics2D.OverlapPointAll (ray.origin);
		bool gridFound = false;
		for (int i = 0; i < colliders.Length; i++) {
			if (colliders [i].tag == "Gridfield") {
				if (colliders [i].GetComponent<GridfieldController> ().GetElement () == null) {
					//Empty Gridfield found, snap icon to gridfields position
					draggingObject.position = new Vector3 (colliders [i].transform.position.x, colliders [i].transform.position.y, dist);
					if (elementFromPse) {
						//Element directly from PSE, set to grid -> put new instance in PSE
						GameObject elementCopy = (GameObject)Instantiate (draggingObject.gameObject, elementOrigin, Quaternion.identity);
						elementCopy.GetComponent<SpriteRenderer> ().sprite = elementCopy.GetComponent<ElementInfo> ().GetDefaultSprite ();
					}
					colliders [i].gameObject.GetComponent<GridfieldController> ().SetElement (draggingObject.gameObject);
					gamefield.GetComponent<GamefieldController> ().updateGameInfo ();
					gridFound = true;
					break;
				} else if (!startetMoving && colliders [i].GetComponent<GridfieldController> ().GetElement ().GetInstanceID () == draggingObject.gameObject.GetInstanceID ()) {
					//Must be a click on one gridfield with element
					bondController.SetBondMode (true, originGridfield);
					gridFound = true;
				}
			}
		}
		//Whats happening, when no gridfield was found? (Destroy element icon? Put back on original pos?)
		if (!gridFound) {
			if (elementFromPse) {
				//Element directly from PSE, not set to grid -> put back in PSE
				draggingObject.position = elementOrigin;
				draggingObject.GetComponent<SpriteRenderer> ().sprite = draggingObject.GetComponent<ElementInfo> ().GetDefaultSprite ();
			} else {
				//Element from grid, not set to grid -> remove element
				GameObject.Destroy (draggingObject.gameObject);
			}
		}
		//Reset elementFromPse-variable
		elementFromPse = false;
		originGridfield = null;
		startetMoving = false;
	}

//	void connExited (Vector3 v3, Vector3 mousePosition)
//	{
//		dragging = false;
//		Ray ray = Camera.main.ScreenPointToRay (mousePosition);
//		Collider2D[] colliders = Physics2D.OverlapPointAll (ray.origin);
//		for (int i = 0; i < colliders.Length; i++) {
//			GameObject currObject = colliders [i].gameObject;
//			if (currObject.tag == "Gridfield" && currObject.GetComponent<GridfieldController> ().GetElement () != null) {
//				connLineRenderer.transform.parent.gameObject.GetComponent<GridfieldController> ().SetNewConnection (currObject);
//				//Update Gamefield Info
//				gamefield.GetComponent<GamefieldController> ().updateGameInfo ();
//			}
//		}
//		GameObject.Destroy (connLineRenderer.gameObject);
//	}
}
