using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
			StartCoroutine(coRoutineTest());


		}



	}

	IEnumerator coRoutineTest()
	{
        
		Debug.Log("Uusi sieni");
		yield return new WaitForSeconds(3);
        randomSpawner();

	}

    void randomSpawner()
    {
        randomInt = Random.Range(0, spawnees.Length);
        Instantiate(spawnees[randomInt], spawnPos.position, Quaternion.Euler(270, 0, 0));
    }
}
