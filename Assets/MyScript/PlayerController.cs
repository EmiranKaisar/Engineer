using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using SpriteRenderer = UnityEngine.SpriteRenderer;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;

	private bool m_Grounded;
	private bool m_RightWalled;
	private bool m_LeftWalled;
	private bool m_Ceiling;
	
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;
	
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

	private PlayerCustomPhysic customPhysic;
	

	private void Awake()
	{
		playerAnimator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
		customPhysic = GetComponent<PlayerCustomPhysic>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
	}

	private void FixedUpdate()
	{
		m_Grounded = customPhysic.detectorArr[(int)CollideDetector.Bottom].collided;
		
		if (!m_Grounded)
			OnLandEvent.Invoke();
		
		m_RightWalled = customPhysic.detectorArr[(int)CollideDetector.Right].collided;
		m_LeftWalled = customPhysic.detectorArr[(int)CollideDetector.Left].collided;
		m_Ceiling = customPhysic.detectorArr[(int)CollideDetector.Bottom].collided;
	}
	
	

	private void AssignCandidate(float move)
	{
		if (m_RightWalled && move > 0.2f)
		{
			candidateObj = customPhysic.detectorArr[(int)CollideDetector.Right].collidedObj;
		}else if (m_LeftWalled && move < -0.2f)
		{
			candidateObj = customPhysic.detectorArr[(int)CollideDetector.Left].collidedObj;
		}else if (m_Ceiling)
		{
			candidateObj = customPhysic.detectorArr[(int)CollideDetector.Upper].collidedObj;
		}
		else if(m_Grounded)
		{
			candidateObj = customPhysic.detectorArr[(int)CollideDetector.Bottom].collidedObj;
		}
		else
		{
			candidateObj = null;
		}

		if (m_Grounded)
		{
			candidateObj = customPhysic.detectorArr[(int)CollideDetector.Bottom].collidedObj;
		}
		else
		{
			if (m_RightWalled && move > 0.2f)
				candidateObj = customPhysic.detectorArr[(int)CollideDetector.Right].collidedObj;
			
			if (m_LeftWalled && move < -0.2f)
				candidateObj = customPhysic.detectorArr[(int)CollideDetector.Left].collidedObj;
			
			if (m_Ceiling)
				candidateObj = customPhysic.detectorArr[(int)CollideDetector.Upper].collidedObj;
		}
			
			
		
		//Debug.Log(candidateObj);

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
		if (candidateObj != null && (m_Grounded || m_RightWalled || m_LeftWalled || m_Ceiling))
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
		if (candidateObj != null && (m_Grounded || m_Grounded || m_RightWalled || m_LeftWalled || m_Ceiling))
		{
			audioSource.clip = stickAudio;
			audioSource.Play();
			candidateObj.GetComponentInParent<ChunkClass>().CollectTool(candidateObj);
		}
		    
	}


	public void Move(float move, bool crouch, bool jump)
	{
		
		AssignCandidate(move);
		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{
			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, customPhysic.velocity.y);
			// And then smoothing it out and applying it to the character
			customPhysic.velocity = Vector3.SmoothDamp(customPhysic.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

			PropAffectedMove(move);
			
			playerAnimator.SetFloat("HorizontalSpeed", Mathf.Abs(move));
			

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				//Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				//Flip();
			}
		}
		// If the player should jump...
		if ((m_Grounded || m_RightWalled || m_LeftWalled) && jump)
		{
			// Add a vertical force to the player.
			m_Grounded = false;
			playerAnimator.SetBool("Jump", true);
			audioSource.clip = jumpAudio;
			audioSource.Play();
			customPhysic.velocity += new Vector3(jumpDirection.x, jumpDirection.y, 0)* m_JumpForce;
		}

		ApplyBound();
	}

	public void AfterJumpAnim()
	{
		playerAnimator.SetBool("Jump", false);
	}

	private float friction = 16;
	private Vector2 jumpDirection;
	private bool newRot = true;
	private Vector3 rotOffset = Vector3.zero;
	private void PropAffectedMove(float horizontalInput)
	{
		jumpDirection = new Vector2(0, 1);
		if (m_Grounded)
		{
			if (candidateObj != null && candidateObj.CompareTag("Stickable"))
				this.transform.position += new Vector3(candidateObj.GetComponentInParent<ChunkClass>().accumulatedMove.x*Time.fixedDeltaTime, 0, 0);
		}

		if (m_RightWalled || m_LeftWalled)
		{
			if (candidateObj != null && candidateObj.CompareTag("Stickable"))
			{
				Vector2 frictionForce = new Vector2();
				if (candidateObj.transform.position.x > this.transform.position.x)
				{
					
					if (horizontalInput > 0)
					{
						customPhysic.velocity = new Vector2(0, customPhysic.velocity.y);
						this.transform.position += new Vector3(candidateObj.GetComponentInParent<ChunkClass>().accumulatedMove.x*Time.fixedDeltaTime, 0, 0);
						if (customPhysic.velocity.y <= 0)
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
						customPhysic.velocity = new Vector2(0, customPhysic.velocity.y);
						this.transform.position += new Vector3(candidateObj.GetComponentInParent<ChunkClass>().accumulatedMove.x*Time.fixedDeltaTime, 0, 0);
						if (customPhysic.velocity.y <= 0)
						{
							frictionForce = new Vector2(0, friction);
						}
						
						jumpDirection = new Vector2(Mathf.Cos(jumpRadiance), Mathf.Sin(jumpRadiance));
					}
				}
				
				customPhysic.AddAcc(frictionForce);
			}
		}


		if (candidateObj != null)
		{
			if (candidateObj.GetComponentInParent<ChunkClass>().inRotateProcedure)
			{
				if (newRot)
				{
					rotOffset = candidateObj.transform.parent.InverseTransformPoint(transform.position + new Vector3(0.5f, -0.5f, 0) - candidateObj.transform.parent.position);
					newRot = false;
				}

				//transform.rotation = candidateObj.transform.parent.rotation;
				
				transform.position = candidateObj.transform.parent.position + candidateObj.transform.parent.TransformPoint(rotOffset) + new Vector3(0.5f, -0.5f, 0);
			}
			else
			{
				newRot = true;
			}
		}
		else
		{
			newRot = true;
		}
		
	}


	public void PlayerDie()
	{
		//disable some components
		this.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.ReturnToolSprite((int)ToolEnum.Corpse);
		playerAnimator.enabled = false;
		customPhysic.Sleep();
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
		customPhysic.WakeUp();
		this.GetComponent<PlayerAction>().enabled = true;
	}
	

	private void ApplyBound()
	{
		if (this.transform.position.x >= GlobalParameters.Instance.horizontalBound && customPhysic.velocity.x >= 0)
		{
			customPhysic.velocity = new Vector2(0, customPhysic.velocity.y);
			this.transform.position = new Vector2(GlobalParameters.Instance.horizontalBound,this.transform.position.y);
		}


		if (this.transform.position.x <= -GlobalParameters.Instance.horizontalBound && customPhysic.velocity.x <= 0)
		{
			customPhysic.velocity = new Vector2(0, customPhysic.velocity.y);
			this.transform.position = new Vector2(-GlobalParameters.Instance.horizontalBound,this.transform.position.y);
		}


		if (this.transform.position.y >= GlobalParameters.Instance.verticalBound && customPhysic.velocity.y >= 0)
		{
			customPhysic.velocity = new Vector2(customPhysic.velocity.x, 0);
			this.transform.position = new Vector2(this.transform.position.x,GlobalParameters.Instance.verticalBound);
		}


		if (this.transform.position.y <= -GlobalParameters.Instance.verticalBound && customPhysic.velocity.y <= 0)
		{
			customPhysic.velocity = new Vector2(customPhysic.velocity.x,0);
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


	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		this.transform.rotation *= Quaternion.AngleAxis(180, Vector3.up);
	}
	
}
