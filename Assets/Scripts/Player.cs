using UnityEngine;
using System.Collections;

public class Player : Object {

	public bool ai = false;
	public char color = '0';
	public int chipsCaptured = 0;
	public Player opponent;
	public Quaternion boardRotation = Quaternion.identity;

	public Player ( char _color ) {
		color = _color;
	}

	void Start () {
	
	}
	
	void Update () {
	
	}
}
