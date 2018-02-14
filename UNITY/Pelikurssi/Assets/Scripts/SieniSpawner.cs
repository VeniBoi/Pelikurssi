using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SieniSpawner : MonoBehaviour
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


	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))          // Sienen piilottaminen kun siihen osuu.
		{
			GetComponent<Collider>().enabled = false;
			StartCoroutine(coRoutineTest());


		}



	}

	IEnumerator coRoutineTest()
	{
        
		
		yield return new WaitForSeconds(3);
		Debug.Log("Uusi sieni");
		randomSpawner();
		Debug.Log("Odotetaan 5 sekuntia.");
		yield return new WaitForSeconds(5);
		Debug.Log("5 sekuntia on mennyt.");
		GetComponent<Collider>().enabled = true;
		
		

	}

	

	void randomSpawner()
    {
        randomInt = Random.Range(0, spawnees.Length);
        Instantiate(spawnees[randomInt], spawnPos.position, Quaternion.Euler(270, 0, 0));
    }

	
}
