using UnityEngine;
using System.Collections;

public class Player : Object {

	public bool ai;
	public char color = '0';
	public int chipsCaptured = 0;
	public Player opponent;
	public Quaternion boardRotation = Quaternion.identity;

	public Player ( char _color, bool _ai ) {
		color = _color;
		ai = _ai;
	}

	void Start () {
	
	}
	
	void Update () {
	
	}

	public spaceScript.Position getMove(char[,] board) {
		spaceScript.Position move;
		do {
			move = new spaceScript.Position(Random.Range(0, 15), Random.Range(0, 15));
		} while (board [move.x, move.y] != '0');
		return move;
	}
}
