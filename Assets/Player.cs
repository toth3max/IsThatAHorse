using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
	public Transform leftFoot;
	public Transform rightFoot;

	public LayerMask groundLayers;

	public float jumpVelocity = 15;
	public float gravity = 20;
	public float moveSpeed = 20;

	// Use this for initialization
	void Start () {
	
	}

	public bool onGround { get; private set;}

	public int hackyFrameWait = 5;

	float yVelocity = 0;

	// Update is called once per frame
	void Update () 
	{
		if (hackyFrameWait > 0)
		{
			hackyFrameWait -= 1;
			return;
		}

		transform.rotation = Quaternion.Euler(0, RotateCamera.instance.yDirection, 0);

		
		Vector3 cameraPosition = RotateCamera.instance.centerOculus.position;

		bool hitGround = false;

		int groundIntervals = 5;
		for (int i = 0 ; i < groundIntervals ; i++)
		{
			Vector3 feetGround = Vector3.Lerp(leftFoot.position, rightFoot.position, (float)i/(groundIntervals-1f));

			Vector3 direction = feetGround - cameraPosition;

			Debug.DrawRay(cameraPosition, direction*2, Color.red);

			RaycastHit [] hits = Physics.RaycastAll(cameraPosition, direction, direction.magnitude*2, groundLayers.value);

			if (hits.Length > 0)
			{
				hitGround = true;
				break ;// HACK
			}
//			Debug.Log (i+": "+hits.Length);
		}

		onGround = hitGround;
		if (hitGround)
		{
			// DESTROY GRAVITY
			yVelocity = 0;

			// jump
			if (Input.GetKeyDown(KeyCode.Space))
			{
				yVelocity = jumpVelocity;
			}
		}
		else
		{
			// apply gravity	
		    yVelocity -= gravity*Time.deltaTime;
		}

		transform.position += Vector3.up*yVelocity*Time.deltaTime;

		// Read the horizontal movement
		float horizontal = Input.GetAxisRaw ("Horizontal");
		
		Vector3 centerFoot = Vector3.Lerp(leftFoot.position, rightFoot.position, 0.5f);
		
		Vector3 centerDirection = centerFoot - cameraPosition;
		centerDirection.Normalize();
		Vector3 rightVector = Vector3.Cross(Vector3.up, centerDirection);

		transform.position += horizontal*rightVector*moveSpeed*Time.deltaTime;

		RotateCamera.instance.transform.position = transform.position;
		RotateCamera.instance.Height = transform.position.y;
	}
}






