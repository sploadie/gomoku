using UnityEngine;
using System.Collections;

public class spaceScript : MonoBehaviour {

	GameObject chip = null;
	public char chip_type { get; private set; }
	public int[] position { get; private set; }

	void Awake () {
		chip_type = 'n';
		position = new int[2] { (int)transform.position.x / 2 + 9, (int)transform.position.z / 2 + 9 };
	}

	void Start () {
		boardScript.instance.board [position[0], position[1]] = chip_type;
	}
	
	void Update () {
	}

	public void hoverOn () {
		if (chip_type == 'n')
			setChip ('t');
	}

	public void hoverOff () {
		if (chip_type == 't')
			setChip ('n');
	}

	public void setChip ( char type ) {
		if (chip_type != 'n') {
			Destroy (chip.gameObject);
			chip = null;
		}
		chip_type = type;
		if (chip_type != 'n') {
			chip = boardScript.instance.genChip (this, chip_type);
			if (!chip)
				Debug.Log ("No Chip at " + this);
			else
				chip.transform.localPosition = Vector3.up * 0.75f;
		}
		boardScript.instance.board [position[0], position[1]] = chip_type;
	}
}
