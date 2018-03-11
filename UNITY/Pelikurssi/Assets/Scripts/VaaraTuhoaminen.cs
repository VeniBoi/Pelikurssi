//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaaraTuhoaminen : MonoBehaviour {

	

	// Use this for initialization
	void Start () {
		StartCoroutine(Tuhous());
	}
	
	// Update is called once per frame
	



	IEnumerator Tuhous()
	{
		yield return new WaitForSeconds(60);
		Destroy(gameObject);
	}
}


