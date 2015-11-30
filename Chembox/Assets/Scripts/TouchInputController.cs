using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Handles all button clicks as well as clicks on to bond elements if bondMode is active.
/// </summary>
public class TouchInputController : MonoBehaviour {


	public bool bondModeActive = false;
	public GameObject bondModeBackground;

	private GameObject activeBtn;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		//------------------Code for mobile phones and tablets------------------
		if (SystemInfo.deviceType == DeviceType.Handheld) {

			Touch touch = Input.touches [0];
			Vector3 touchPos = touch.position;

			if (touch.phase == TouchPhase.Began) {
				OnTouchDown(touchPos);
			}
			
			if (activeBtn != null && touch.phase == TouchPhase.Moved) {
				OnTouchContinued (touchPos);
			}
			
			if (activeBtn != null && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)) {
				OnTouchUp ();
			}
		}

		//------------------Code for Desktop or laptop computers (Unity Editor)------------------
		if (SystemInfo.deviceType == DeviceType.Desktop) {
			if (Input.GetMouseButtonDown (0)) {
				OnTouchDown(Input.mousePosition);
			}
			if (activeBtn != null  && Input.GetMouseButton (0)) {
				OnTouchContinued (Input.mousePosition);
				
			}
			if (activeBtn != null  && Input.GetMouseButtonUp (0)) {
				OnTouchUp ();
			}
		}
	}

	void OnTouchDown (Vector3 pos)
	{
//		Debug.Log("OnTouchDown called!");
		Ray ray = Camera.main.ScreenPointToRay (pos);
		Collider2D[] colls = Physics2D.OverlapPointAll (ray.origin);
		if (colls != null) {
			for (int i = 0; i < colls.Length; i++) {
				if (colls [i].tag.Equals ("Button")) {
					activeBtn = colls [i].gameObject;
					activeBtn.GetComponent<BtnValues> ().OnTouchDown ();
				}
			}
		}
	}

	void OnTouchContinued (Vector3 pos)
	{
		Ray ray = Camera.main.ScreenPointToRay (pos);
		Collider2D[] colls = Physics2D.OverlapPointAll (ray.origin);
		if (Array.Find (colls, c => c.gameObject.GetInstanceID().Equals(activeBtn.GetInstanceID())) == null) {
			activeBtn.GetComponent<BtnValues> ().OutOfRange ();
			activeBtn = null;
		}
	}

	void OnTouchUp ()
	{
		activeBtn.GetComponent<BtnValues> ().OnTouchUp ();
		//Get pushed Button and set its behavior
		switch(activeBtn.name){
		case "ConnectionBtn":
			ConnBtnPressed();
			break;
		case "ArrowLeft":
			gameObject.GetComponent<BehaviorController>().switchToLeft();
			break;
		case "ArrowRight":
			gameObject.GetComponent<BehaviorController>().switchToRight();
			break;
		case "Frame":
			//InfoBoxFrame
			GameObject.FindGameObjectWithTag("Gamefield").transform.FindChild("InfoBoxWrapper").gameObject.SetActive(false);
			break;
		}
		activeBtn = null;
	}

	void ConnBtnPressed ()
	{
		bondModeActive = !bondModeActive;
//		GameObject.FindGameObjectWithTag("Gamefield").GetComponent<BondingControlller>().SetBondMode(bondModeActive);
		SpriteRenderer bondModeRenderer = bondModeBackground.GetComponent<SpriteRenderer>();
		bondModeRenderer.enabled = !bondModeRenderer.enabled;
	}
}
