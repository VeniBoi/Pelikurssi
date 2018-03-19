//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SieniSpawnerVol2 : MonoBehaviour
{
	public GameObject[] spawnees;
	public Transform spawnPos;


	int randomInt;

	// Use this for initialization
	void Start()
	{
		randomSpawner();
	}
	void Update()
	{
	}


	void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))          // Sienen piilottaminen kun siihen osuu.
		{
			GetComponent<Collider>().enabled = false;
			StartCoroutine(SpawnaaSieni());                //Aloitetaan rutiini joka on asetettu alempana.


		}



	}

	IEnumerator SpawnaaSieni()                 //Rutiini: Kun pelaaja osuu sieneen randomSpawner funktio pyörii läpi
	{                                           // Tämän jälkeen odotetaan hetki kunnes sieni spawnaa uudestaan.
												// Kun sieneen on osuttu asetetaan collideri hetkeksi pois päältä
												//Tämä estää sienikämppäämisen.
		yield return new WaitForSeconds(3);
		Debug.Log("Uusi sieni");
		randomSpawner();
		Debug.Log("Odotetaan 5 sekuntia.");
		yield return new WaitForSeconds(5);
		Debug.Log("5 sekuntia on mennyt.");
		GetComponent<Collider>().enabled = true;



	}



	void randomSpawner()                                        //Ottaa random sienen sille asetetusta listasta
	{                                                           // Objektin voi asettaa listaan pelin editorista (public).
		randomInt = Random.Range(0, spawnees.Length);
		Instantiate(spawnees[randomInt], spawnPos.position, Quaternion.Euler(270, 0, 0));
	}


}

