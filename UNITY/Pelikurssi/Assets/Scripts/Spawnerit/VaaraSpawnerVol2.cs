//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaaraSpawnerVol2 : MonoBehaviour
{
	public GameObject[] spawnees;
	public Transform spawnPos;
	bool isSpawned;



	int randomInt;

	// Use this for initialization
	void Start()
	{
		randomSpawner();
		isSpawned = false;

	}
	void Update()
	{
		StartCoroutine(Testi());

	}


	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))          // Sienen piilottaminen kun siihen osuu.
		{
			GetComponent<Collider>().enabled = false;
			StartCoroutine(coRoutineTest());                //ALoitetaan rutiini joka on asetettu alempana.


		}




	}



	IEnumerator coRoutineTest()                 //Rutiini: Kun pelaaja osuu sieneen randomSpawner funktio pyörii läpi
	{                                           // Tämän jälkeen odotetaan hetki kunnes sieni spawnaa uudestaan.
												// Kun sieneen on osuttu asetetaan collideri hetkeksi pois päältä
												//Tämä estää sienikämppäämisen.
		yield return new WaitForSeconds(3);
		Debug.Log("Uusi vaara!");
		Destroy(gameObject);
		yield return new WaitForSeconds(3);
		randomSpawner();

		yield return new WaitForSeconds(5);

		GetComponent<Collider>().enabled = true;



	}




	void randomSpawner()                                        //Ottaa random objektin sille asetetusta listasta
	{                                                           // Objektin voi asettaa listaan pelin editorista (public).
		randomInt = Random.Range(0, spawnees.Length);
		Instantiate(spawnees[randomInt], spawnPos.position, Quaternion.Euler(270, 0, 0));
	}



	IEnumerator Testi()
	{
		if (!isSpawned)
		{
			isSpawned = true;
			yield return new WaitForSeconds(65);
			randomSpawner();
			isSpawned = false;
		}


	}



}
