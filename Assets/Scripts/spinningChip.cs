using UnityEngine;
using System.Collections;

public class spinningChip : MonoBehaviour {

	public Color color;
	public MeshRenderer mesh;

	public float alpha {
		get {
			return mesh.material.color.a;
		}
		set {
			Color temp = mesh.material.color;
			temp.a = value;
			mesh.material.color = temp;
		}
	}
	
	// Use this for initialization
	void Start () {
		mesh = GetComponent<MeshRenderer> ();
		color = mesh.material.color;
	}
	
	// Update is called once per frame
	void Update () {
		// Position
		transform.position = Camera.main.ViewportToWorldPoint (playerScript.instance.turnChipPoint);
		// Rotation
		transform.Rotate (playerScript.instance.turnChipRotation * Time.deltaTime);
	}
}
