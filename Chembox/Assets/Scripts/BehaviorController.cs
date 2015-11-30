using UnityEngine;
using System.Collections;

/// <summary>
/// Gets current gestures from GestureController and starts adequate actions.
/// </summary>
public class BehaviorController : MonoBehaviour {

	public Vector3 leftMenuPos;
	public Vector3 middleMenuPos;
	public Vector3 rightMenuPos;

	private bool isMenuSwitching = false;
	private Vector3 targetPos;
	private float switchSpeed = 8.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(isMenuSwitching){

			transform.position = new Vector3(Mathf.Lerp(transform.position.x, targetPos.x, switchSpeed * Time.deltaTime)
			                                 , transform.position.y
			                                 , transform.position.z);
			if(RoughlyEqual(transform.position.x, targetPos.x)){
				isMenuSwitching = false;
				transform.position = targetPos;
				targetPos = Vector3.zero;
			}
		}
	}

	public void switchToLeft(){
//		Debug.Log("SwitchToLeft called");
		if(transform.position != leftMenuPos){
			//Menu is not in switch-mode and we are not in the most left menu
//			Debug.Log("LeftSwitch is happening");
			if(transform.position == rightMenuPos || targetPos == rightMenuPos){
//				Debug.Log("CurrMenu: rightMenu");
				targetPos = middleMenuPos;
			}
			else if (transform.position == middleMenuPos || targetPos == middleMenuPos){
//				Debug.Log("CurrMenu: MiddleMenu");
				targetPos = leftMenuPos;
			}
			isMenuSwitching = true;
		}
	}

	public void switchToRight(){
//		Debug.Log("SwitchToRight called");
		if(transform.position != rightMenuPos){
			//Menu is not in switch-mode and we are not in the most right menu
			if(transform.position == leftMenuPos || targetPos == leftMenuPos){
				targetPos = middleMenuPos;
			}
			else if (transform.position == middleMenuPos || targetPos == middleMenuPos){
				targetPos = rightMenuPos;
			}
			isMenuSwitching = true;
		}
	}

	static bool RoughlyEqual(float a, float b) {
		float treshold = 0.01f; //how much roughly
		return (Mathf.Abs(a-b)< treshold);
	}
}
