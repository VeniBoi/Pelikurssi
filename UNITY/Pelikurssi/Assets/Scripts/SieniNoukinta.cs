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
	public GameObject Varoitus;

	static public int kavalakarpassieni;
	static public int Haavapunikkitatti;
	static public int Kangasrousku;
	static public int Isohapero;
	static public int Herkkutatti;					//Sienien noukinta määrät.
	static public int Kärpässieni;
	static public int Haaparousku;
	static public int Psilosieni;
	static public int Keltavahvero;
	static public float score;
	static public float multiplier;

	public Text multiText;
	public Text countText;
	public Animator anim;

	public GameObject SiluettiKavala;
	public GameObject SiluettiRussu;
	public GameObject SiluettiPenny;
	public GameObject SiluettiFly;
	public GameObject SiluettiChan;
	public GameObject SiluettiLact;
	public GameObject SiluettiRufus;
	public GameObject SiluettiLecci;


	static public bool onTrue = false;
	static public float levelTimer;
	static public bool updateTimer = true;
	static public float minutes;

	bool herkku = true;
	bool kelta = true;
	bool kavala = true;
	bool karpas = true;
	bool haapa = true;
	bool kangas = true;
	bool iso = true;
	bool haava = true;







	// Use this for initialization
	void Start()
	{
		Haavapunikkitatti = 0;
		Isohapero = 0;
		Kangasrousku = 0;
		Herkkutatti = 0;
		Haaparousku = 0;
		Kärpässieni = 0;				
		Haaparousku = 0;
		Psilosieni = 0;
		Keltavahvero = 0;
		score = 0;                       //Asetetaan pistemäärä nollaan.
		multiplier = 1;
		
		SetCountText();
		multiplierSet();

		levelTimer = 0;
		
	}

	// Update is called once per frame
	void Update()
	{
		
		if (updateTimer == true)
		{
			levelTimer += Time.deltaTime*1;

		}

		minutes = levelTimer / 60;

	}

	private void OnTriggerStay(Collider collider)
	{
		if (onTrue == true && collider.gameObject.CompareTag("Karpassieni")) //Tuhoaa sienen ja soittaa animaation jos painetaan yes.
		{
			StartCoroutine(SieniPysäytysKarpa());
			Destroy(collider.gameObject);
			GameObject.Find("SieniPickupKarpa").GetComponent<Text>().enabled = true;
			GameObject.Find("SieniPickupKarpa").GetComponent<Animator>().enabled = true;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			SiluettiFly.GetComponent<Image>().enabled = false;
			
			Kärpässieni++;
			onTrue = false;

			if (karpas == true)
			{
				multiplier += .1f;
				karpas = false;
				multiplierSet();
			}
		}

		else if (onTrue == true && collider.gameObject.CompareTag("kavalakarpassieni")) //Tuhoaa sienen ja soittaa animaation jos painetaan yes.
		{
			StartCoroutine(SieniPysäytysKavala());
			Destroy(collider.gameObject);
			GameObject.Find("SieniPickupKavala").GetComponent<Text>().enabled = true;
			GameObject.Find("SieniPickupKavala").GetComponent<Animator>().enabled = true;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			SiluettiKavala.GetComponent<Image>().enabled = false;
			
			kavalakarpassieni++;
			onTrue = false;

			if (kavala == true)
			{
				multiplier += .1f;
				kavala = false;
				multiplierSet();
			}

		}

	}



	void OnTriggerEnter(Collider other )
	{

		if (other.gameObject.CompareTag("kavalakarpassieni"))
		{
			Debug.Log("Kavalaan osuttu");
			Cursor.lockState = CursorLockMode.None;  //Lukitaan kursori näyttöön ja piilotetaan se
			Cursor.visible = true;
			Varoitus.SetActive(true);
			Time.timeScale = 0f;
			
		}


		else if (other.gameObject.CompareTag("Karpassieni")) {        
		
			Cursor.lockState = CursorLockMode.None;  //Lukitaan kursori näyttöön ja piilotetaan se
			Cursor.visible = true;
			Varoitus.SetActive(true);
			Time.timeScale = 0f;

			


			/*GameObject.Find("SieniPickupKarpa").GetComponent<Text>().enabled = true;
			GameObject.Find("SieniPickupKarpa").GetComponent<Animator>().enabled = true;

			Debug.Log("Kärpässieni noukittu!");
            Destroy(other.gameObject);                      // Sienen tuhoaminen kun siihen osuu.
			score = score + -50;								// Scoren lisääminen.
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 0.1f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			StartCoroutine(SieniPysäytysKarpa());
			Kärpässieni++;
			*/

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
			SiluettiChan.GetComponent<Image>().enabled = false;
			Keltavahvero++;

			if (kelta == true)
			{
				multiplier += .1f;
				kelta = false;
				multiplierSet();
			}
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
			SiluettiLact.GetComponent<Image>().enabled = false;
			Haaparousku++;

			if (haapa == true)
			{
				multiplier += .1f;
				haapa = false;
				multiplierSet();
			}
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
			SiluettiPenny.GetComponent<Image>().enabled = false;
			
			if (herkku == true)
			{
				multiplier += .1f;
				herkku = false;
				multiplierSet();
			}

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

		else if (other.gameObject.CompareTag("Kangasrousku"))
		{
			GameObject.Find("SieniPickupKangas").GetComponent<Text>().enabled = true;
			GameObject.Find("SieniPickupKangas").GetComponent<Animator>().enabled = true;
			SiluettiRufus.GetComponent<Image>().enabled = false;
			Debug.Log("Herkkutatti noukittu!");
			Destroy(other.gameObject);
			score = score + 40;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 0.1f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			StartCoroutine(SieniPysäytysKangas());
			Kangasrousku++;

			if (kangas == true)
			{
				multiplier += .1f;
				kangas = false;
				multiplierSet();
			}
		}

		else if (other.gameObject.CompareTag("Isohapero"))
		{
			GameObject.Find("SieniPickupIso").GetComponent<Text>().enabled = true;
			GameObject.Find("SieniPickupIso").GetComponent<Animator>().enabled = true;
			SiluettiRussu.GetComponent<Image>().enabled = false;
			Debug.Log("Herkkutatti noukittu!");
			Destroy(other.gameObject);
			score = score + 35;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 0.1f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			StartCoroutine(SieniPysäytysIso());
			Isohapero++;

			if (iso == true)
			{
				multiplier += .1f;
				iso = false;
				multiplierSet();
			}
		}

		else if (other.gameObject.CompareTag("Haavapunikkitatti"))
		{
			GameObject.Find("SieniPickupHaava").GetComponent<Text>().enabled = true;
			GameObject.Find("SieniPickupHaava").GetComponent<Animator>().enabled = true;
			SiluettiLecci.GetComponent<Image>().enabled = false;
			Debug.Log("Herkkutatti noukittu!");
			Destroy(other.gameObject);
			score = score + 65;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 0.1f;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			StartCoroutine(SieniPysäytysHaava());
			Haavapunikkitatti++;

			if (haava == true)
			{
				multiplier += .1f;
				haava = false;
				multiplierSet();
			}
		}


	}

	public void SetCountText()
	{
		countText.text = "Score: " + score;
	}

	public void multiplierSet()
	{
		multiText.text = " X " + multiplier;
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

	IEnumerator SieniPysäytysKangas()        //Pysäyttää hahmon hetkeksi kun poimii sienen.
	{
		yield return new WaitForSeconds(0.2f);
		GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 8f;
		yield return new WaitForSeconds(1.6f);

		anim.Play("SieniAnimation", 0, 0);
		GameObject.Find("SieniPickupKangas").GetComponent<Text>().enabled = false;
		GameObject.Find("SieniPickupKangas").GetComponent<Animator>().enabled = false;

	}

	IEnumerator SieniPysäytysHaava()        //Pysäyttää hahmon hetkeksi kun poimii sienen.
	{
		yield return new WaitForSeconds(0.2f);
		GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 8f;
		yield return new WaitForSeconds(1.6f);

		anim.Play("SieniAnimation", 0, 0);
		GameObject.Find("SieniPickupHaava").GetComponent<Text>().enabled = false;
		GameObject.Find("SieniPickupHaava").GetComponent<Animator>().enabled = false;

	}

	IEnumerator SieniPysäytysIso()        //Pysäyttää hahmon hetkeksi kun poimii sienen.
	{
		yield return new WaitForSeconds(0.2f);
		GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 8f;
		yield return new WaitForSeconds(1.6f);

		anim.Play("SieniAnimation", 0, 0);
		GameObject.Find("SieniPickupIso").GetComponent<Text>().enabled = false;
		GameObject.Find("SieniPickupIso").GetComponent<Animator>().enabled = false;

	}

	IEnumerator SieniPysäytysKavala()        //Pysäyttää hahmon hetkeksi kun poimii sienen.
	{
		yield return new WaitForSeconds(0.2f);
		GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed = 8f;
		yield return new WaitForSeconds(1.6f);

		anim.Play("SieniAnimation", 0, 0);
		GameObject.Find("SieniPickupKavala").GetComponent<Text>().enabled = false;
		GameObject.Find("SieniPickupKavala").GetComponent<Animator>().enabled = false;

	}

}
	
