using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Miina : MonoBehaviour {

	public GameObject OsumaEfekti;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			MiinaOsui();
		}
	}

	void MiinaOsui()
	{
		Instantiate(OsumaEfekti, transform.position, transform.rotation);
		Destroy(gameObject);

	}
}
