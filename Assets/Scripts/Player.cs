using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
	public Transform leftFoot;
	public Transform rightFoot;
	public GameObject catSprite;

	public LayerMask groundLayers;

	public float jumpVelocity = 15;
	public float gravity = 20;
	public float moveSpeed = 20;

	public AudioSource teleportSound;

	public float groundCheckDistance = 0.1f;
	private Animator animator;

	private Vector3 originalCatLocalPosition = Vector3.zero;

	// Use this for initialization
	void Start () {
		lastKnownGoodPosition = transform.position;
		
		animator = catSprite.GetComponent<Animator>();
		originalCatLocalPosition = catSprite.transform.localPosition;
	}

	private bool onGroundForward;
	private bool onGroundDown;
	private bool onGround { get { return onGroundDown || onGroundForward; } }
	private Vector3 lastKnownGoodPosition = new Vector3 (2, 2, 2);

	public int hackyFrameWait = 5;

	float yVelocity = 0;
	Vector3 closestHitPoint = Vector3.zero;
	Vector3 oppositeSide = Vector3.zero;
	GameObject closestForwardObject = null;
	GameObject downObject = null;
	bool wasGoingDown;


	void FixedUpdate()
	{
		
		Vector3 cameraPosition = RotateCamera.instance.centerOculus.position;

		IsGrounded(cameraPosition, out onGroundDown, out onGroundForward);
		
		CheckTeleport(cameraPosition);
		
		ApplyVerticalMovement(onGround);

	}

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
		
		wasGoingDown = yVelocity < 0;



		CheckHorizontalMovement(cameraPosition);

		RealignCamera();
		CheckReset ();

		catSprite.transform.localPosition = Vector3.Lerp(catSprite.transform.localPosition, originalCatLocalPosition, 0.1f);
		catSprite.transform.localPosition = new Vector3(catSprite.transform.localPosition.x, originalCatLocalPosition.y, catSprite.transform.localPosition.z);
	}

	void CheckReset() {
		if (Input.GetButton ("ResetPlayer")) {
			Reset();
		}
	}

	public void Reset()
	{
		transform.position = lastKnownGoodPosition;
		yVelocity = 0;
	}

	void SaveGroundedPosition() {
		if(transform.position.y > lastKnownGoodPosition.y) {
			lastKnownGoodPosition = transform.position;
		}
	}

	void ApplyVerticalMovement(bool grounded) {
		if (grounded)
		{
			SaveGroundedPosition();
			// Remove gravity
			yVelocity = 0;
			animator.SetBool("isJumping", false); 
			animator.SetBool("isFalling", false);

			if (downObject != null && downObject.GetComponent<JumpTrigger>() != null)
			{
				yVelocity = downObject.GetComponent<JumpTrigger>().jumpVelocity;
			}
			else if (Input.GetButton("Jump")) {
				yVelocity = jumpVelocity;
			}
		}
		else
		{

			// apply gravity	
			yVelocity = yVelocity <= -gravity ? -gravity : yVelocity - gravity*Time.deltaTime;

			animator.SetBool("isWalking", false);
			if (yVelocity < 0f) {
				animator.SetBool("isFalling", true); 
				animator.SetBool("isJumping", false);
				//Debug.Log ("falling: " + yVelocity);
			}
			if (yVelocity > 0f) {
				animator.SetBool("isJumping", true); 
				animator.SetBool("isFalling", false); 
				//Debug.Log ("jumping: " + yVelocity);
			}
		}

		transform.position += Vector3.up*yVelocity*Time.deltaTime;
	}

	void RealignCamera() {
		RotateCamera.instance.TargetPosition = transform.position;
	}

	void IsGrounded(Vector3 cameraPosition, out bool isGroundedDown, out bool isGroundedForward) 
	{
		int groundIntervals = 5;
		isGroundedForward = false;
		isGroundedDown = false;


		closestForwardObject = null;
		downObject = null;
		for (int i = 0 ; i < groundIntervals ; i++)
		{
			Vector3 feetGround = Vector3.Lerp(leftFoot.position, rightFoot.position, (float)i/(groundIntervals-1f));
			// grounded downwards

			
			if (yVelocity <= 0) // only check for downward objects if going down
			{
				RaycastHit downHit;
				Debug.DrawRay(feetGround + Vector3.up*groundCheckDistance, Vector3.down*groundCheckDistance, Color.blue);
				if (Physics.Raycast(feetGround + Vector3.up*groundCheckDistance, Vector3.down, out downHit, groundCheckDistance, groundLayers.value))
				{
					downObject = downHit.collider.gameObject;
					transform.position = new Vector3(transform.position.x, downHit.point.y, transform.position.z);
					isGroundedDown = true;
				}
			}

			// grounded forwards
			Vector3 direction = feetGround - cameraPosition;

			float directionMultiplier = 100;

			Debug.DrawRay(cameraPosition, direction*10, Color.red);
			Debug.DrawRay(cameraPosition + Vector3.up*groundCheckDistance, direction*directionMultiplier, Color.green);
			
			RaycastHit [] hits = Physics.RaycastAll(cameraPosition, direction, direction.magnitude*directionMultiplier, groundLayers.value);
			RaycastHit [] upperHits = Physics.RaycastAll(cameraPosition+Vector3.up*groundCheckDistance, direction, direction.magnitude*directionMultiplier, groundLayers.value);

			bool hitOnGround = hits.Length > 0;
			bool hitAboveGround = upperHits.Length > 0;

			if (hitOnGround && !hitAboveGround && yVelocity < 0)
			{
//				Debug.Log("hitGround!");
				float distance = Vector3.Distance(hits[0].point, cameraPosition);
				float closestDistance = Vector3.Distance(closestHitPoint, cameraPosition);
				
				if (distance < closestDistance || closestForwardObject == null)
				{
					closestHitPoint = hits[0].point;
					closestForwardObject = hits[0].collider.gameObject;
					oppositeSide = cameraPosition + direction*directionMultiplier;
				}
				isGroundedForward = true;
			}

		}

	}

	void CheckTeleport(Vector3 cameraPosition) {
		// teleport in 'z' direction toclosest object
		// only teleport when moving downwards and not grounded
//		Debug.Log (onGroundForward+" "+wasGoingDown+" "+downObject);
		if ((onGroundForward && (wasGoingDown || yVelocity == 0)) || downObject == null)
		{
//			Debug.Log ("closestForwardObject "+closestForwardObject);
			if (closestForwardObject != null)
			{
				Vector3 rayDirection = oppositeSide - cameraPosition;
				RaycastHit [] oppositeHits = Physics.RaycastAll(oppositeSide, -rayDirection, rayDirection.magnitude, groundLayers);
				RaycastHit [] forwardHits = Physics.RaycastAll(cameraPosition, rayDirection, rayDirection.magnitude, groundLayers);
				
				
				int oppositeHitIndex = System.Array.FindIndex(oppositeHits, (h) => h.collider.gameObject == closestForwardObject);
				int forwardHitIndex = System.Array.FindIndex(forwardHits, (h) => h.collider.gameObject == closestForwardObject);

				Debug.DrawRay(oppositeSide, -rayDirection, Color.magenta);

				if (oppositeHitIndex == -1)
				{
					Debug.LogError("Did not raycast a hit in the opposite direciton, IMPOSSIBRU!");
					return;
				}
				
				if (forwardHitIndex == -1)
				{
					Debug.LogError("Did not raycast a hit in the foraward direciton, IMPOSSIBRU!");
					return;
				}
				
				Vector3 farHit = oppositeHits[oppositeHitIndex].point;
				Vector3 closeHit = forwardHits[forwardHitIndex].point;
				
				
				float closeHitDistance = Vector3.Distance(closeHit, cameraPosition);
				float farHitDistance   = Vector3.Distance(farHit, cameraPosition);
				
				float playerDistance   = Vector3.Distance(transform.position, cameraPosition);
				
//				Debug.Log (closeHitDistance +" : "+playerDistance+" : "+farHitDistance);
				float targetPlayerPosition = Mathf.Clamp(playerDistance, closeHitDistance, farHitDistance);

//				Debug.Log (closestForwardObject);

				Vector3 centerDirection = GetCenterDirection(cameraPosition);

				Vector3 playerSphericalPosition = centerDirection.normalized;

				Vector3 oldPosition = transform.position;
				transform.position = cameraPosition + playerSphericalPosition * targetPlayerPosition;
				catSprite.transform.position = oldPosition;

				Debug.DrawRay(cameraPosition, transform.position, Color.red);
				
				//steleportSound.Play();
			}
		}
	}

	Vector3 GetCenterDirection(Vector3 cameraPosition) {
		Vector3 centerFoot = Vector3.Lerp(leftFoot.position, rightFoot.position, 0.5f);
		
		Vector3 centerDirection = centerFoot - cameraPosition;
		centerDirection.Normalize();


		return centerDirection;
	}

	void CheckHorizontalMovement(Vector3 cameraPosition) {
		// Read the horizontal movement
		float horizontal = Input.GetAxisRaw ("Horizontal");

		// Flip player sprite based on movement
		if(horizontal < -0.4f) {
			this.transform.localScale = new Vector3(-1f, this.transform.localScale.y, this.transform.localScale.z);
			if(onGround) {
				animator.SetBool("isWalking", true);
			}
		}
		if(horizontal > 0.4f) {
			this.transform.localScale = new Vector3(1f, this.transform.localScale.y, this.transform.localScale.z);
			if(onGround) {
				animator.SetBool("isWalking", true);
			}
		}
		if(horizontal > -0.4f && horizontal < 0.4f) {
			animator.SetBool("isWalking", false);
		}

		Vector3 rightVector = Vector3.Cross(Vector3.up, GetCenterDirection(cameraPosition));
		
		transform.position += horizontal*rightVector*moveSpeed*Time.deltaTime;
	}
}






