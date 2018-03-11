//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Varoitus : MonoBehaviour {

	public GameObject VaroitusRuutu;
	public Animator anim;
	

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	

	public void yes()
	{
		Time.timeScale = 1f;
		SieniNoukinta.score -= 70;
		GameObject.Find("SieniNoukinta").GetComponent<SieniNoukinta>().SetCountText();
		VaroitusRuutu.SetActive(false);
		Cursor.lockState = CursorLockMode.Locked;  //Lukitaan kursori näyttöön ja piilotetaan se
		Cursor.visible = false;
		SieniNoukinta.onTrue = true;
		
		//Miinusta pisteet ja sulje ruutu

	}

	public void no()
	{
		Time.timeScale = 1f;
		VaroitusRuutu.SetActive(false);
		Cursor.lockState = CursorLockMode.Locked;  //Lukitaan kursori näyttöön ja piilotetaan se
		Cursor.visible = false;
		//Älä tee mitään.
		//Sieni poistuu itsellään hetken kuluttua
	}
}
