using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	

	}

	void FixedUpdate() {
		transform.RotateAround(transform.position, Vector3.forward, 0.5f);
	}
}
