//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class Pause_Menu : MonoBehaviour {

	static public bool GameIsPaused = false;
	public AudioMixer audioMixer;
	public GameObject pauseMenuUI;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (GameIsPaused)
			{
				Cursor.lockState = CursorLockMode.Locked;  //Lukitaan kursori näyttöön ja piilotetaan se
				Cursor.visible = false;
				Resume();
			}
			else
			{
				Cursor.lockState = CursorLockMode.Confined;  //Lukitaan kursori näyttöön ja piilotetaan se
				Cursor.visible = true;
				Pause();
			}
		}
		
	}

	public void Resume()
	{
		pauseMenuUI.SetActive(false);
		Time.timeScale = 1f;
		GameIsPaused = false;
		Cursor.lockState = CursorLockMode.Locked;  //Lukitaan kursori näyttöön ja piilotetaan se
		Cursor.visible = false;

	}

	public void Pause()
	{
		pauseMenuUI.SetActive(true);
		Time.timeScale = 0f;
		GameIsPaused = true;
	}

	public void QuitGame()
	{
		Debug.Log("Suljetaan peli!");
		Application.Quit();
	}

	public void LoadMenu()
	{
		Debug.Log("Ladataan menu...");
		SceneManager.LoadScene("Menu");
		pauseMenuUI.SetActive(false);
		Time.timeScale = 1f;
		GameIsPaused = false;
	}

	public void setVolume (float volume)
	{
		audioMixer.SetFloat("volume", volume);
	}

	public void SetFullscreen (bool isFullscreen)
	{
		Debug.Log("FullScreen Pressed.");
		Screen.fullScreen = isFullscreen;
	}

	

}



