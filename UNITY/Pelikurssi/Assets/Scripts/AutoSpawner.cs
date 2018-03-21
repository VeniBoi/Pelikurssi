//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSpawner : MonoBehaviour
{
	public GameObject[] spawnees;
	public Transform spawnPos;



	int randomInt;

	// Use this for initialization
	void Start()
	{
		InvokeRepeating("randomSpawner", 1f, 5f);


	}
	void Update()
	{


	}

	void randomSpawner()                                        //Ottaa random objektinsille asetetusta listasta
	{                                                           // Objektin voi asettaa listaan unityn editorista (public).
		randomInt = Random.Range(0, spawnees.Length);
		Instantiate(spawnees[randomInt], spawnPos.position, Quaternion.Euler(0, 0, 0));
	}



}


