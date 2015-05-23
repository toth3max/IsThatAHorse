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

	// Use this for initialization
	void Start () {
		animator = catSprite.GetComponent<Animator>();
	}

	private bool onGroundForward;
	private bool onGroundDown;
	private bool onGround { get { return onGroundDown || onGroundForward; } }

	public int hackyFrameWait = 5;

	float yVelocity = 0;
	Vector3 closestHitPoint = Vector3.zero;
	Vector3 oppositeSide = Vector3.zero;
	GameObject closestForwardObject = null;
	GameObject downObject = null;
	bool wasGoingDown;


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

		IsGrounded(cameraPosition, out onGroundDown, out onGroundForward);

		CheckTeleport(cameraPosition);

		ApplyVerticalMovement(onGround);


		CheckHorizontalMovement(cameraPosition);

		RealignCamera();
	}

	void ApplyVerticalMovement(bool grounded) {
		if (grounded)
		{
			// Remove gravity
			yVelocity = 0;
			animator.SetBool("isJumping", false); 
			animator.SetBool("isFalling", false);

			if (Input.GetButton("Jump")) {
				yVelocity = jumpVelocity;
			}
		}
		else
		{

			// apply gravity	
			yVelocity -= gravity*Time.deltaTime;

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

			RaycastHit downHit;
			Debug.DrawRay(feetGround + Vector3.up*groundCheckDistance, Vector3.down*groundCheckDistance, Color.blue);
			if (Physics.Raycast(feetGround + Vector3.up*groundCheckDistance, Vector3.down, out downHit, groundCheckDistance, groundLayers.value))
			{
				downObject = downHit.collider.gameObject;
				transform.position = new Vector3(transform.position.x, downHit.point.y, transform.position.z);
				isGroundedDown = true;
			}

			// grounded forwards
			Vector3 direction = feetGround - cameraPosition;
			
			Debug.DrawRay(cameraPosition, direction*10, Color.red);
			Debug.DrawRay(cameraPosition + Vector3.up*groundCheckDistance, direction*10, Color.green);
			
			RaycastHit [] hits = Physics.RaycastAll(cameraPosition, direction, direction.magnitude*10, groundLayers.value);
			RaycastHit [] upperHits = Physics.RaycastAll(cameraPosition+Vector3.up*groundCheckDistance, direction, direction.magnitude*10, groundLayers.value);

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
					oppositeSide = cameraPosition + direction*2;
				}
				isGroundedForward = true;
			}

		}
	}

	void CheckTeleport(Vector3 cameraPosition) {
		// teleport in 'z' direction toclosest object
		// only teleport when moving downwards and not grounded
		Debug.Log (onGroundForward+" "+wasGoingDown+" "+downObject);
		if ((onGroundForward && wasGoingDown) || downObject == null)
		{
			Debug.Log ("closestForwardObject "+closestForwardObject);
			if (closestForwardObject != null)
			{
				Vector3 rayDirection = oppositeSide - cameraPosition;
				RaycastHit [] oppositeHits = Physics.RaycastAll(oppositeSide, -rayDirection, rayDirection.magnitude, groundLayers);
				RaycastHit [] forwardHits = Physics.RaycastAll(cameraPosition, rayDirection, rayDirection.magnitude, groundLayers);
				
				
				int oppositeHitIndex = System.Array.FindIndex(oppositeHits, (h) => h.collider.gameObject == closestForwardObject);
				int forwardHitIndex = System.Array.FindIndex(forwardHits, (h) => h.collider.gameObject == closestForwardObject);
				
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

				Debug.Log (closestForwardObject);

				Vector3 centerDirection = GetCenterDirection(cameraPosition);

				Vector3 playerSphericalPosition = centerDirection.normalized;
				transform.position = cameraPosition + playerSphericalPosition * targetPlayerPosition;
				
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






