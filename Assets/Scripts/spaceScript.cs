using UnityEngine;
using System.Collections;

public class spaceScript : MonoBehaviour {

	GameObject chipObject = null;
	public char chip { get; private set; }
	public Position position { get; private set; }

	private char savedChip = '0';

	public struct Position {
		public int x;
		public int y;

		public Position( int _x, int _y ) {
			x = _x;
			y = _y;
		}

		public void set( int _x, int _y ) {
			x = _x;
			y = _y;
		}

		public Position Clone() {
			return new Position (x, y);
		}
	}

	void Awake () {
		chip = '0';
		position = new Position ((int)transform.position.x / 2 + 7, (int)transform.position.z / 2 + 7);
	}

	void Start () {
		if (!boardScript.instance)
			Debug.Log("Error: Board not ready!");
		else
			boardScript.instance.board [position.x, position.y] = this;
	}
	
	void Update () {
	}

	public void hoverOn () {
		if (chip == '0') {
			setTemp(playerScript.instance.whichTurn);
			if (boardScript.instance.canPlace(playerScript.instance.whichTurn, position)) {
				unsetTemp();
				setChip ('t');
			} else {
				unsetTemp();
				setChip ('f');
			}
		}
	}

	public void hoverOff () {
		if (chip == 't' || chip == 'f')
			setChip ('0');
	}

	public void setChip ( char type ) {
		if (chip == type)
			return;
		if (chipObject)
			Destroy (chipObject.gameObject);
		chip = type;
		chipObject = boardScript.instance.placeChip (this);
		if (chipObject) {
			chipObject.transform.parent = transform;
			chipObject.transform.localPosition = Vector3.up * 0.75f;
		}
	}

	private void setTemp ( char type ) {
		savedChip = chip;
		chip = type;
	}

	private void unsetTemp () {
		chip = savedChip;
	}
}
