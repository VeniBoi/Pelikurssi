//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalScore : MonoBehaviour {

	public Text scoreText;
	SieniNoukinta testi;

	// Use this for initialization
	void Start () {
		scoreText.text = "FINAL SCORE: " + SieniNoukinta.score;  //Päätetään mitä teksti sanoo ja haetaan score toisesta scriptistä.
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	
}
