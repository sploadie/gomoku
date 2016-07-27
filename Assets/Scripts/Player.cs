using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Position = spaceScript.Position;

public class Player : Object {

	public bool ai;
	public int std_depth = 3;
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

	public class boardState {
		public char[,] board;
		// Chips that YOU captured
		public int white;
		// Chips that you LOST
		public int black;

		public boardState (char [,] _board) {
			board = (char[,])_board.Clone();
			white = 0;
			black = 0;
		}

		public boardState (boardState _state) {
			board = (char[,])_state.board.Clone ();
			white = _state.white;
			black = _state.black;
		}

		public char this[int a, int b] {
			get {
				return board[a,b];
			}
			set {
				board[a,b] = value;
			}
		}

		public int this[char color] {
			get {
				if (color == 'w')
					return white;
				else
					return black;
			}
			set {
				if (color == 'w')
					white = value;
				else
					black = value;
			}
		}
	}

	private float Heuristic(boardState state) {
		float rand = Random.value * 100f - 50f;
		rand += state [color] * 100f;
		rand -= state [opponent.color] * 100f;
//		Debug.Log ("Heuristic: " + rand.ToString());
		return rand;
	}

	public float miniMax(boardState state, Position[] bestMove, float parentBestWeight, int depth) {
		Position move = new Position();
		bool moveSet = false;
		float bestWeight = -Mathf.Infinity;
		float tmpWeight;
		Position pos = new Position();
		float currentWeight = Heuristic(state);
		if (depth <= 0) {
			return currentWeight;
		} else if (currentWeight <= parentBestWeight) {
			return currentWeight;
		} else {
			int i, j;
			for (i = 0; i < 15; ++i) {
				for (j = 0; j < 15; ++j) {
					pos.set (i,j);
					if (state[i,j] == '0') {
//						Debug.Log ("Color: " + color.ToString() + " Move: " + i.ToString() + "," + j.ToString());
						state[i,j] = color;
						if (noFreeThree(state.board, pos)) {
							List<Position> captured = handleCapture (state.board, pos);
							state[color] += captured.Count;
							if (moveSet == false)
								tmpWeight = -opponent.miniMax(state, null, -Mathf.Infinity, depth - 1);
							else
								tmpWeight = -opponent.miniMax(state, null, -bestWeight, depth - 1);
							if (tmpWeight > bestWeight || moveSet == false) {
								bestWeight = tmpWeight;
								move.set (pos);
								moveSet = true;
							}
							state[color] -= captured.Count;
							captured.ForEach(delegate(Position captPos) {
								state[captPos.x, captPos.y] = opponent.color;
							});
						}
						state[i,j] = '0';
					}
				}
			}
		}
		if (moveSet == false) {
			Debug.Log ("AI couldn't find a move!!");
		}
		if (bestMove != null) {
			Debug.Log ("AI Minimax: " + bestWeight.ToString());
			bestMove[0].set (move);
		}
		return bestWeight;
	}

	public Position getMove(char[,] board) {
		boardState state = new boardState (board);
		state [color] = chipsCaptured;
		state [opponent.color] = opponent.chipsCaptured;
		Position[] bestMove = new Position[1];
		miniMax(state, bestMove, -Mathf.Infinity, std_depth);
		return bestMove[0];
	}

	// Returns true if no free three
	private bool noFreeThree (char[,] board, Position pos) {
		bool freeThree = false;
		char offense = board [pos.x, pos.y];
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
							tempChip = board [pos.x + i*(k+l), pos.y + j*(k+l)];
							if (tempChip != '0')
								continue;
							l = 1; // SECOND
							tempChip = board [pos.x + i*(k+l), pos.y + j*(k+l)];
							if (tempChip != offense)
								continue;
							l = 2; // THIRD
							tempChip = board [pos.x + i*(k+l), pos.y + j*(k+l)];
							if (tempChip != offense && tempChip != '0')
								continue;
							if (tempChip == '0')
								empty = true;
							l = 3; // FOURTH
							tempChip = board [pos.x + i*(k+l), pos.y + j*(k+l)];
							if (tempChip != offense && tempChip != '0')
								continue;
							if (tempChip == '0') {
								if (empty == true)
									continue;
								else
									empty = true;
							}
							l = 4; // FIFTH
							tempChip = board [pos.x + i*(k+l), pos.y + j*(k+l)];
							if (empty == true && tempChip != offense)
								continue;
							if (empty == false && tempChip != '0')
								continue;
							l = 5; // SIXTH
							if (empty == true && board [pos.x + i*(k+l), pos.y + j*(k+l)] != '0')
								continue;
							// FREE THREE VERIFICATION END
							// FREE THREE CONFIRMED
							if (freeThree) {
								// Debug.Log ("Double free three found!");
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

	// Returns list of captured chips; board modified!
	private List<Position> handleCapture (char[,] board, Position pos) {
		char offense = board [pos.x, pos.y];
		char defense = ( offense == 'w' ? 'b' : 'w' );
		List<Position> captured = new List<Position> ();
		int i, j;
		for (i = -1; i < 2; ++i) {
			for (j = -1; j < 2; ++j) {
				try {
					if ( board [pos.x + i, pos.y + j] == defense
					    && board [pos.x + i*2, pos.y + j*2] == defense
					    && board [pos.x + i*3, pos.y + j*3] == offense) {
						// Capture occured
						board [pos.x + i, pos.y + j] = '0';
						captured.Add(new Position(pos.x + i, pos.y + j));
						board [pos.x + i*2, pos.y + j*2] = '0';
						captured.Add(new Position(pos.x + i*2, pos.y + j*2));
					}
				} catch(System.IndexOutOfRangeException) {}
			}
		}
		return (captured);
	}

	// Returns true if chip at pos gives win
	private bool isWin (char[,] board, Position pos) {
		char color = board [pos.x, pos.y];
		int i, j, k, l;
		bool win;
		for (i = -1; i < 1; ++i) {
			for (j = -1; j < 2; ++j) {
				if (!(i == 0 && j ==0) && !(i == 0 && j == -1)) {
					for (k = -4; k < 1; k++) {
						try {
							win = true;
							for (l = 0; l < 5; l++) {
								if (!(board [pos.x + i*(k+l), pos.y + j*(k+l)] == color)) {
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
}
