using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour {

	 void Start()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}


	public void Menu()
	{
		Debug.Log("Menu!");
		SceneManager.LoadScene("Menu");

	}


	public void QuitGame()
	{
		Debug.Log("Quit!");
		Application.Quit();
	}
}
