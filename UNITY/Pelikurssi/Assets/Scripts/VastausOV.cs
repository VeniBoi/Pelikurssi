//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class VastausOV : MonoBehaviour {

	
	private GameObject testi;
	public GameObject Prefab;
	public GameObject Correcti;
	public GameObject Wrongi;



	// Use this for initialization
	void Start () {

		 
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Correct()
	{
		
		StartCoroutine(CorrectIE());
		
	}


	public void WrongB()
	{
		StartCoroutine(WrongIEB());
	}

	public void WrongC()
	{
		StartCoroutine(WrongIEC());
	}

	public void WrongD()
	{
		StartCoroutine(WrongIED());
	}


	IEnumerator CorrectIE()
	{
		GameObject.Find("VastausA").GetComponent<Image>().color = Color.green;
		SieniNoukinta.score = SieniNoukinta.score * 2;
		Debug.Log("Pisteet tuplattu!");
		Correcti.SetActive(true);
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		yield return new WaitForSeconds(3);

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		testi = Instantiate(Prefab, transform.position, Quaternion.identity) as GameObject;

		Canvas canvas = GameObject.Find("Canvas2").GetComponent<Canvas>();

		testi.transform.SetParent(canvas.transform, false);

		GameObject.Find("Canvas").SetActive(false);


	}

	IEnumerator WrongIEB()
	{
		GameObject.Find("VastausA").GetComponent<Image>().color = Color.green;
		GameObject.Find("VastausB").GetComponent<Image>().color = Color.red;
		Cursor.visible = false;
		Wrongi.SetActive(true);
		Cursor.lockState = CursorLockMode.Locked;
		yield return new WaitForSeconds(3);
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		testi = Instantiate(Prefab, transform.position, Quaternion.identity) as GameObject;

		Canvas canvas = GameObject.Find("Canvas2").GetComponent<Canvas>();

		testi.transform.SetParent(canvas.transform, false);

		GameObject.Find("Canvas").SetActive(false);

	}

	IEnumerator WrongIEC()
	{
		GameObject.Find("VastausA").GetComponent<Image>().color = Color.green;
		GameObject.Find("VastausC").GetComponent<Image>().color = Color.red;
		Wrongi.SetActive(true);
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		yield return new WaitForSeconds(3);
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		testi = Instantiate(Prefab, transform.position, Quaternion.identity) as GameObject;

		Canvas canvas = GameObject.Find("Canvas2").GetComponent<Canvas>();

		testi.transform.SetParent(canvas.transform, false);

		GameObject.Find("Canvas").SetActive(false);

	}

	IEnumerator WrongIED()
	{
		GameObject.Find("VastausA").GetComponent<Image>().color = Color.green;
		GameObject.Find("VastausD").GetComponent<Image>().color = Color.red;
		Wrongi.SetActive(true);
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		yield return new WaitForSeconds(3);
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		testi = Instantiate(Prefab, transform.position, Quaternion.identity) as GameObject;

		Canvas canvas = GameObject.Find("Canvas2").GetComponent<Canvas>();

		testi.transform.SetParent(canvas.transform, false);

		GameObject.Find("Canvas").SetActive(false);

	}
}


