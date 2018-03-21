using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutonLiikutus : MonoBehaviour {


	public int speed;

	// Use this for initialization
	void Start () {
		StartCoroutine(Tuhous());
	}
	
	// Update is called once per frame
	void Update () {

		transform.Translate(Vector3.forward * Time.deltaTime * speed);

	}

	IEnumerator Tuhous()
	{
		yield return new WaitForSeconds(14);
		Destroy(gameObject);
	}
}
