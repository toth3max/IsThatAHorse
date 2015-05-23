using UnityEngine;
using System.Collections;

public class RotateCamera : MonoBehaviour 
{
	public static RotateCamera instance;

	void Awake()
	{
		instance = this;
	}

//	public float distance = 60;

	public Transform centerOculus;
	public Transform trackingCenter;

	


	// Use this for initialization
	void Start () 
	{
		
	}
	
	public float yDirection { get; private set;}
	public float Height 
	{ 
		get
		{
			return transform.position.y;
		}
		set
		{
			transform.position = new Vector3(transform.position.x, value, transform.position.z);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 flatForward = centerOculus.transform.localRotation.eulerAngles;

		yDirection = flatForward.y;

		Debug.DrawRay(transform.position, Quaternion.AngleAxis(flatForward.y, Vector3.up)*Vector3.forward * 100, Color.red);

		transform.localRotation = Quaternion.Euler(0, flatForward.y, 0);

		trackingCenter.localRotation = Quaternion.Euler(0, -flatForward.y, 0);
	}
}
