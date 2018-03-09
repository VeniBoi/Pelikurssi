//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour {

	 void Start()
	{
		Cursor.lockState = CursorLockMode.None;		//Asetetaan kursori näkyväksi uudestaan.
		Cursor.visible = true;
	}


	public void Menu()
	{
		Debug.Log("Menu!");
		GameObject.Find("PommiText").GetComponent<Text>().enabled = false;
		GameObject.Find("VesiText").GetComponent<Text>().enabled = false;
		GameObject.Find("NuotioText").GetComponent<Text>().enabled = false;
		GameObject.Find("MyllyText").GetComponent<Text>().enabled = false;
		SceneManager.LoadScene("Menu");

	}
											//Tehdään funktioita joita UI:n buttonit käyttävät.

	public void QuitGame()
	{
		Debug.Log("Quit!");
		Application.Quit();
	}
}
