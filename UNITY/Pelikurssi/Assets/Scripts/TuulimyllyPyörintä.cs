//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TuulimyllyPyörintä : MonoBehaviour {

	public float speed = 10f;
	
	
	void Update () {

		transform.Rotate(Vector3.up, speed * Time.deltaTime);   //Vector.up/down/right/left/forward/back, päättää pyörimisen suunnan.

	}
}
