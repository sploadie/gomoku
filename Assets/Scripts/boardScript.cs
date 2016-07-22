using UnityEngine;
using System.Collections;

public class boardScript : MonoBehaviour {

	public static boardScript instance;

	public GameObject blackChip;
	public GameObject whiteChip;
	public GameObject tempChip;

	public char[,] board = new char[19,19];

	void Awake () {
		if (!instance)
			instance = this;
		int i, j;
		for (i = 0; i < 19; ++i) {
			for (j = 0; j < 19; ++j) {
				board[i,j] = '0';
			}
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public GameObject genChip ( spaceScript space, char type ) {
		GameObject chip;
		switch (type) {
		case 'b':
			chip = GameObject.Instantiate (blackChip, Vector3.zero, Quaternion.identity) as GameObject;
			break;
		case 'w':
			chip = GameObject.Instantiate (whiteChip, Vector3.zero, Quaternion.identity) as GameObject;
			break;
		case 't':
			chip = GameObject.Instantiate (tempChip, Vector3.zero, Quaternion.identity) as GameObject;
			break;
		default:
			Debug.Log("Wtf man " + type + "isn't a type...");
			return null;
		}
		chip.transform.parent = space.transform;
		return chip;
	}

	public void debugBoard() {
		string output = "Board:\n";
		int i, j;
		for (i = 0; i < 19; ++i) {
			for (j = 0; j < 19; ++j) {
				output += board[i,j].ToString() + ", ";
			}
			output += "\n";
		}
		Debug.Log (output);
	}
}
