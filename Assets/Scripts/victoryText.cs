using UnityEngine;
using System.Collections;

public class victoryText : MonoBehaviour {

	GUIText text;
	Color color = Color.white;
	public bool show = false;
	public float speed = 1f;

	// Use this for initialization
	void Start () {
		text = GetComponent<GUIText> ();
		color.a = 0f;
		text.color = color;
	}
	
	// Update is called once per frame
	void Update () {
		if (show && color.a != 1f) {
			color.a = Mathf.Clamp01(color.a + Time.deltaTime * speed);
			text.color = color;
		} else if (!show && color.a != 0f) {
			color.a = Mathf.Clamp01(color.a - Time.deltaTime * speed);
			text.color = color;
		}
	}

	public void setText( string new_text ) {
		text.text = new_text;
	}
}
