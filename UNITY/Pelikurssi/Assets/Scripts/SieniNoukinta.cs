using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SieniNoukinta : MonoBehaviour
{

	private int score;
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

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("PickUp"))          // Sienen piilottaminen kun siihen osuu.
		{
			Debug.Log("Sieni noukittu!");
			other.gameObject.SetActive(false);
			score = score + 10;
			SetCountText();
		}

	}
	void SetCountText()
	{
		countText.text = "Score: " + score;


	}
}
	
