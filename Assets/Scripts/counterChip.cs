using UnityEngine;
using System.Collections;

public class counterChip : MonoBehaviour {

	public Vector3 position = Vector3.zero;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		// Position
		transform.position = Camera.main.ViewportToWorldPoint (position);
		transform.LookAt (Camera.main.transform.position);
	}
}
