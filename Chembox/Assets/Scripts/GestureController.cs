using UnityEngine;
using System.Collections;

/// <summary>
/// This Class interpretes user input and sends interpreted gestures to the behavior controller.
/// </summary>
public class GestureController : MonoBehaviour {

	private float mouseDownPosX;
	private float mouseUpPosX;
	
	void Start () {

	
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown("escape")){
			//Only works in build version, not in editor
			Application.Quit();
		}
	
		if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved){
			Vector2 touchDelta = Input.GetTouch(0).deltaPosition;
			if (touchDelta.x < -50){
				//Flick to the left
				if(!IsDragging())
				gameObject.GetComponent<BehaviorController>().switchToRight();


			} else if (touchDelta.x > 50){
				//Flick to the right
				if(!IsDragging())
				gameObject.GetComponent<BehaviorController>().switchToLeft();

			}
		}

		if(Input.GetMouseButtonDown(0)){
			mouseDownPosX = Camera.main.ScreenToViewportPoint(Input.mousePosition).x;
//			Debug.Log("MouseDown at " + mouseDownPosX);
		}

		if(Input.GetMouseButtonUp(0)){
			mouseUpPosX = Camera.main.ScreenToViewportPoint(Input.mousePosition).x;
//			Debug.Log("MouseUp at " + mouseUpPosX);
			float mousePosDelta = mouseDownPosX - mouseUpPosX;
//			Debug.Log ("MouseDelta is " + mousePosDelta);
			if(mousePosDelta > 0.2f){
				//Mouse swipe to the right
				if(!IsDragging())
				gameObject.GetComponent<BehaviorController>().switchToRight();
			}
			else if(mousePosDelta < -0.2f){
				//Mouse swipe to the left
				if(!IsDragging())
				gameObject.GetComponent<BehaviorController>().switchToLeft();
			}
		}

	}

	private bool IsDragging(){
		return gameObject.GetComponent<DragController>().dragging;
	}
}
