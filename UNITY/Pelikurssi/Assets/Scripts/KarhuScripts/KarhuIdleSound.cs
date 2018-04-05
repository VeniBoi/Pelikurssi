using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KarhuIdleSound : MonoBehaviour {

	public AudioSource Aani;

	// Use this for initialization
	void Start () {
		InvokeRepeating("Soita", 1, Random.Range(30, 60));
		
	}
	
	// Update is called once per frame
	void Update () {
		

	}

	public void Soita()
	{
		Aani.Play();
	}
}
