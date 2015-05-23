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

	public EnvironmentData winterEnvironment;
	public EnvironmentData springEnvironment;
//	public EnvironmentData summerEnvironment; 

	EnvironmentData currentSetEnvironment = null;


	List<EnvironmentData> environments = new List<EnvironmentData>();
	// Use this for initialization
	void Start () 
	{
		environments.Add (winterEnvironment);
		environments.Add (springEnvironment);
//		environments.Add (summerEnvironment);

		environments.Sort ((e, f) => e.environment.position.y.CompareTo(f.environment.position.y));

		foreach (EnvironmentData e in environments)
		{
			e.effectObject.Stop();
			e.effectWind.gameObject.SetActive(false);
		}
	}


	// Update is called once per frame
	void Update () 
	{
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
			RenderSettings.skybox = currentEnvironment.skybox;
			
			// fade out currentSetEnvironment
			if (currentSetEnvironment != null)
			{
				currentSetEnvironment.effectObject.Stop();
				currentSetEnvironment.effectWind.gameObject.SetActive(false);

			}

			// fade in currentEnvironment
			
			currentEnvironment.effectObject.Play();
			currentEnvironment.effectWind.gameObject.SetActive(true);


			// set new
			currentSetEnvironment = currentEnvironment;
		}
	}
}








