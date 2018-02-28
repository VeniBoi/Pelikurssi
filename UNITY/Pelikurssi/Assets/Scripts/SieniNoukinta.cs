//Spagettikoodi a'la Veni
//Use at your own risk. :-D


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
			score = score + 10;								// Scoren lisääminen.
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			Kärpässieni++;
			

		}

		else if (other.gameObject.CompareTag("Keltavahvero"))
		{
			Debug.Log("Kanttarelli noukittu!");
            Destroy(other.gameObject);
            score = score + 15;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			Keltavahvero++;
		}

		else if (other.gameObject.CompareTag("Haaparousku"))
		{
			Debug.Log("Haaparousku noukittu!");
            Destroy(other.gameObject);
            score = score + 30;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			Haaparousku++;
		}

		else if (other.gameObject.CompareTag("Herkkutatti"))
		{
			Debug.Log("Herkkutatti noukittu!");
            Destroy(other.gameObject);
            score = score + 50;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			Herkkutatti++;
		}

		else if (other.gameObject.CompareTag("PsiloSieni"))
		{
			Debug.Log("Psilosieni noukittu!");
            Destroy(other.gameObject);
            score = score + 5000;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed += 10;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			Psilosieni++;
		}


	}

	void SetCountText()
	{
		countText.text = "Score: " + score;


	}

	
}
	
