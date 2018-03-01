using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MasterVolume : MonoBehaviour {

	public AudioMixer audioMixer;

	public void VolumeSaato(float volume)
	{
		audioMixer.SetFloat("volume", volume);
	}
}
