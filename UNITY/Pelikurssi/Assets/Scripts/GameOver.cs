//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour {

	 void Start()
	{
		Cursor.lockState = CursorLockMode.None;		//Asetetaan kursori näkyväksi uudestaan.
		Cursor.visible = true;
	}


	public void Menu()
	{
		Debug.Log("Menu!");
		SceneManager.LoadScene("Menu");

	}
											//Tehdään funktioita joita UI:n buttonit käyttävät.

	public void QuitGame()
	{
		Debug.Log("Quit!");
		Application.Quit();
	}
}
