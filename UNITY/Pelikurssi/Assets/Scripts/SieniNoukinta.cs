//Spagettikoodi a'la Veni
//Use at your own risk. :-D


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SieniNoukinta : MonoBehaviour
{

	public int score;
	public Text countText;

	// Use this for initialization
	void Start()
	{
		score = 0;                       //Asetetaan pistemäärä nollaan.
		SetCountText();
	}

	// Update is called once per frame
	void Update()
	{
	}

	void OnTriggerEnter(Collider other )
	{
		if (other.gameObject.CompareTag("Karpassieni"))          // Sienen piilottaminen kun siihen osuu.
		{
			Debug.Log("Kärpässieni noukittu!");
            Destroy(other.gameObject);
            score = score + 10;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
			

		}

		else if (other.gameObject.CompareTag("Keltavahvero"))
		{
			Debug.Log("Kanttarelli noukittu!");
            Destroy(other.gameObject);
            score = score + 15;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
		}

		else if (other.gameObject.CompareTag("Haaparousku"))
		{
			Debug.Log("Haaparousku noukittu!");
            Destroy(other.gameObject);
            score = score + 30;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
		}

		else if (other.gameObject.CompareTag("Herkkutatti"))
		{
			Debug.Log("Herkkutatti noukittu!");
            Destroy(other.gameObject);
            score = score + 50;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
		}

		else if (other.gameObject.CompareTag("PsiloSieni"))
		{
			Debug.Log("Psilosieni noukittu!");
            Destroy(other.gameObject);
            score = score + 5000;
			SetCountText();
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().runSpeed += 10;
			GameObject.Find("Pelaaja").GetComponent<PlayerController>().animator.Play("Noukkiminen");
		}


	}

	void SetCountText()
	{
		countText.text = "Score: " + score;


	}

}
	
