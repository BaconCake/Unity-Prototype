using UnityEngine;
using System.Collections;

/// <summary>
/// This Class holds Infos about this element.
/// </summary>
public class ElementInfo : MonoBehaviour {

	[SerializeField]
	private int nrOfPossibleBonds;

	[SerializeField]
	private string nameOfElement;

	[SerializeField]
	private string chemicalSymbol;

	[SerializeField]
	private Sprite dragSprite;
	[SerializeField]
	private Sprite defaultSprite;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public int GetNrOfBonds(){
		if(nrOfPossibleBonds == null)
			Debug.LogError("nrOfPossibleBonds not set on " + gameObject);
		return nrOfPossibleBonds;
	}

	public string GetName(){
		if(nameOfElement == null)
			Debug.LogError("nameOfElement not set on " + gameObject);
		return nameOfElement;
	}

	public string GetSymbol(){
		if(chemicalSymbol == null)
			Debug.LogError("chemicalSymbol not set on " + gameObject);
		return chemicalSymbol;
	}

	public Sprite GetDragSprite(){
		return dragSprite;
	}

	public Sprite GetDefaultSprite(){
		return defaultSprite;
	}
}
