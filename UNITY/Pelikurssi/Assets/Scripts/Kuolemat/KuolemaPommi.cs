//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

public class KuolemaPommi : MonoBehaviour
{
	
	public PostProcessingProfile otherProfile;
	private GameObject testi;
	public GameObject Prefab;
	// Use this for initialization
	void Start()
	{
		GameObject pisteet = GameObject.Find("SieniNoukinta");
		SieniNoukinta sieninoukinta = pisteet.GetComponent<SieniNoukinta>();

	}

	// Update is called once per frame
	void Update()
	{
		

	}

	void OnTriggerEnter(Collider other)             //Jos objekti jolla on pelaaja tägi osuu tähän collideriin
	{                                                   // niin ladataan uusi  scene (loppu).
		if (other.gameObject.CompareTag("Player"))
		{
			SieniNoukinta.updateTimer = false;
			StartCoroutine(Pommi());
			//GameObject.Find("PommiText").GetComponent<Text>().enabled = true;
			//SceneManager.LoadScene("Loppu");
		}

	}

	 IEnumerator Pommi()
	{
		Debug.Log("Loppu ladattu.");
		GameObject.Find("ScoreText").GetComponent<Text>().enabled = false;
		GameObject.Find("Multiplier").GetComponent<Text>().enabled = false;

		yield return new WaitForSeconds(3);
		
		GameObject.Find("Main Camera").GetComponent<PostProcessingBehaviour>().profile = otherProfile;
		Time.timeScale = 0f;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		testi = Instantiate(Prefab, transform.position, Quaternion.identity) as GameObject;

		Canvas canvas = GameObject.Find("PauseCanvas").GetComponent<Canvas>();

		testi.transform.SetParent(canvas.transform, false);


	}
}