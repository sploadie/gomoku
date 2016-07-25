using UnityEngine;
using System.Collections;
using System;

public class boardScript : MonoBehaviour {

	public static boardScript instance;

	public GameObject blackChip;
	public GameObject whiteChip;
	public GameObject tempChip;

	public spaceScript[,] board = new spaceScript[15,15];

	void Awake () {
		if (!instance)
			instance = this;
//		int i, j;
//		for (i = 0; i < 19; ++i) {
//			for (j = 0; j < 19; ++j) {
//				board[i,j] = '0';
//			}
//		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool canPlace ( char type, spaceScript.Position pos) {
		bool freeThree = false;
		char offense = type;
		char defense = ( offense == 'w' ? 'b' : 'w' );
		int i, j, k, l, count, empty;
		char tempChip;
		for (i = -1; i < 1; ++i) {
			for (j = -1; j < 2; ++j) {
				if (!(i == 0 && j ==0) && !(i == 0 && j == -1)) {
					try {
						for (k = -4; k < 1; k++) {
							try {
								count = 0;
								empty = 0;
								for (l = 0; l < 5; l++) {
									if (k + l == 0) {
										count++;
									} else {
										tempChip = board [pos.x + i*(k+l), pos.y + j*(k+l)].chip;
										if (tempChip == offense) {
											count++;
										} else if (tempChip == defense) {
											count = 0;
											break;
										} else {
											empty++;
										}
									}
								}
								if (count > 1)
									Debug.Log ("Count:" + count + " Empty:" + empty);
								if (count == 3 && empty >= 2) {
									Debug.Log ("Free three found!");
									if (freeThree)
										return false;
									freeThree = true;
									break;
								}
							} catch(System.IndexOutOfRangeException) {}
						}
					} catch(System.IndexOutOfRangeException) {}
				}
			}
		}
		return true;
	}

	public GameObject placeChip ( spaceScript space ) {
		GameObject chip;
		switch (space.chip) {
		case 'b':
			chip = GameObject.Instantiate (blackChip, Vector3.zero, Quaternion.identity) as GameObject;
			break;
		case 'w':
			chip = GameObject.Instantiate (whiteChip, Vector3.zero, Quaternion.identity) as GameObject;
			break;
		case 't':
			chip = GameObject.Instantiate (tempChip, Vector3.zero, Quaternion.identity) as GameObject;
			break;
		case 'f':
			chip = null;
			break;
		case '0':
			chip = null;
			break;
		default:
			Debug.Log("Wtf man " + space.chip + "isn't a type...");
			return null;
		}
		if (space.chip == 'w' || space.chip == 'b')
			handleCapture (space.position);
		return chip;
	}

	public void handleCapture ( spaceScript.Position pos ) {
		char offense = board [pos.x, pos.y].chip;
		char defense = ( offense == 'w' ? 'b' : 'w' );
		int i, j;
		for (i = -1; i < 2; ++i) {
			for (j = -1; j < 2; ++j) {
				try {
					if ( board [pos.x + i, pos.y + j].chip == defense
					    && board [pos.x + i*2, pos.y + j*2].chip == defense
					    && board [pos.x + i*3, pos.y + j*3].chip == offense) {
						// Capture occured
						playerScript.instance.chipsCaptured(offense);
						board [pos.x + i, pos.y + j].setChip('0');
						board [pos.x + i*2, pos.y + j*2].setChip('0');
					}
				} catch(System.IndexOutOfRangeException) {}
			}
		}
	}

	public void debugBoard() {
		string output = "Board:\n";
		int i, j;
		for (i = 0; i < 15; ++i) {
			for (j = 0; j < 15; ++j) {
				if (board[i,j] != null) {
					output += board[i,j].chip.ToString() + ", ";
				} else {
					output += "X" + ", ";
//					Debug.Log ("Chip at [" + i + ", " + j + "] not ready!");
				}
			}
			output += "\n";
		}
		Debug.Log (output);
	}
}
