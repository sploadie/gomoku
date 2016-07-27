using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Position = spaceScript.Position;

public class Player : Object {

	public bool ai;
	public int std_depth = 4;
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

	struct possibleMove {
		public Position pos;
		public float weight;

		public possibleMove (Position _pos, float _weight) {
			pos = new Position(_pos);
			weight = _weight;
		}
	}
	
	public float myHeuristic(boardState state) {
		float weight = 0;
		weight += state [color] * 100f;
		int count = 0;
		int total_distance = 0;
		int i, j;
		for (i = 0; i < 15; ++i) {
			for (j = 0; j < 15; ++j) {
				if (state[i,j] == color) {
					count++;
					total_distance += Mathf.Abs(i - 7) + Mathf.Abs(j - 7);
				}
			}
		}
		weight += 14 - ((float)total_distance / (float)count);
		return weight;
	}

	public float Heuristic(boardState state) {
		return myHeuristic (state) - opponent.myHeuristic (state);
	}

	static string miniMaxDebug;
	public float miniMax(boardState state, Position[] bestMove, float parentBestWeight, int depth) {
		playerScript.instance.pings++;
		bool moveSet = false;
		Position localBestMove = new Position();
		float bestWeight = -Mathf.Infinity;
		if (depth <= 0) { // If at last node
			playerScript.instance.final_pings++;
			return Heuristic(state);
		} else {
			// Get all moves
			Position tempPos = new Position();
			List<possibleMove> possibleMoves = new List<possibleMove>();
			int i, j;
			for (i = 0; i < 15; ++i) {
				for (j = 0; j < 15; ++j) {
					tempPos.set (i,j);
					if (state[i,j] == '0') {
						state[i,j] = color;
						if (noFreeThree(state.board, tempPos)) {
							if (std_depth - depth < 2)
								possibleMoves.Add(new possibleMove(tempPos, -opponent.Heuristic(state)));
							else
								possibleMoves.Add(new possibleMove(tempPos, 0f));
						}
						state[i,j] = '0';
					}
				}
			}
			// Sort moves
			if (std_depth - depth < 2)
				possibleMoves.Sort((x, y) => x.weight.CompareTo(y.weight));
			// Calculate moves
			float tmpWeight;
			bool doBreak = false;
			possibleMoves.ForEach(delegate(possibleMove move) {
				if (doBreak)
					return;
				state[move.pos.x, move.pos.y] = color;
				List<Position> captured = handleCapture (state.board, move.pos);
				state[color] += captured.Count;
				// MINIMAX START
				if (moveSet == false) {
					bestWeight = -opponent.miniMax(state, null, -parentBestWeight, depth - 1);
					localBestMove.set (move.pos);
					moveSet = true;
				} else {
					tmpWeight = -opponent.miniMax(state, null, -bestWeight, depth - 1);
					if (tmpWeight > bestWeight) {
						bestWeight = tmpWeight;
						localBestMove.set (move.pos);
					} else if (Mathf.Abs(parentBestWeight) == Mathf.Infinity) {
						doBreak = true;
					}
				}
				if (bestWeight > parentBestWeight && Mathf.Abs(parentBestWeight) != Mathf.Infinity) {
					doBreak = true;
				}
				// MINIMAX END
				state[color] -= captured.Count;
				captured.ForEach(delegate(Position captPos) {
					state[captPos.x, captPos.y] = opponent.color;
				});
				state[move.pos.x, move.pos.y] = '0';
			});
		}
		if (moveSet == false) {
			Debug.Log ("AI couldn't find a move!!");
		}
		if (bestMove != null) {
			Debug.Log ("AI Minimax: " + bestWeight.ToString());
			bestMove[0].set (localBestMove);
		}
//		miniMaxDebug += "Depth:" + depth.ToString() + " Weight:" + bestWeight.ToString() + "\n";
		return bestWeight;
	}

	public Position getMove(char[,] board) {
		boardState state = new boardState (board);
		state [color] = chipsCaptured;
		state [opponent.color] = opponent.chipsCaptured;
		Position[] bestMove = new Position[1];
		miniMaxDebug = "MiniMax Debug:\n";
		playerScript.instance.pings = 0;
		playerScript.instance.final_pings = 0;
		miniMax(state, bestMove, Mathf.Infinity, std_depth);
		Debug.Log (miniMaxDebug);
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
