//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Kysymys : MonoBehaviour
{
	public GameObject[] spawnees;
	public Transform spawnPos;
	private GameObject testi;
	public GameObject Dialogi1;
	public GameObject Dialogi2;
	public Kysymys koodi;

	
	int randomInt;

	// Use this for initialization
	void Start()
	{
		Kysymys koodi = GetComponent<Kysymys>();
		
		
	}
	void Update()
	{
		SpawnaaPanel();

	}


	public void SpawnaaPanel()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Debug.Log("Paneeli spawnattu");
			randomSpawner();
			PoistaDialogi();
		}
		
	}

	//Instantiate(spawnees[randomInt], transform.position, Quaternion.identity);

	

	void randomSpawner()                                        //Ottaa random sienen sille asetetusta listasta
	{                                                           // Objektin voi asettaa listaan pelin editorista (public).
		randomInt = Random.Range(0, spawnees.Length);
		testi = Instantiate(spawnees[randomInt], transform.position, Quaternion.identity) as GameObject;

		Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

		testi.transform.SetParent(canvas.transform, false);
		
	}

	void PoistaDialogi()
	{
		Dialogi1.SetActive(false);
		Dialogi2.SetActive(false);

		koodi.enabled = false;

	}

}