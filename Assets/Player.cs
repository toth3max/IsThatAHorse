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

		Vector3 closestHitPoint = Vector3.zero;
		Vector3 oppositeSide = Vector3.zero;
		GameObject closestObject = null;

		bool hitGround = false;

		int groundIntervals = 5;
		for (int i = 0 ; i < groundIntervals ; i++)
		{
			Vector3 feetGround = Vector3.Lerp(leftFoot.position, rightFoot.position, (float)i/(groundIntervals-1f));

			Vector3 direction = feetGround - cameraPosition;


			RaycastHit [] hits = Physics.RaycastAll(cameraPosition, direction, direction.magnitude*2, groundLayers.value);

			Debug.DrawRay(cameraPosition, direction*2, Color.red);

			if (hits.Length > 0)
			{
				float distance = Vector3.Distance(hits[0].point, cameraPosition);
				float closestDistance = Vector3.Distance(closestHitPoint, cameraPosition);

				if (distance < closestDistance || closestObject == null)
				{
					closestHitPoint = hits[0].point;
					closestObject = hits[0].collider.gameObject;
					oppositeSide = cameraPosition + direction*2;
				}
				hitGround = true;
			}
		}

		onGround = hitGround;
		bool wasGoingDown = yVelocity < 0;
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


		float leftMove = 0;
		if (Input.GetKey(KeyCode.A))
		{
			leftMove += 1;
		}
		if (Input.GetKey(KeyCode.D))
		{
			leftMove -= 1;
		}
//		Debug.Log(leftMove);

		// left right controls
		Vector3 centerFoot = Vector3.Lerp(leftFoot.position, rightFoot.position, 0.5f);
		
		Vector3 centerDirection = centerFoot - cameraPosition;
		centerDirection.Normalize();
		Vector3 rightVector = Vector3.Cross(Vector3.up, centerDirection);

		transform.position -= leftMove*rightVector*moveSpeed*Time.deltaTime;

		
		// teleport in 'z' direction toclosest object
		// only teleport when moving downwards and not grounded
		if (onGround && wasGoingDown)
		{
			if (closestObject != null)
			{
				Vector3 rayDirection = oppositeSide - cameraPosition;
				RaycastHit [] oppositeHits = Physics.RaycastAll(oppositeSide, -rayDirection, rayDirection.magnitude, groundLayers);
				RaycastHit [] forwardHits = Physics.RaycastAll(cameraPosition, rayDirection, rayDirection.magnitude, groundLayers);


				int oppositeHitIndex = System.Array.FindIndex(oppositeHits, (h) => h.collider.gameObject == closestObject);
				int forwardHitIndex = System.Array.FindIndex(forwardHits, (h) => h.collider.gameObject == closestObject);
				
				if (oppositeHitIndex == -1)
					Debug.LogError("Did not raycast a hit in the opposite direciton, IMPOSSIBRU!");

				if (forwardHitIndex == -1)
					Debug.LogError("Did not raycast a hit in the foraward direciton, IMPOSSIBRU!");
				
				Vector3 farHit = oppositeHits[oppositeHitIndex].point;
				Vector3 closeHit = forwardHits[forwardHitIndex].point;

				
				float closeHitDistance = Vector3.Distance(closeHit, cameraPosition);
				float farHitDistance   = Vector3.Distance(farHit, cameraPosition);

				float playerDistance   = Vector3.Distance(transform.position, cameraPosition);

				Debug.Log (closeHitDistance +" : "+playerDistance+" : "+farHitDistance);
				float targetPlayerPosition = Mathf.Clamp(playerDistance, closeHitDistance, farHitDistance);

				// TODO target distances is WRONG!

				Debug.Log (closestObject);
				Vector3 playerSphericalPosition = centerDirection.normalized;
				transform.position = cameraPosition + playerSphericalPosition * targetPlayerPosition;
				
				Debug.DrawRay(cameraPosition, transform.position, Color.red);
			}
		}


		// adjust camera position to player position
		RotateCamera.instance.transform.position = transform.position;
		RotateCamera.instance.Height = transform.position.y;
	}
}






