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
	public GameObject SieniMenuUI;
	
	
	public GameObject SieniKirja2;

	private void Start()
	{
		
	}


	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (GameIsPaused)
			{
				Cursor.lockState = CursorLockMode.Locked;  //Lukitaan kursori näyttöön ja piilotetaan se
				Cursor.visible = false;
				SieniPois();
				Kirja2();
				Resume();
			}
			else
			{
				Cursor.lockState = CursorLockMode.Confined;  
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

	
	public void SieniPois()				
	{
		SieniMenuUI.SetActive(false);
		Time.timeScale = 1f;
		GameIsPaused = false;
		Cursor.lockState = CursorLockMode.Locked;  //Lukitaan kursori näyttöön ja piilotetaan se
		Cursor.visible = false;
	}

	public void Pois()					//Laittaa sienimenunu pois ja avaa pausemenun
	{
		pauseMenuUI.SetActive(true);
		SieniMenuUI.SetActive(false);
	}

	public void Kirja1()
	{
		pauseMenuUI.SetActive(true);
		
	}


	public void Kirja2()
	{
		pauseMenuUI.SetActive(true);
		SieniKirja2.SetActive(false);
	}

	public void Kirja2NuoliVas()
	{
		SieniKirja2.SetActive(false);
		SieniMenuUI.SetActive(true);
	}

	
}




