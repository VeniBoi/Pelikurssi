//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KuolemaCanvas : MonoBehaviour {

	public static KuolemaCanvas instance { get; private set; }

	public int value;

	// Use this for initialization
	private void Awake () {
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
