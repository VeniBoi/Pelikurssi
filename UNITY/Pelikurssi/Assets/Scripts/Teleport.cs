using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour {

	private void Start()
	{
		
	}

	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Putous"))
		{
			Liikutus();
		}
	}

	void Liikutus()
	{
		transform.position = new Vector3(-2475.74f, 173.66f, -85.21922f);
	}

}
