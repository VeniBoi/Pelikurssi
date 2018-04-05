﻿//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

public class KuolemaKarhu : MonoBehaviour
{

	public GameObject Paneeli;
	public PostProcessingProfile otherProfile;
	private GameObject testi;
	public GameObject Prefab;
	public AudioSource Aani;
	

	// Use this for initialization
	void Start()
	{
		

	}

	// Update is called once per frame
	void Update()
	{


	}

	void OnTriggerEnter(Collider other)             //Jos objekti jolla on pelaaja tägi osuu tähän collideriin
	{                                                   // niin ladataan uusi  scene (loppu).
		if (other.gameObject.CompareTag("Player"))
		{
			Aani.enabled = false;
			Debug.Log("Loppu ladattu.");
			GameObject.Find("ScoreText").GetComponent<Text>().enabled = false;
			GameObject.Find("Multiplier").GetComponent<Text>().enabled = false;

			GameObject.Find("Main Camera").GetComponent<PostProcessingBehaviour>().profile = otherProfile;
			//Paneeli.SetActive(true);
			Time.timeScale = 0f;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			SieniNoukinta.updateTimer = false;

			testi = Instantiate(Prefab, transform.position, Quaternion.identity) as GameObject;

			Canvas canvas = GameObject.Find("PauseCanvas").GetComponent<Canvas>();

			testi.transform.SetParent(canvas.transform, false);
			//GameObject.Find("VesiText").GetComponent<Text>().enabled = true;
			//SceneManager.LoadScene("Loppu");
		}

	}
}
