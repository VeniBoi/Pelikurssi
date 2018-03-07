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
	public Animator anim;

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
			GameObject.Find("SieniPickupKarpa").GetComponent<Text>().enabled = true;
			GameObject.Find("SieniPickupKarpa").GetComponent<Animator>().enabled = true;

			Debug.Log("Kärpässieni noukittu!");
            Destroy(other.gameObject);                      // Sienen tuhoaminen kun siihen osuu.
			score = score + -50;								// Scoren lisääminen.
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 0.1f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			StartCoroutine(SieniPysäytysKarpa());
			Kärpässieni++;
			

		}

		else if (other.gameObject.CompareTag("Keltavahvero"))
		{
			GameObject.Find("SieniPickupKelta").GetComponent<Text>().enabled = true;
			GameObject.Find("SieniPickupKelta").GetComponent<Animator>().enabled = true;

			Debug.Log("Kanttarelli noukittu!");
            Destroy(other.gameObject);
            score = score + 90;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 0.1f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			StartCoroutine(SieniPysäytysKelta());
			Keltavahvero++;
		}

		else if (other.gameObject.CompareTag("Haaparousku"))
		{
			GameObject.Find("SieniPickupHaapa").GetComponent<Text>().enabled = true;
			GameObject.Find("SieniPickupHaapa").GetComponent<Animator>().enabled = true;

			Debug.Log("Haaparousku noukittu!");
            Destroy(other.gameObject);
            score = score + 70;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 0.1f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			StartCoroutine(SieniPysäytysHaapa());
			Haaparousku++;
			

		}

		else if (other.gameObject.CompareTag("Herkkutatti"))
		{
			GameObject.Find("SieniPickupHerkku").GetComponent<Text>().enabled = true;
			GameObject.Find("SieniPickupHerkku").GetComponent<Animator>().enabled = true;

			Debug.Log("Herkkutatti noukittu!");
            Destroy(other.gameObject);
            score = score + 40;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 0.1f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			StartCoroutine(SieniPysäytysHerkku());
			Herkkutatti++;
			
		}

		else if (other.gameObject.CompareTag("PsiloSieni"))
		{
			GameObject.Find("SieniPickupPsilo").GetComponent<Text>().enabled = true;
			GameObject.Find("SieniPickupPsilo").GetComponent<Animator>().enabled = true;

			Debug.Log("Psilosieni noukittu!");
            Destroy(other.gameObject);
            score = score + -70;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed *= 2f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			Psilosieni++;
			StartCoroutine(SieniPysäytysPsilo());
		}

		

	}

	void SetCountText()
	{
		countText.text = "Score: " + score;


	}

	IEnumerator SieniPysäytysHaapa()		//Pysäyttää hahmon hetkeksi kun poimii sienen.
	{
		yield return new WaitForSeconds(0.2f);
		GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 8f;
		yield return new WaitForSeconds(1.6f);
	
		anim.Play("SieniAnimation", 0, 0);
		GameObject.Find("SieniPickupHaapa").GetComponent<Text>().enabled = false;
		GameObject.Find("SieniPickupHaapa").GetComponent<Animator>().enabled = false;
		
	}

	IEnumerator SieniPysäytysKarpa()        //Pysäyttää hahmon hetkeksi kun poimii sienen.
	{
		yield return new WaitForSeconds(0.2f);
		GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 8f;
		yield return new WaitForSeconds(1.6f);

		anim.Play("SieniAnimation", 0, 0);
		GameObject.Find("SieniPickupKarpa").GetComponent<Text>().enabled = false;
		GameObject.Find("SieniPickupKarpa").GetComponent<Animator>().enabled = false;

	}

	IEnumerator SieniPysäytysPsilo()        //Pysäyttää hahmon hetkeksi kun poimii sienen.
	{
		yield return new WaitForSeconds(0.2f);
		GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 8f;
		yield return new WaitForSeconds(1.6f);

		anim.Play("SieniAnimation", 0, 0);
		GameObject.Find("SieniPickupPsilo").GetComponent<Text>().enabled = false;
		GameObject.Find("SieniPickupPsilo").GetComponent<Animator>().enabled = false;

	}

	IEnumerator SieniPysäytysHerkku()        //Pysäyttää hahmon hetkeksi kun poimii sienen.
	{
		yield return new WaitForSeconds(0.2f);
		GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 8f;
		yield return new WaitForSeconds(1.6f);

		anim.Play("SieniAnimation", 0, 0);
		GameObject.Find("SieniPickupHerkku").GetComponent<Text>().enabled = false;
		GameObject.Find("SieniPickupHerkku").GetComponent<Animator>().enabled = false;

	}

	IEnumerator SieniPysäytysKelta()        //Pysäyttää hahmon hetkeksi kun poimii sienen.
	{
		yield return new WaitForSeconds(0.2f);
		GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 8f;
		yield return new WaitForSeconds(1.6f);

		anim.Play("SieniAnimation", 0, 0);
		GameObject.Find("SieniPickupKelta").GetComponent<Text>().enabled = false;
		GameObject.Find("SieniPickupKelta").GetComponent<Animator>().enabled = false;

	}

}
	
