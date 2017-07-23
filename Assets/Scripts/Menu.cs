using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	public static bool ai_mode;
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PlayerVsPlayer () {
		Debug.Log ("Player Vs Player");
		SceneManager.LoadScene("Board");
		ai_mode = false;
	}

	public void PlayerVsAI () {
		Debug.Log ("Player VS AI");
		SceneManager.LoadScene("Board");
		ai_mode = true;
	}
}
