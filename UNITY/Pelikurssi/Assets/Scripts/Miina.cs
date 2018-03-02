//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class Miina : MonoBehaviour {

	public GameObject OsumaEfekti;
	public GameObject Mesh;
	public AudioClip Räjähdys;
	public AudioSource Rajahdys;
	

	// Use this for initialization
	void Start () {
		Rajahdys.clip = Räjähdys;
	}
	
	// Update is called once per frame
	void Update () {

		
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			GameObject.Find("Alusta2").GetComponent<AudioSource>().enabled = false;
			Rajahdys.Play();			
			Instantiate(OsumaEfekti, transform.position, transform.rotation);
			Debug.Log("PAM!");
			Destroy(GameObject.Find("Pelaaja"));
			MeshRenderer m = Mesh.GetComponent<MeshRenderer>();
			m.enabled = false;
			GameObject.Find("Main Camera").GetComponent<ThirdPersonCamera>().enabled=false;
			Debug.Log("Nyt pitäisi tulla Loppu");


			StartCoroutine(LoppuLataus());




		}
	}



	IEnumerator LoppuLataus()
	{
		Debug.Log("Toimiiko odotus?");
		yield return new WaitForSeconds(3.5f);
		Debug.Log("Loppu ladattu");
		SceneManager.LoadScene("Loppu");
		

	}
	

	
}
