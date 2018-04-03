using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SetHighScore : MonoBehaviour
{
	public int score;
	public string username;
	public InputField input;

	private void Start()
	{
		score = SieniNoukinta.score;
	}

	private void Update()
	{

		if (Input.GetKeyDown(KeyCode.Space))
		{
			Debug.Log("" + username);
		}
	}

	
	public void GetInput(string userinput)
	{
		username = userinput;
		input.text = "";
		Highscores.AddNewHighscore(username, score);
		StartCoroutine(Siirto());

	}

	IEnumerator Siirto()
	{
		yield return new WaitForSeconds(0.5f);
		SceneManager.LoadScene("Menu");
	}
}


