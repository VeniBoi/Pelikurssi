using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Miina : MonoBehaviour {

	public GameObject OsumaEfekti;
	public GameObject Mesh;
	public GameObject KameraStop;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			Instantiate(OsumaEfekti, transform.position, transform.rotation);
			Debug.Log("PAM!");
			Destroy(GameObject.Find("Pelaaja"));
			MeshRenderer m = Mesh.GetComponent<MeshRenderer>();
			m.enabled = false;
			KameraStop.GetComponent<ThirdPersonCamera>().enabled = false;

			StartCoroutine(LoppuLataus());

			

			
		}
	}



	IEnumerator LoppuLataus()
	{

		yield return new WaitForSeconds(3.5f);
		SceneManager.LoadScene("Loppu");
		

	}
	

	
}
