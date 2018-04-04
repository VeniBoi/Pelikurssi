//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class KarhuSpawner : MonoBehaviour
{
	public GameObject[] spawnees;
	public Transform spawnPos;
	public int aika1;
	public int aika2;

	static public int karhujaKartalla;
	public int Karhut;
	public Text setText;


	int randomInt;

	// Use this for initialization
	void Start()
	{
		karhujaKartalla = 1;
		InvokeRepeating("randomSpawner", 1f, Random.Range(aika1, aika2));
		

	}
	void Update()
	{
		

	}



	




	void randomSpawner()                                        //Ottaa random objektinsille asetetusta listasta
	{                                                           // Objektin voi asettaa listaan unityn editorista (public).
		randomInt = Random.Range(0, spawnees.Length);
		Instantiate(spawnees[randomInt], spawnPos.position, Quaternion.Euler(270, 0, 0));
		karhujaKartalla++;
	}

	public void SetBearText()
	{
		setText.text = "BEARS IN THE AREA :  " + karhujaKartalla;
	}

}


