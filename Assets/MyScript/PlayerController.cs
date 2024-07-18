using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using SpriteRenderer = UnityEngine.SpriteRenderer;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;							// A position marking where to check for ceilings
	[SerializeField] private Collider2D m_CrouchDisableCollider;				// A collider that will be disabled when crouching
	[SerializeField] private Transform[] m_WallChecks;

	const float k_GroundedRadius = .1f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	private const float k_WalledRadius = .1f;
	private bool m_Walled;
	const float k_CeilingRadius = .1f; // Radius of the overlap circle to determine if the player can stand up
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;

	public float gravity = 20;
	
	[HideInInspector]
	public string playerName;

	[HideInInspector] public int playerIndex = 0;

	private float jumpRadiance = Mathf.PI/3;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;

	private bool m_wasCrouching = false;

	private GameObject groundObj;
	private GameObject wallObj;
	private GameObject candidateObj;
	private GameObject previousCandidateObj;

	private Animator playerAnimator;

	private AudioSource audioSource;

	public AudioClip stickAudio;
	public AudioClip jumpAudio;
	public AudioClip successAudio;
	

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		playerAnimator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
	}

	private void FixedUpdate()
	{
		CheckGround();
		CheckWall();
		AssignCandidate();
	}

	private void CheckGround()
	{
		m_Grounded = false;
		groundObj = null;
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		if (colliders.Length > 0)
		{
			groundObj = colliders[colliders.Length -1].gameObject;
		}
	}

	private void CheckWall()
	{
		m_Walled = false;
		wallObj = null;
		for (int i = 0; i < m_WallChecks.Length; i++)
		{
			Collider2D[] colliders = Physics2D.OverlapCircleAll(m_WallChecks[i].position, k_WalledRadius, m_WhatIsGround);
			if (colliders.Length > 0)
			{
				wallObj = colliders[colliders.Length -1].gameObject;
			}
		}
		
	}

	private void AssignCandidate()
	{
		if (groundObj != null)
		{
			candidateObj = groundObj;
			if (!m_Grounded)
				OnLandEvent.Invoke();
			m_Grounded = true;
		}
		else
		{
			if (wallObj != null)
			{
				candidateObj = wallObj;
				if (!m_Walled)
					OnLandEvent.Invoke();
				m_Walled = true;
			}
		}

		if (candidateObj != null)
		{
			candidateObj.GetComponentInParent<ChunkClass>()?.CheckTrap(candidateObj, this.gameObject);
			if (candidateObj != previousCandidateObj)
			{
				candidateObj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, .9f);
				if(previousCandidateObj != null)
				    previousCandidateObj.GetComponent<SpriteRenderer>().color = Color.white;
				previousCandidateObj = candidateObj;
			}
		}
		else
		{
			if(previousCandidateObj != null)
				previousCandidateObj.GetComponent<SpriteRenderer>().color = Color.white;
		}
		
		
	}
	
	public void StampCandidate()
	{
		if (candidateObj != null && (m_Grounded || m_Walled))
		{
			audioSource.clip = stickAudio;
			audioSource.Play();
			if (candidateObj.GetComponentInParent<ChunkClass>().StickTool(candidateObj))
			{
				GameManager.Instance.SetResult(playerIndex, true);
				audioSource.clip = successAudio;
				audioSource.Play();
			}
				
		}
		   
	}

	public void CollectCandidate()
	{
		if (candidateObj != null && (m_Grounded || m_Walled))
		{
			audioSource.clip = stickAudio;
			audioSource.Play();
			candidateObj.GetComponentInParent<ChunkClass>().CollectTool(candidateObj);
		}
		    
	}


	public void Move(float move, bool crouch, bool jump)
	{
		// If crouching, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
			{
				crouch = true;
			}
		}

		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{

			// If crouching
			if (crouch)
			{
				if (!m_wasCrouching)
				{
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
				}

				// Reduce the speed by the crouchSpeed multiplier
				move *= m_CrouchSpeed;

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;
			} else
			{
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching)
				{
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}

			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

			PropAffectedMove(move);
			
			playerAnimator.SetFloat("HorizontalSpeed", Mathf.Abs(move));
			

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}
		// If the player should jump...
		if ((m_Grounded || m_Walled) && jump)
		{
			// Add a vertical force to the player.
			m_Grounded = false;
			playerAnimator.SetBool("Jump", true);
			audioSource.clip = jumpAudio;
			audioSource.Play();
			m_Rigidbody2D.AddForce(jumpDirection*m_JumpForce);
		}

		ApplyBound();
		ApplyGravity();
	}

	public void AfterJumpAnim()
	{
		playerAnimator.SetBool("Jump", false);
	}

	private float friction = 16;
	private Vector2 jumpDirection;
	private void PropAffectedMove(float horizontalInput)
	{
		jumpDirection = new Vector2(0, 1);
		if (m_Grounded)
		{
			if (candidateObj != null && candidateObj.CompareTag("Stickable"))
				this.transform.position += new Vector3(candidateObj.GetComponentInParent<ChunkClass>().accumulatedMove.x*Time.fixedDeltaTime, 0, 0);
		}

		if (m_Walled)
		{
			if (candidateObj != null && candidateObj.CompareTag("Stickable"))
			{
				Vector2 frictionForce = new Vector2();
				if (candidateObj.transform.position.x > this.transform.position.x)
				{
					
					if (horizontalInput > 0)
					{
						m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
						this.transform.position += new Vector3(candidateObj.GetComponentInParent<ChunkClass>().accumulatedMove.x*Time.fixedDeltaTime, 0, 0);
						if (m_Rigidbody2D.velocity.y <= 0)
						{
							frictionForce = new Vector2(0, friction);
						}

						jumpDirection = new Vector2(Mathf.Cos(Mathf.PI - jumpRadiance), Mathf.Sin(Mathf.PI - jumpRadiance));
					}

				}
				else
				{
					if (horizontalInput < 0)
					{
						m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
						this.transform.position += new Vector3(candidateObj.GetComponentInParent<ChunkClass>().accumulatedMove.x*Time.fixedDeltaTime, 0, 0);
						if (m_Rigidbody2D.velocity.y <= 0)
						{
							frictionForce = new Vector2(0, friction);
						}
						
						jumpDirection = new Vector2(Mathf.Cos(jumpRadiance), Mathf.Sin(jumpRadiance));
					}
				}
				
				m_Rigidbody2D.AddForce(frictionForce);
			}
			
			
				
		}
	}


	public void PlayerDie()
	{
		//disable some components
		this.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.ReturnToolSprite((int)ToolEnum.Corpse);
		playerAnimator.enabled = false;
		m_Rigidbody2D.Sleep();
		this.GetComponent<BoxCollider2D>().enabled = false;
		this.GetComponent<PlayerAction>().enabled = false;
		if(dieProcedure != null)
			StopCoroutine(dieProcedure);
		dieProcedure = DieProcedure();
		StartCoroutine(dieProcedure);
	}

	public void PlayerAlive()
	{
		//enable those components
		this.gameObject.SetActive(true);
		playerAnimator.enabled = true;
		m_Rigidbody2D.WakeUp();
		this.GetComponent<BoxCollider2D>().enabled = true;
		this.GetComponent<PlayerAction>().enabled = true;
	}

	//if walled and 
	private void ClingWall()
	{
		
	}

	private void ApplyBound()
	{
		if (this.transform.position.x >= GlobalParameters.Instance.horizontalBound && m_Rigidbody2D.velocity.x >= 0)
		{
			m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
			this.transform.position = new Vector2(GlobalParameters.Instance.horizontalBound,this.transform.position.y);
		}


		if (this.transform.position.x <= -GlobalParameters.Instance.horizontalBound && m_Rigidbody2D.velocity.x <= 0)
		{
			m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
			this.transform.position = new Vector2(-GlobalParameters.Instance.horizontalBound,this.transform.position.y);
		}


		if (this.transform.position.y >= GlobalParameters.Instance.verticalBound && m_Rigidbody2D.velocity.y >= 0)
		{
			m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
			this.transform.position = new Vector2(this.transform.position.x,GlobalParameters.Instance.verticalBound);
		}


		if (this.transform.position.y <= -GlobalParameters.Instance.verticalBound && m_Rigidbody2D.velocity.y <= 0)
		{
			m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x,0);
			this.transform.position = new Vector2(this.transform.position.x,-GlobalParameters.Instance.verticalBound);
			PlayerDie();
		}
	}

	private WaitForSeconds dieDur = new WaitForSeconds(0.4f);
	private IEnumerator dieProcedure;
	private IEnumerator DieProcedure()
	{
		yield return dieDur;
		GameManager.Instance.SetResult(playerIndex, false);
	}
	
	private void ApplyGravity()
	{
		m_Rigidbody2D.AddForce(new Vector2(0f, -gravity));
	}


	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		this.transform.rotation *= Quaternion.AngleAxis(180, Vector3.up);
	}
	
}
