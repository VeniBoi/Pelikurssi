using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScreen : MonoBehaviour {

	public GameObject Test;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.anyKeyDown)
		{
			//GameObject.Find("TutorialImage").GetComponent<Text>().enabled = false;
			GameObject.Find("TutorialScreen").SetActive(false);
			GameObject.Find("ScoreText").GetComponent<Text>().enabled = true;
			
			Time.timeScale = 1f;
			//Destroy(Test);

		}

	}
}
