//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KuolemaNuotio : MonoBehaviour
{

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
			Debug.Log("Loppu ladattu.");

			GameObject.Find("NuotioText").GetComponent<Text>().enabled = true;
			SceneManager.LoadScene("Loppu");
		}

	}
}