//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSpawnerVas : MonoBehaviour
{
	public GameObject[] spawnees;
	public Transform spawnPos;



	int randomInt;

	// Use this for initialization
	void Start()
	{
		InvokeRepeating("randomSpawner", 2f, 4.5f);


	}
	void Update()
	{


	}

	void randomSpawner()                                        //Ottaa random objektinsille asetetusta listasta
	{                                                           // Objektin voi asettaa listaan unityn editorista (public).
		randomInt = Random.Range(0, spawnees.Length);
		Instantiate(spawnees[randomInt], spawnPos.position, Quaternion.Euler(0, 180, 0));
	}



}

