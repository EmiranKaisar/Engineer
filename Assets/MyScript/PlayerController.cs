using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using SpriteRenderer = UnityEngine.SpriteRenderer;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 400f; // Amount of force added when the player jumps.

    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f; // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false; // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround; // A mask determining what is ground to the character;

    [SerializeField] public List<FaceDetector> faceDetectorList;
    
    [Serializable]
    public class FaceDetector
    {
        public DetectorEnum face;
        public bool collided;
        public bool hasCandidate;
        public PointDetector[] pointDetector;
    }
    
    [Serializable]
    public class PointDetector
    {
        public Vector2 detectPos;
        public GameObject stickableObj;
    }

    const float k_DetectorRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded; // Whether or not the player is grounded.
    private bool m_Walled;
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true; // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;

    public float gravity = 20;

    [HideInInspector] public string playerName;

    [HideInInspector] public int playerIndex = 0;

    private float jumpRadiance = Mathf.PI / 3;

    [Header("Events")] [Space] public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool>
    {
    }

    public BoolEvent OnCrouchEvent;

    private GameObject candidateObj;
    private GameObject previousCandidateObj;

    private Animator playerAnimator;

    private AudioSource audioSource;

    public AudioClip stickAudio;
    public AudioClip jumpAudio;
    public AudioClip successAudio;

    private Transform playerTransform;
    


    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        playerTransform = transform;

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
    }

    private void FixedUpdate()
    {
        CheckCollision();
        UpdateState();
    }

    private void CheckCollision()
    {
        for (int i = 0; i < faceDetectorList.Count; i++)
        {
            CheckFace(i);
        }
    }

    private void CheckFace(int index)
    {
        faceDetectorList[index].collided = false;
        faceDetectorList[index].hasCandidate = false;
        for (int i = 0; i < faceDetectorList[index].pointDetector.Length; i++)
        {
            bool newObj = true;
            Vector2 originPos = new Vector2(playerTransform.position.x, playerTransform.position.y) +
                                faceDetectorList[index].pointDetector[i].detectPos;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(originPos, k_DetectorRadius, m_WhatIsGround);

            if (colliders.Length > 0)
            {
                faceDetectorList[index].collided = true;
                for (int j = 0; j < colliders.Length; j++)
                {
                    if (faceDetectorList[index].pointDetector[i].stickableObj == colliders[j])
                        newObj = false;
                    
                }

                if (newObj)
                    faceDetectorList[index].pointDetector[i].stickableObj =
                        colliders[0].gameObject;
            }
            else
            {
                faceDetectorList[index].pointDetector[i].stickableObj = null;
            }

        }
        

        if (faceDetectorList[index].pointDetector[0].stickableObj ==
            faceDetectorList[index].pointDetector[1].stickableObj && faceDetectorList[index].collided)
            faceDetectorList[index].hasCandidate = true;

    }

    private void UpdateState()
    {
        m_Grounded = faceDetectorList[(int)DetectorEnum.Bottom].collided;
        m_Walled = faceDetectorList[(int)DetectorEnum.Right].collided ||
                   faceDetectorList[(int)DetectorEnum.Left].collided;
    }

    private void AssignCandidate(float move)
    {
        if (move > 0 && faceDetectorList[(int)DetectorEnum.Right].hasCandidate)
        {
            candidateObj = faceDetectorList[(int)DetectorEnum.Right].pointDetector[0].stickableObj;
        }else if (move < 0 && faceDetectorList[(int)DetectorEnum.Left].hasCandidate)
        {
            candidateObj = faceDetectorList[(int)DetectorEnum.Left].pointDetector[0].stickableObj;
        }else if (faceDetectorList[(int)DetectorEnum.Upper].hasCandidate)
        {
            candidateObj = faceDetectorList[(int)DetectorEnum.Upper].pointDetector[0].stickableObj;
        }else if (faceDetectorList[(int)DetectorEnum.Bottom].hasCandidate)
        {
            candidateObj = faceDetectorList[(int)DetectorEnum.Bottom].pointDetector[0].stickableObj;
        }
        

        if (candidateObj != null)
        {
            candidateObj.GetComponentInParent<ChunkClass>()?.CheckTrap(candidateObj, this.gameObject);
            if (candidateObj != previousCandidateObj)
            {
                candidateObj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, .9f);
                if (previousCandidateObj != null)
                    previousCandidateObj.GetComponent<SpriteRenderer>().color = Color.white;
                previousCandidateObj = candidateObj;
            }
        }
        else
        {
            if (previousCandidateObj != null)
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
        AssignCandidate(move);
        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity,
                m_MovementSmoothing);

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
        if ((m_Grounded || (m_Walled && canJumpOnWall)) && jump)
        {
            // Add a vertical force to the player.
            m_Grounded = false;
            playerAnimator.SetBool("Jump", true);
            audioSource.clip = jumpAudio;
            audioSource.Play();
            m_Rigidbody2D.AddForce(jumpDirection * m_JumpForce);
        }

        ApplyBound();
        ApplyGravity();
    }

    public void AfterJumpAnim()
    {
        playerAnimator.SetBool("Jump", false);
    }

    private Vector2 jumpDirection;
    private bool canJumpOnWall;

    private void PropAffectedMove(float horizontalInput)
    {
        jumpDirection = new Vector2(0, 1);
        canJumpOnWall = false;
        if (m_Grounded)
        {
            if (candidateObj != null && candidateObj.CompareTag("Stickable"))
                this.transform.position +=
                    new Vector3(candidateObj.GetComponentInParent<ChunkClass>().accumulatedMove.x * Time.fixedDeltaTime,
                        0, 0);
        }else if (faceDetectorList[(int)DetectorEnum.Right].collided && horizontalInput > 0)
        {
            canJumpOnWall = true;
            this.transform.position +=
                new Vector3(
                    candidateObj.GetComponentInParent<ChunkClass>().accumulatedMove.x * Time.fixedDeltaTime,
                    0, 0);
            if(m_Rigidbody2D.velocity.y <= 0)
               m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -2);
            jumpDirection = new Vector2(Mathf.Cos(Mathf.PI - jumpRadiance),
                Mathf.Sin(Mathf.PI - jumpRadiance))*1.3f;
        }else if (faceDetectorList[(int)DetectorEnum.Left].collided && horizontalInput < 0)
        {
            canJumpOnWall = true;
            this.transform.position +=
                new Vector3(
                    candidateObj.GetComponentInParent<ChunkClass>().accumulatedMove.x * Time.fixedDeltaTime,
                    0, 0);
            if(m_Rigidbody2D.velocity.y <= 0)
               m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -2);
            jumpDirection = new Vector2(Mathf.Cos(jumpRadiance),
                Mathf.Sin(jumpRadiance))*1.3f;
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
        if (dieProcedure != null)
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


    private void ApplyBound()
    {
        if (this.transform.position.x >= GlobalParameters.Instance.horizontalBound && m_Rigidbody2D.velocity.x >= 0)
        {
            m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
            this.transform.position = new Vector2(GlobalParameters.Instance.horizontalBound, this.transform.position.y);
        }


        if (this.transform.position.x <= -GlobalParameters.Instance.horizontalBound && m_Rigidbody2D.velocity.x <= 0)
        {
            m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
            this.transform.position =
                new Vector2(-GlobalParameters.Instance.horizontalBound, this.transform.position.y);
        }


        if (this.transform.position.y >= GlobalParameters.Instance.verticalBound && m_Rigidbody2D.velocity.y >= 0)
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
            this.transform.position = new Vector2(this.transform.position.x, GlobalParameters.Instance.verticalBound);
        }


        if (this.transform.position.y <= -GlobalParameters.Instance.verticalBound && m_Rigidbody2D.velocity.y <= 0)
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
            this.transform.position = new Vector2(this.transform.position.x, -GlobalParameters.Instance.verticalBound);
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