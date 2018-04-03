//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LevelLoader : MonoBehaviour  //Tehdään funktioita joita UI:n buttonit käyttävät.
{
	public GameObject Highscores;
	public GameObject loadingScreen;
	public Slider slider;

    public void LoadLevel (int sceneIndex)
    {
		Highscores.SetActive(false);
		StartCoroutine(LoadAsynchronously(sceneIndex));

    }
    
	public void QuitGame()
	{
		Debug.Log("Quit!");
		Application.Quit();
	}

	

	IEnumerator LoadAsynchronously (int sceneIndex)
	{
		AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
		loadingScreen.SetActive(true);

		while  (operation.isDone == false)
		{
			float progress = Mathf.Clamp01(operation.progress / .9f);
			slider.value = progress;
			Time.timeScale = 0f;

			yield return null;
		}
	}

	public void SetFullscreen(bool isFullscreen)
	{
		Debug.Log("FullScreen Pressed.");
		Screen.fullScreen = isFullscreen;
	}
}
