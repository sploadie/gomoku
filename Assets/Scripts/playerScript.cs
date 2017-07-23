using UnityEngine;
using System.Collections;

public class playerScript : MonoBehaviour {

	public static playerScript instance;

	public GameObject cameraPivot;

	// Turn UI
	public spinningChip whiteTurnChip;
	public spinningChip blackTurnChip;
	public Vector3 turnChipRotation = new Vector3 (30f, 30f, 30f);
	public Vector3 turnChipPoint;

	// Mouse hover UI
	spaceScript hoverSpace = null;

	// Player variables
	public char whichTurn { get; private set; }
	public long pings;
	public long final_pings;
	private Player whitePlayer = new Player('w', false);
	public GUIText whiteCaptured;
	private Player blackPlayer = new Player('b', Menu.ai_mode);
	public GUIText blackCaptured;
	public victoryText victoryText;
	
	// Board rotation animation variables
	public bool do_board_reset = false;
	bool resetting_board = false;
	Vector3 reset_angles_start;
	float reset_ratio;
	float reset_time;

	bool changing_player = false;
	float change_velocity = 0f;
	public float change_time = 1f;
	public bool gameOver = false;

	// Last Turn Handling
	char lastTurn = '0';
	spaceScript.Position lastTurnPos;

	void Awake () {
		Player.Awake ();

		if (!instance)
			instance = this;
		whichTurn = Random.value > 0.5f ? 'w' : 'b';
		if (!currentPlayer().ai)
			changing_player = true;
		change_velocity = 0f;
		reset_time = 0f;
		whitePlayer.opponent = blackPlayer;
		whitePlayer.boardRotation = Quaternion.Euler (0f, 0f, 0f);
		blackPlayer.opponent = whitePlayer;
		blackPlayer.boardRotation = Quaternion.Euler (0f, 180f, 0f);
	}

	void Start () {
		whiteTurnChip.alpha = 0f;
		blackTurnChip.alpha = 0f;
	}
	
	// Update is called once per frame
	void Update () {
		// Randomly spin turn chip
		turnChipRotation.z = Mathf.Clamp (turnChipRotation.z + (Random.value * 2f) - 1f, -30f, 30f);
		// Handle turn switch
		if (changing_player) {
			reset_time = Mathf.SmoothDamp(reset_time, 1.1f, ref change_velocity, change_time);
			if (reset_time > 1f)
				reset_time = 1f;
			if (whichTurn == 'w') {
				cameraPivot.transform.rotation = Quaternion.Slerp(blackPlayer.boardRotation, whitePlayer.boardRotation, reset_time);
				whiteTurnChip.alpha = Mathf.Clamp01((reset_time * 2f) - 1f);
				blackTurnChip.alpha = Mathf.Clamp01(1f - (reset_time * 2f));
			} else {
				cameraPivot.transform.rotation = Quaternion.Slerp(whitePlayer.boardRotation, blackPlayer.boardRotation, reset_time);
				blackTurnChip.alpha = Mathf.Clamp01((reset_time * 2f) - 1f);
				whiteTurnChip.alpha = Mathf.Clamp01(1f - (reset_time * 2f));
			}
			if (reset_time >= 1f) {
				changing_player = false;
				if (whichTurn == 'w') {
					whiteTurnChip.alpha = 1f;
					blackTurnChip.alpha = 0f;
				} else {
					blackTurnChip.alpha = 1f;
					whiteTurnChip.alpha = 0f;
				}
			}
		// Board reset input
		} else if (Input.GetKey (KeyCode.R) || do_board_reset) {
			do_board_reset = false;
			if (cameraPivot.transform.eulerAngles == Vector3.zero)
				cameraPivot.transform.eulerAngles = Vector3.one * 359.9f;
			hoverStop();
			resetting_board = true;
			reset_angles_start = cameraPivot.transform.eulerAngles;
			reset_ratio = (reset_angles_start.magnitude / 625f) * 3f;
			reset_time = 0f;
		// Handle board reset
		} else if (resetting_board) {
			reset_time += Time.deltaTime / reset_ratio;
			cameraPivot.transform.eulerAngles = Vector3.Slerp (reset_angles_start, Vector3.zero, reset_time);
			if (reset_time > 1f) {
				resetting_board = false;
			}
		// Handle manual board rotation
		} else if (Input.GetKey (KeyCode.LeftControl)) {
			hoverStop();
			if (Input.GetMouseButton (0)) {
				cameraPivot.transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * 2f);
				cameraPivot.transform.Rotate(Vector3.forward * Input.GetAxis("Mouse Y") * 2f);
			}
		// Handle zoom
		} else if (Input.GetAxis("Vertical") != 0) {
			float scale = 1f - (Input.GetAxis("Vertical") / 10f);
			if (Input.GetAxis("Vertical") > 0 && cameraPivot.transform.localScale.magnitude > 0.2f)
				cameraPivot.transform.localScale = cameraPivot.transform.localScale * scale;
			if (Input.GetAxis("Vertical") < 0 && cameraPivot.transform.localScale.magnitude <= 1.5f)
				cameraPivot.transform.localScale = cameraPivot.transform.localScale * scale;
		// Handle Game Over
		} else if (gameOver) {
			// FIXME
		// Handle last turn
		} else if (lastTurn == whichTurn) {
			if (boardScript.instance.isWin(lastTurn, lastTurnPos)) {
				win(lastTurn);
			} else {
				lastTurn = '0';
				victoryText.show = false;
			}
		// Handle AI if not Game Over
		} else if (currentPlayer().ai == true) {
			// Debug.Log ("AI?");
			spaceScript.Position aiPos = currentPlayer().getMoveFromC(boardScript.instance.getSample());
			boardScript.instance.board [aiPos.x, aiPos.y].setChip(whichTurn);
			if (lastTurn == '0' && boardScript.instance.isWin(whichTurn, aiPos)) {
				lastTurn = whichTurn;
				lastTurnPos = aiPos;
				victoryText.setText ("Last Turn?");
				victoryText.show = true;
			}
			if (whichTurn == 'w') {
				whiteTurnChip.alpha = 0f;
				blackTurnChip.alpha = 1f;
			} else {
				whiteTurnChip.alpha = 1f;
				blackTurnChip.alpha = 0f;
			}
			nextTurn();
			if (!currentPlayer().ai) {
				cameraPivot.transform.rotation = currentPlayer().boardRotation;
			}
		// Handle mouse if not Game Over
		} else if (currentPlayer().ai == false) {
			RaycastHit hit;
			Ray mouseRay = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (mouseRay, out hit, Mathf.Infinity, 1)) {
				Debug.DrawLine (mouseRay.origin, hit.point, Color.red, Time.deltaTime);
				spaceScript space = hit.collider.GetComponent<spaceScript> ();
				if (space) {
					if (space != hoverSpace) {
						hoverStop();
						hoverSpace = space;
						hoverSpace.hoverOn ();
					}
					if (Input.GetMouseButtonDown (0) && space.chip == 't') {
						hoverStop();
						space.setChip (whichTurn);
						currentPlayer().boardRotation = cameraPivot.transform.rotation;
						if (lastTurn == '0' && boardScript.instance.isWin(space.chip, space.position)) {
							lastTurn = whichTurn;
							lastTurnPos = space.position;
							victoryText.setText ("Last Turn?");
							victoryText.show = true;
						}
						nextTurn();
						if (!currentPlayer().ai) {
							changing_player = true;
							change_velocity = 0f;
							reset_time = 0f;
						}
						boardScript.instance.debugBoard ();
					}
				} else {
					hoverStop();
				}
			}
		}
	}

	public void win( char color ) {
		if (color == 'w') {
			Debug.Log ("White Player Wins");
			victoryText.setText ("White Wins");
		} else {
			Debug.Log ("Black Player Wins");
			victoryText.setText ("Black Wins");
		}
		victoryText.show = true;
		do_board_reset = true;
		gameOver = true;
	}

	void nextTurn() {
		whichTurn = ( whichTurn == 'w' ? 'b' : 'w' );
	}
	
	public Player getPlayer(char color) {
		if (color == 'w')
			return whitePlayer;
		else
			return blackPlayer;
	}
	
	public Player currentPlayer() {
		return getPlayer (whichTurn);
	}

	public Player otherPlayer() {
		return getPlayer (whichTurn == 'w' ? 'b' : 'w');
	}

	public void chipsCaptured( char color ) {
		Debug.Log ("Pieces captured!");
		getPlayer (color).chipsCaptured += 2;
		whiteCaptured.text = whitePlayer.chipsCaptured.ToString();
		blackCaptured.text = blackPlayer.chipsCaptured.ToString();
		if (getPlayer (color).chipsCaptured > 9) {
			win (color);
		}
	}

	void hoverStop() {
		if (hoverSpace) {
			hoverSpace.hoverOff ();
			hoverSpace = null;
		}
	}
}
