﻿using UnityEngine;
using System.Collections;

public class RotateCamera : MonoBehaviour 
{
	public static RotateCamera instance;

	void Awake()
	{
		instance = this;
		
		if (nonOculusCamera.gameObject.activeSelf != !useOculusForControls)
			nonOculusCamera.gameObject.SetActive(!useOculusForControls);
		 	
		if (oculusControls.gameObject.activeSelf != useOculusForControls)
			oculusControls.gameObject.SetActive(useOculusForControls);
	}

//	public float distance = 60;

	public Transform centerOculus;
	public Transform trackingCenter;


	
	[Header("Oculus Eyes")]
//	public float targetFOV = 30;
	public Camera leftEye;
	public Camera rightEye;

	[Header("Non oculus mode")]
	public bool useOculusForControls = true;

	public float nonOculusRotationSpeed = 30;
	public Camera nonOculusCamera;
	public GameObject oculusControls;
	
	public float rotationFactor = 2;

	// Use this for initialization
	void Start () 
	{


		
	}
	
	public float yDirection { get; private set;}

	public Vector3 _targetPosition;
	public Vector3 TargetPosition 
	{ 
		get
		{
			return _targetPosition;
		}
		set
		{
			_targetPosition = value;

			Vector3 relativePosition = _targetPosition;
			relativePosition.y = 0;

			Vector3 right = transform.right;

			float angle = Vector3.Angle(transform.forward, relativePosition);

//			Debug.Log (angle);

			float offset = relativePosition.magnitude * Mathf.Cos(angle*Mathf.Deg2Rad);

			transform.position = _targetPosition - offset*transform.forward;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		float yDirectionOffset = 0;
		if (useOculusForControls)
		{
			Vector3 flatForward = centerOculus.transform.localRotation.eulerAngles;

			float targetYDirection = flatForward.y*rotationFactor;
			yDirection = Mathf.LerpAngle(yDirection, targetYDirection, 0.33f);
			yDirectionOffset = yDirection - targetYDirection;

//			leftEye.fieldOfView = targetFOV;
//			rightEye.fieldOfView = targetFOV;
		}
		else
		{
			if (Input.GetButton("PanLeft"))
			{
				yDirection += nonOculusRotationSpeed*Time.deltaTime;
			}
			if (Input.GetButton("PanRight"))
			{
				yDirection -= nonOculusRotationSpeed*Time.deltaTime;
			}
		}
		
		Debug.DrawRay(transform.position, Quaternion.AngleAxis(yDirection, Vector3.up)*Vector3.forward * 100, Color.red);
		
		transform.localRotation = Quaternion.Euler(0, yDirection, 0);
		
		trackingCenter.localRotation = Quaternion.Euler(0, -(yDirection - yDirectionOffset)/rotationFactor, 0);
	}
}
