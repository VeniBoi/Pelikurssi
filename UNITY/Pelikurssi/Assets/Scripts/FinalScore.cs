using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalScore : MonoBehaviour {

	public Text scoreText;

	// Use this for initialization
	void Start () {
		scoreText.text = "FINAL SCORE:" + SieniNoukinta.score;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	
}
