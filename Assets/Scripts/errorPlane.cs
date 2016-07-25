using UnityEngine;
using System.Collections;

public class errorPlane : MonoBehaviour {
	public float rotationMultiplier = 1f;

	void Update () {
		transform.Rotate (0f, Time.deltaTime * rotationMultiplier, 0f);
	}
}
