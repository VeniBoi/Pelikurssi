//Spagettikoodi a'la Veni
//Use at your own risk. :-D


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SieniNoukinta : MonoBehaviour
{
	static public int Herkkutatti;					//Sienien noukinta määrät.
	static public int Kärpässieni;
	static public int Haaparousku;
	static public int Psilosieni;
	static public int Keltavahvero;
	static public int score;
	public Text countText;

	// Use this for initialization
	void Start()
	{
		Herkkutatti = 0;
		Haaparousku = 0;
		Kärpässieni = 0;				
		Haaparousku = 0;
		Psilosieni = 0;
		Keltavahvero = 0;
		score = 0;                       //Asetetaan pistemäärä nollaan.
		SetCountText();
	}

	// Update is called once per frame
	void Update()
	{
	}

	void OnTriggerEnter(Collider other )
	{
		if (other.gameObject.CompareTag("Karpassieni"))          
		{
			Debug.Log("Kärpässieni noukittu!");
            Destroy(other.gameObject);                      // Sienen tuhoaminen kun siihen osuu.
			score = score + -50;								// Scoren lisääminen.
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 0.1f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			StartCoroutine(SieniPysäytys());
			Kärpässieni++;
			

		}

		else if (other.gameObject.CompareTag("Keltavahvero"))
		{
			Debug.Log("Kanttarelli noukittu!");
            Destroy(other.gameObject);
            score = score + 90;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 0.1f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			StartCoroutine(SieniPysäytys());
			Keltavahvero++;
		}

		else if (other.gameObject.CompareTag("Haaparousku"))
		{
			Debug.Log("Haaparousku noukittu!");
            Destroy(other.gameObject);
            score = score + 70;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 0.1f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			StartCoroutine(SieniPysäytys());
			Haaparousku++;
		}

		else if (other.gameObject.CompareTag("Herkkutatti"))
		{
			Debug.Log("Herkkutatti noukittu!");
            Destroy(other.gameObject);
            score = score + 40;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 0.1f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			StartCoroutine(SieniPysäytys());		
			Herkkutatti++;
		}

		else if (other.gameObject.CompareTag("PsiloSieni"))
		{
			Debug.Log("Psilosieni noukittu!");
            Destroy(other.gameObject);
            score = score + -70;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed *= 2f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			Psilosieni++;
		}

		

	}

	void SetCountText()
	{
		countText.text = "Score: " + score;


	}

	IEnumerator SieniPysäytys()		//Pysäyttää hahmon hetkeksi kun poimii sienen.
	{
		yield return new WaitForSeconds(0.2f);
		GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 8f;		
	}


	
}
	
