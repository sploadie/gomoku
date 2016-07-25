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
	public Player whitePlayer = new Player('w');
	public GUIText whiteCaptured;
	public Player blackPlayer = new Player('b');
	public GUIText blackCaptured;
	
	// Board rotation animation variables
	bool resetting_board = false;
	Vector3 reset_angles_start;
	float reset_ratio;
	float reset_time;

	bool changing_player = false;
	float change_velocity = 0f;
	public float change_time = 1f;


	void Awake () {
		if (!instance)
			instance = this;
		whichTurn = 'w';
		whitePlayer.opponent = blackPlayer;
		blackPlayer.opponent = whitePlayer;
		blackPlayer.boardRotation = Quaternion.Euler (0f, 180f, 0f);
	}

	void Start () {
		whiteTurnChip.alpha = 1f;
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
		} else if (Input.GetKey (KeyCode.R)) {
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
		// Handle mouse
		} else {
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
						nextTurn();
						if (!currentPlayer().ai) {
							changing_player = true;
							reset_time = 0f;
						} else {
							// Handle AI FIXME
							nextTurn();
						}
						boardScript.instance.debugBoard ();
					}
				} else {
					hoverStop();
				}
			}
		}
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

	public void chipsCaptured( char color ) {
		Debug.Log ("Pieces captured!");
		getPlayer (color).chipsCaptured += 2;
		whiteCaptured.text = whitePlayer.chipsCaptured.ToString();
		blackCaptured.text = blackPlayer.chipsCaptured.ToString();
	}

	void hoverStop() {
		if (hoverSpace) {
			hoverSpace.hoverOff ();
			hoverSpace = null;
		}
	}
}
