//Spagettikoodi a'la Veni
//Use at your own risk. :-D


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tattitesti : MonoBehaviour
{
	public Text countText;
	public int score;



	// Use this for initialization
	void Start()
	{
		SetCountText();

	}

	// Update is called once per frame
	void Update()
	{
		
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))          // Sienen piilottaminen kun siihen osuu.
		{
			Debug.Log("Herkkutatti noukittu!");
			gameObject.SetActive(false);

			GameObject.Find("SieniNoukinta").GetComponent<SieniNoukinta>().score -= 100;

			

			



		}

		
	}

	void SetCountText()
	{
		countText.text = "Score: " + score;


	}

}


