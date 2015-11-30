using UnityEngine;
using System.Collections;

public class BtnValues : MonoBehaviour {
	
	private Sprite defaultSprite;
	[SerializeField]
	private Sprite clickSprite;
	[SerializeField]
	private Sprite activeSprite;
	private bool isActive = false;

	// Use this for initialization
	void Start () {
		//Set sprite at starttime as the defaultSprite
		defaultSprite = GetComponent<SpriteRenderer>().sprite;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTouchDown(){
		GetComponent<SpriteRenderer>().sprite = clickSprite;
	}

	/// <summary>
	/// Behavior when touch input moved away from this button.
	/// </summary>
	public void OutOfRange(){
		if(isActive){
			GetComponent<SpriteRenderer>().sprite = activeSprite;
		} else {
			GetComponent<SpriteRenderer>().sprite = defaultSprite;
		}
	}

	public void OnTouchUp(){
		if(isActive){
			GetComponent<SpriteRenderer>().sprite = defaultSprite;
		} else {
			GetComponent<SpriteRenderer>().sprite = activeSprite;
		}
		isActive = !isActive;

	}
}
