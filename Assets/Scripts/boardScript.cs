using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class boardScript : MonoBehaviour {

	public static boardScript instance;

	public GameObject blackChip;
	public GameObject whiteChip;
	public GameObject tempChip;
	public GameObject errorChip;

	public bool showAI = true;
	public GameObject AITextContainer;
	public TextMesh AIText;

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

	public char[,] getSample() {
		char[,] sample = new char[15,15];
		int i, j;
		for (i = 0; i < 15; ++i) {
			for (j = 0; j < 15; ++j) {
				sample[i,j] = board[i,j].chip;
			}
		}
		return sample;
	}
	
	public bool canPlace ( char type, spaceScript.Position pos) {
		bool freeThree = false;
		char offense = type;
		int i, j, k, l;
		bool empty;
		char tempChip;
		for (i = -1; i < 1; ++i) {
			for (j = -1; j < 2; ++j) {
				if (!(i == 0 && j ==0) && !(i == 0 && j == -1)) {
					for (k = -4; k < 1; k++) {
						try {
							// FREE THREE VERIFICATION
							empty = false;
							l = 0; // FIRST
							tempChip = board [pos.x + i*(k+l), pos.y + j*(k+l)].chip;
							if (tempChip != '0')
								continue;
							l = 1; // SECOND
							tempChip = board [pos.x + i*(k+l), pos.y + j*(k+l)].chip;
							if (tempChip != offense)
								continue;
							l = 2; // THIRD
							tempChip = board [pos.x + i*(k+l), pos.y + j*(k+l)].chip;
							if (tempChip != offense && tempChip != '0')
								continue;
							if (tempChip == '0')
								empty = true;
							l = 3; // FOURTH
							tempChip = board [pos.x + i*(k+l), pos.y + j*(k+l)].chip;
							if (tempChip != offense && tempChip != '0')
								continue;
							if (tempChip == '0') {
								if (empty == true)
									continue;
								else
									empty = true;
							}
							l = 4; // FIFTH
							tempChip = board [pos.x + i*(k+l), pos.y + j*(k+l)].chip;
							if (empty == true && tempChip != offense)
								continue;
							if (empty == false && tempChip != '0')
								continue;
							l = 5; // SIXTH
							if (empty == true && board [pos.x + i*(k+l), pos.y + j*(k+l)].chip != '0')
								continue;
							// FREE THREE VERIFICATION END
							// FREE THREE DEBUG
							string output = "Free three found: ";
							try {
								for (l = 0; l < 5; l++) {
									if (k+l != 0)
										output += board [pos.x + i*(k+l), pos.y + j*(k+l)].chip.ToString();
									else
										output += offense.ToString();
								}
								if (empty == true)
									output += board [pos.x + i*(k+l), pos.y + j*(k+l)].chip.ToString();
							} catch(System.IndexOutOfRangeException) {}
							Debug.Log (output);
							// FREE THREE DEBUG END
							// FREE THREE CONFIRMED
							if (freeThree) {
								Debug.Log ("Double free three found!");
								return false;
							}
							freeThree = true;
							break;
						} catch(System.IndexOutOfRangeException) {}
					}
				}
			}
		}
		return true;
	}

	public bool isWin ( char type, spaceScript.Position pos) {
		int i, j, k, l;
		bool win;
		for (i = -1; i < 1; ++i) {
			for (j = -1; j < 2; ++j) {
				if (!(i == 0 && j ==0) && !(i == 0 && j == -1)) {
					for (k = -4; k < 1; k++) {
						try {
							win = true;
							for (l = 0; l < 5; l++) {
								if (!(board [pos.x + i*(k+l), pos.y + j*(k+l)].chip == type)) {
									win = false;
									break;
								}
							}
							if (win) {
								return true;
							}
						} catch(System.IndexOutOfRangeException) {}
					}
				}
			}
		}
		return false;
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
			chip = GameObject.Instantiate (errorChip, Vector3.zero, Quaternion.identity) as GameObject;
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

	public void newAIText(spaceScript.Position pos, string content) {
		if (!showAI)
			return;
		TextMesh newText = GameObject.Instantiate (AIText, Vector3.zero, AIText.transform.rotation) as TextMesh;
		newText.text = content;
		newText.transform.parent = AITextContainer.transform;
		newText.transform.localPosition = new Vector3 ((float)(pos.x - 7) * 2f, 0f, (float)(pos.y - 7) * 2f);
	}

	public void clearAIText() {
		Destroy (AITextContainer);
		AITextContainer = new GameObject ();
		AITextContainer.transform.position = Vector3.up;
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
