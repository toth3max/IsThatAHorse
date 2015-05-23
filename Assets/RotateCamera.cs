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


	public GameObject oculusControls;

	[Header("Non oculus mode")]
	public bool useOculusForControls = true;

	public float nonOculusRotationSpeed = 30;
	public Camera nonOculusCamera;
	


	// Use this for initialization
	void Start () 
	{
		
		nonOculusCamera.gameObject.SetActive(!useOculusForControls);
		oculusControls.gameObject.SetActive(useOculusForControls);

		
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

		if (useOculusForControls)
		{
			Vector3 flatForward = centerOculus.transform.localRotation.eulerAngles;

			yDirection = flatForward.y;
		}
		else
		{
			if (Input.GetKey(KeyCode.Q))
			{
				yDirection += nonOculusRotationSpeed*Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.E))
			{
				yDirection -= nonOculusRotationSpeed*Time.deltaTime;
			}
		}
		
		Debug.DrawRay(transform.position, Quaternion.AngleAxis(yDirection, Vector3.up)*Vector3.forward * 100, Color.red);
		
		transform.localRotation = Quaternion.Euler(0, yDirection, 0);
		
		trackingCenter.localRotation = Quaternion.Euler(0, -yDirection, 0);
	}
}
