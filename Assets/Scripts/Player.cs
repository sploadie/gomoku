using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Position = spaceScript.Position;
using System.Text;

public class Player : Object {

	public bool ai;
	public int std_depth = 2;
	public char color = '0';
	public int chipsCaptured = 0;
	public Player opponent;
	public Quaternion boardRotation = Quaternion.identity;

	public Player ( char _color, bool _ai ) {
		color = _color;
		ai = _ai;
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

		public string ToString () {
			StringBuilder sb = new StringBuilder();
			int i, j;
			for (i = 0; i < 15; ++i) {
				for (j = 0; j < 15; ++j) {
					sb.Append (board [i, j]);
				}
				sb.Append ('\n');
			}
			sb.AppendFormat ("{0}{1}\n", white, black);
			return sb.ToString ();
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

	
	// Returns free three count
	int freeThreeCount (char[,] board, Position pos) {
		int freeThrees = 0;
		char offense = color;
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
							freeThrees++;
						} catch(System.IndexOutOfRangeException) {}
					}
				}
			}
		}
		return freeThrees;
	}

	void checkLines (char[,] board, ref int freeThrees, ref int wins, ref int align) {
		freeThrees = 0;
		wins = 0;
		align = 0;
		int tmp;
		Position pos = new Position();
		int i, j;
		for (i = 0; i < 15; ++i) {
			for (j = 0; j < 15; ++j) {
				if (board[i, j] == '0')
					continue;
				pos.set (i, j);
				freeThrees += freeThreeCount (board, pos);
				tmp = winOrAlign(board, pos);
				if (tmp == 5)
					wins++;
				else if (tmp == 4)
					align++;
			}
		}
	}

	public float myHeuristic(boardState state) {
		float weight = 0;
		int freeThrees = 0;
		int opponentFreeThrees = 0;
		int wins = 0;
		int opponentWins = 0;
		int align = 0;
		int opponentAlign = 0;

		weight += Mathf.Pow(2, state [color]) * 1000;
		weight -= Mathf.Pow(2, state [opponent.color]) * 1000;

		if (state [color] > 9)
			weight += 1000 * 1000;
		else if (state [opponent.color] > 9)
			weight -= 1000 * 1000;

		checkLines(state.board, ref freeThrees, ref wins, ref align);
		opponent.checkLines(state.board, ref opponentFreeThrees, ref opponentWins, ref opponentAlign);

		if (freeThrees == 1)
			weight += 1;
		else if (freeThrees > 1)
			weight += 5;
		if (opponentFreeThrees == 1)
			weight -= 1;
		else if (opponentFreeThrees > 1)
			weight -= 5;
		if (align == 1)
			weight += 10;
		else if (align > 1)
			weight += 50;
		if (opponentAlign == 1)
			weight -= 10;
		else if (opponentAlign > 1)
			weight -= 50;
		if (wins == 1)
			weight += 100 * 1000;
		else if (wins > 1)
			weight += 500 * 1000;
		if (opponentWins == 1)
			weight -= 100 * 1000;
		else if (opponentWins > 1)
			weight -= 500 * 1000;
		return weight;
	}

	public float Heuristic(boardState state) {
		return myHeuristic (state);
	}

	public float miniMax(boardState state, Position[] bestMove, float parentBestWeight, int depth) {
		playerScript.instance.pings++;
		bool moveSet = false;
		Position localBestMove = new Position();
		float bestWeight = 0;
		if (depth == std_depth * 2) { // If at last node
			playerScript.instance.final_pings++;
			return -Heuristic(state);
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
						if (noFreeThree(state.board, tempPos) && maxDistance(state.board, tempPos, 1)) {
							if (depth < 2) {
								state[tempPos.x, tempPos.y] = color;
								List<Position> captured = handleCapture (state.board, tempPos);
								state[color] += captured.Count;
								possibleMoves.Add(new possibleMove(tempPos, Heuristic(state)));
								state[color] -= captured.Count;
								captured.ForEach(delegate(Position captPos) {
									state[captPos.x, captPos.y] = opponent.color;
								});
								state[tempPos.x, tempPos.y] = '0';
							} else {
								possibleMoves.Add(new possibleMove(tempPos, 0f));
							}
						}
						state[i,j] = '0';
					}
				}
			}
			// Sort moves
			if (depth < 2)
				possibleMoves.Sort((x, y) => y.weight.CompareTo(x.weight));
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
					bestWeight = opponent.miniMax(state, null, Mathf.Infinity, depth + 1);
					// FIXME START
					if (bestMove != null) {
						boardScript.instance.newAIText(move.pos, bestWeight.ToString() + " - " + move.weight.ToString());
					}
					// FIXME END
					localBestMove.set (move.pos);
					moveSet = true;
				} else {
					tmpWeight = opponent.miniMax(state, null, -bestWeight, depth + 1);
					// FIXME START
					if (bestMove != null) {
						boardScript.instance.newAIText(move.pos, tmpWeight.ToString() + " - " + move.weight.ToString());
					}
					// FIXME END
					if (tmpWeight > bestWeight) {
						bestWeight = tmpWeight;
						localBestMove.set (move.pos);
					}
				}
				if (parentBestWeight != Mathf.Infinity && bestWeight > parentBestWeight) {
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
//			Debug.Log ("AI Minimax: " + bestWeight.ToString());
			bestMove[0].set (localBestMove);
		}
		return -bestWeight;
	}

	public Position getMove(char[,] board) {
		boardState state = new boardState (board);
		state [color] = chipsCaptured;
		state [opponent.color] = opponent.chipsCaptured;
		Debug.Log (state.ToString ());
		Position[] bestMove = new Position[1];
		playerScript.instance.pings = 0;
		playerScript.instance.final_pings = 0;
		boardScript.instance.clearAIText ();
		long startTime = System.DateTime.Now.Ticks;
		if (boardEmpty (state.board) == false)
			miniMax (state, bestMove, Mathf.Infinity, 0);
		else
			bestMove [0].set (7, 7);
		Debug.Log ("Minimax Time: " + ((float)((System.DateTime.Now.Ticks - startTime) / System.TimeSpan.TicksPerMillisecond) / 1000f).ToString());
		return bestMove[0];
	}

	
	bool boardEmpty(char[,] board) {
		int i, j;
		for (i = 0; i < 15; ++i) {
			for (j = 0; j < 15; ++j) {
				if (board[i,j] != '0')
					return false;
			}
		}
		return true;
	}

	bool maxDistance(char[,] board, Position pos, int distance) {
		int i, j;
		for (i = -distance; i <= distance; ++i) {
			for (j = -distance; j <= distance; ++j) {
				if (i == 0 && j == 0)
					continue;
				try {
					if (board[pos.x + i, pos.y + j] != '0')
						return true;
				} catch(System.IndexOutOfRangeException) {}
			}
		}
		return false;
	}

	// Returns true if no free three
	bool noFreeThree (char[,] board, Position pos) {
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
	
	// Returns 0, 4, or 5
	int winOrAlign (char[,] board, Position pos) {
		int i, j, k, l;
		bool win, align, empty;
		char tempChip;
		bool isAlign = false;
		bool isWin = false;
		for (i = -1; i < 1; ++i) {
			for (j = -1; j < 2; ++j) {
				if (!(i == 0 && j == 0) && !(i == 0 && j == -1)) {
					for (k = -4; k < 1; k++) {
						try {
							win = true;
							align = false;
							empty = false;
							for (l = 0; l < 5; l++) {
								tempChip = board [pos.x + i * (k + l), pos.y + j * (k + l)];
								if (!(tempChip == color)) {
									win = false;
									if (tempChip == '0') {
										if (!empty) {
											align = true;
											empty = true;
										} else {
											align = false;
										}
									} else {
										align = false;
										break;
									}
								}
							}
							if (win) {
								isWin = true;
							} else if (align) {
								isAlign = true;
							}
						} catch (System.IndexOutOfRangeException) {
						}
					}
				}
			}
		}
		if (isWin)
			return 5;
		if (isAlign)
			return 4;
		return 0;
	}
}
