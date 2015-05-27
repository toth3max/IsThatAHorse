using UnityEngine;
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
	public EnvironmentData startEnvironment; 

	EnvironmentData currentSetEnvironment = null;


	List<EnvironmentData> environments = new List<EnvironmentData>();
	// Use this for initialization
	void Start () 
	{
		environments.Add (winterEnvironment);
		environments.Add (springEnvironment);
		environments.Add (summerEnvironment);
		environments.Add (autumnEnvironment);
		if (startEnvironment.environment != null)
			environments.Add (startEnvironment);

		environments.Sort ((e, f) => e.environment.position.y.CompareTo(f.environment.position.y));

		foreach (EnvironmentData e in environments)
		{
			if (e.effectWind != null)
			{
				e.effectObject.Stop();

				e.effectWind.gameObject.SetActive(false);
			}

			e.audio.volume = 0;
			e.audio.Play();
		}
	}


	// Update is called once per frame
	void Update () 
	{
		if (Input.GetButton ("ResetGame")) { 
			Application.LoadLevel(Application.loadedLevel);
		} 

		if (player.transform.position.y < environments[0].environment.transform.position.y - 50)
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

				if (currentSetEnvironment.effectObject != null)
					currentSetEnvironment.effectObject.Stop();
				if (currentSetEnvironment.effectWind != null)
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
			
			if (currentEnvironment.effectObject != null)
				currentEnvironment.effectObject.Play();
			if (currentEnvironment.effectWind != null)
				currentEnvironment.effectWind.gameObject.SetActive(true);
//			QueueMusic(currentEnvironment.audio);

			Dictionary<AudioSource, float> targetValue = new Dictionary<AudioSource, float>();


			foreach (EnvironmentData e in environments)
			{
				if (!targetValue.ContainsKey(e.audio))
					targetValue[e.audio] = 0;
				targetValue[e.audio] = Mathf.Max(targetValue[e.audio], currentEnvironment == e ? 1 : 0);
			}

			
			foreach (AudioSource a in targetValue.Keys)
				StartCoroutine(FadeAudioSource(0.66f, a, targetValue[a]));


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
			RenderSettings.skybox.SetColor("_Tint", Color.Lerp(from.GetColor("_Tint"), to.GetColor("_Tint"), t/time));
			t += Time.deltaTime;
			yield return null;
		}
		
		RenderSettings.skybox.SetFloat("_Fade", 1);
		RenderSettings.skybox.SetColor("_Tint", to.GetColor("_Tint"));
	}

	IEnumerator FadeAudioSource(float time, AudioSource audio, float volume)
	{
//		Debug.Log ("Fading audio "+audio.volume +" => "+ volume);
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







