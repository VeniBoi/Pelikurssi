//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class FinalScore : MonoBehaviour {

	public TextMeshProUGUI Fly;
	public TextMeshProUGUI Chanterelle;
	public TextMeshProUGUI Lactariusrufus;
	public TextMeshProUGUI Russula;
	public TextMeshProUGUI Penny;
	public TextMeshProUGUI Leccinum;
	public TextMeshProUGUI Lactarius;
	public TextMeshProUGUI scoreText;
	public TextMeshProUGUI Time;
	SieniNoukinta testi;

	// Use this for initialization
	void Start () {
		scoreText.text = "" + SieniNoukinta.score;  //Päätetään mitä teksti sanoo ja haetaan score toisesta scriptistä.

		Fly.text = SieniNoukinta.Kärpässieni + " FLY AGARIC";
		Chanterelle.text = SieniNoukinta.Keltavahvero + " CHANTERELLE";
		Lactariusrufus.text = SieniNoukinta.Kangasrousku + " LACTARIUS RUFUS";		//Asetetaan määrät muuttujista jotka on haettu sieninoukinta koodista.
		Russula.text = SieniNoukinta.Isohapero + " RUSSULA PALUDOSA";
		Penny.text = SieniNoukinta.Herkkutatti + " PENNY BUN";
		Leccinum.text = SieniNoukinta.Haavapunikkitatti + " LECCINUM";
		Lactarius.text = SieniNoukinta.Haaparousku + " LACTARIUS TRIVIALIS";


		float aika = SieniNoukinta.levelTimer;		//Haetaan aika sieninoukinta koodista ja näytetään se.
		Time.text = aika.ToString("f2") + " seconds";
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	
}
