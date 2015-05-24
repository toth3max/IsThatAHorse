﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour 
{
	[System.Serializable]
	public class EnvironmentData
	{
		public Transform environment;
		public ParticleSystem effectObject;
		public WindZone effectWind;
		public Material skybox;
		public AudioSource audio;
	}

	public Player player;

	public EnvironmentData winterEnvironment;
	public EnvironmentData springEnvironment;
	public EnvironmentData summerEnvironment; 
	public EnvironmentData autumnEnvironment; 

	EnvironmentData currentSetEnvironment = null;


	List<EnvironmentData> environments = new List<EnvironmentData>();
	// Use this for initialization
	void Start () 
	{
		environments.Add (winterEnvironment);
		environments.Add (springEnvironment);
		environments.Add (summerEnvironment);
		environments.Add (autumnEnvironment);

		environments.Sort ((e, f) => e.environment.position.y.CompareTo(f.environment.position.y));

		foreach (EnvironmentData e in environments)
		{
			e.effectObject.Stop();
			e.effectWind.gameObject.SetActive(false);

			e.audio.volume = 0;
			e.audio.Play();
		}
	}


	// Update is called once per frame
	void Update () 
	{
		if (player.transform.position.y < -50)
			player.Reset();

		EnvironmentData currentEnvironment = null;

		foreach (EnvironmentData e in environments)
		{
			if (RotateCamera.instance.TargetPosition.y > e.environment.position.y-1)
			{
				currentEnvironment = e;
//				Debug.Log ("above "+currentEnvironment.environment.name);
			}
//			Debug.Log (e.environment.position.y);
		}

//		Debug.Log ("--- "+currentEnvironment != null ? currentEnvironment.environment.name : "null");

		if (currentEnvironment != null && currentEnvironment != currentSetEnvironment)
		{
			StopAllCoroutines();
//			RenderSettings.skybox = currentEnvironment.skybox;

			// fade out currentSetEnvironment
			if (currentSetEnvironment != null)
			{
				StartCoroutine(FadeSkybox(0.66f, currentSetEnvironment.skybox, currentEnvironment.skybox));

				currentSetEnvironment.effectObject.Stop();
				currentSetEnvironment.effectWind.gameObject.SetActive(false);
//				FadeOutMusic();
			}
			else
			{
				foreach (string propName in propNames)
					RenderSettings.skybox.SetTexture(propName, currentEnvironment.skybox.GetTexture(propName));
				RenderSettings.skybox.SetFloat("_Fade", 0);
			}

			// fade in currentEnvironment
			
			currentEnvironment.effectObject.Play();
			currentEnvironment.effectWind.gameObject.SetActive(true);
//			QueueMusic(currentEnvironment.audio);

			foreach (EnvironmentData e in environments)
			{
				StartCoroutine(FadeAudioSource(0.66f, e.audio, e == currentEnvironment ? 1 : 0));
			}

//			StopCoroutine("CrossFadeMusic");
			
//			if (isFading)
//			{
//				StopCoroutine("CrossFadeMusic");
//				
//				CleanUpMusic();
//				isFading = false;
//			}
//			StartCoroutine("CrossFadeMusic");

			// set new
			currentSetEnvironment = currentEnvironment;
		}
	}

	string [] propNames = new string[] {"_FrontTex", "_BackTex", "_LeftTex", "_RightTex", "_UpTex", "_DownTex"};


	IEnumerator FadeSkybox(float time, Material from, Material to)
	{
		foreach (string propName in propNames)
		{
			RenderSettings.skybox.SetTexture(propName, from.GetTexture(propName));
			RenderSettings.skybox.SetTexture(propName+"2", to.GetTexture(propName));
		}

		float t = 0;
		while (t < time)
		{
			RenderSettings.skybox.SetFloat("_Fade", t/time);
			t += Time.deltaTime;
			yield return null;
		}
		
		RenderSettings.skybox.SetFloat("_Fade", 1);
	}

	IEnumerator FadeAudioSource(float time, AudioSource audio, float volume)
	{
		Debug.Log ("Fading audio "+audio.volume +" => "+ volume);
		float origVol = audio.volume;
		float t = 0;
		while (t < time)
		{
			audio.volume = Mathf.Lerp(origVol, volume, t/time);
			t += Time.deltaTime;
			yield return null;
		}
		audio.volume = volume;
	}

//	private AudioSource previousAudioSource;
//	private AudioSource currentAudioSource;
//	void FadeOutMusic() {
//		previousAudioSource = currentAudioSource;
//		currentAudioSource = null;
//	}
//	void QueueMusic(AudioSource audio) {
//		currentAudioSource = Instantiate<AudioSource>(audio);
//		currentAudioSource.volume = 0f;
//		currentAudioSource.Play ();
//	}
//
//
//	bool isFading = false;
//	
//	private IEnumerator CrossFadeMusic()
//	{
//		isFading = true;
//		float fTimeCounter = 0f;
//		
//		while(!(Mathf.Approximately(fTimeCounter, 1f)))
//		{
//			fTimeCounter = Mathf.Clamp01(fTimeCounter + Time.deltaTime);
//			if(previousAudioSource != null) 
//			{
//				previousAudioSource.volume = 1f - fTimeCounter;
//			}
//			currentAudioSource.volume = fTimeCounter;
//			yield return new WaitForSeconds(0.02f);
//		}
//
//		CleanUpMusic();
//
//		isFading = false;
//	}
//
//	void CleanUpMusic()
//	{
//		if(previousAudioSource != null) 
//		{
//			previousAudioSource.Stop ();
//			Destroy(previousAudioSource.gameObject);
//			previousAudioSource = null;
//		}
//	}
}







