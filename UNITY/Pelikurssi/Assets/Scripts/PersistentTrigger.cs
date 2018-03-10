using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentTrigger : MonoBehaviour
{

	public static PersistentTrigger instance { get; private set; }

	public int value;

	// Use this for initialization
	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}

	}

	// Update is called once per frame
	void Update()
	{

	}
}