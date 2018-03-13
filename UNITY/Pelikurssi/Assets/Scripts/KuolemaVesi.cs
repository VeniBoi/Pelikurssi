﻿//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

public class KuolemaVesi : MonoBehaviour {

	public GameObject Paneeli;
	public PostProcessingProfile otherProfile;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		

	}

	 void OnTriggerEnter(Collider other)				//Jos objekti jolla on pelaaja tägi osuu tähän collideriin
	{													// niin ladataan uusi  scene (loppu).
		if (other.gameObject.CompareTag("Player"))
		{
			Debug.Log("Loppu ladattu.");

			GameObject.Find("Main Camera").GetComponent<PostProcessingBehaviour>().profile = otherProfile;
			Paneeli.SetActive(true);
			Time.timeScale = 0f;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;


			//GameObject.Find("VesiText").GetComponent<Text>().enabled = true;
			//SceneManager.LoadScene("Loppu");
		}

	}
}
