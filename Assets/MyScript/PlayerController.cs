using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using SpriteRenderer = UnityEngine.SpriteRenderer;
using Vector2 = UnityEngine.Vector2;

public class PlayerController : MonoBehaviour, IAlive
{
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

    private float playerSpeed = 10;

    [HideInInspector] public string playerName;

    [HideInInspector] public int playerIndex = 0;

    private float jumpRadiance = Mathf.PI / 3;

    private GameObject candidateObj;
    private GameObject previousCandidateObj;

    private Animator playerAnimator;

    private Transform playerTransform;
    
    private Vector2 jumpDirection;
    private float standardJumpHeight = 3;
    private float jumpHeight = 3;
    private bool canJumpOnWall;
    private float jumpBufferTime = 0.15f;
    private float jumpTimer = 0;
    

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        playerTransform = transform;
    }

    private void FixedUpdate()
    {
        CheckCollision();
        UpdateState();
    }
    

    #region Detection

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
        
        Vector2 originPos1 = new Vector2(playerTransform.position.x, playerTransform.position.y) +
                            faceDetectorList[index].pointDetector[0].detectPos;
        Vector2 originPos2 = new Vector2(playerTransform.position.x, playerTransform.position.y) +
            faceDetectorList[index].pointDetector[1].detectPos;
        
        
        Collider2D[] collider1 = Physics2D.OverlapCircleAll(originPos1, k_DetectorRadius, m_WhatIsGround);
        
        Collider2D[] collider2 = Physics2D.OverlapCircleAll(originPos2, k_DetectorRadius, m_WhatIsGround);

        if (collider1.Length > 0 || collider2.Length > 0)
        {
            faceDetectorList[index].collided = true;
            for (int i = 0; i < collider1.Length; i++)
            {
                for (int j = 0; j < collider2.Length; j++)
                {
                    if (collider1[i].gameObject == collider2[j].gameObject)
                    {
                        faceDetectorList[index].hasCandidate = true;
                        faceDetectorList[index].pointDetector[0].stickableObj = collider1[i].gameObject;
                        faceDetectorList[index].pointDetector[1].stickableObj = collider1[i].gameObject;
                    }
                        
                }
            }
        }
        else
        {
            faceDetectorList[index].pointDetector[0].stickableObj = null;
            faceDetectorList[index].pointDetector[1].stickableObj = null;
        }
        

    }

    private void UpdateState()
    {
        m_Grounded = faceDetectorList[(int)DetectorEnum.Bottom].collided || (gravity < 0 && faceDetectorList[(int)DetectorEnum.Upper].collided);
        
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
        }else if (faceDetectorList[(int)DetectorEnum.Upper].hasCandidate && upsideDown)
        {
            candidateObj = faceDetectorList[(int)DetectorEnum.Upper].pointDetector[0].stickableObj;
        }else if (faceDetectorList[(int)DetectorEnum.Bottom].hasCandidate && !upsideDown)
        {
            candidateObj = faceDetectorList[(int)DetectorEnum.Bottom].pointDetector[0].stickableObj;
        }
        else
        {
            candidateObj = null;
        }
        

        if (candidateObj != null)
        {
            if (candidateObj != previousCandidateObj)
            {
                candidateObj.GetComponent<SpriteRenderer>().color = Color.white;
                if (previousCandidateObj != null)
                    previousCandidateObj.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.8f, 1);
                previousCandidateObj = candidateObj;
            }
        }
        else
        {
            if (previousCandidateObj != null)
                previousCandidateObj.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.8f, 1);

            previousCandidateObj = null;
        }
    }

    #endregion


    #region Player action API

    public void StampCandidate()
    {
        if (candidateObj != null)
        {
            AudioManager.Instance.PlayerAudioSourcePlay(playerIndex, PlayerAudioEnum.PlayerStick);
            if (candidateObj.GetComponentInParent<ChunkClass>().StickTool(candidateObj, playerIndex))
                GameManager.Instance.IncrementPlayerOperactionCount(playerIndex);
            
        }
    }

    public void CollectCandidate()
    {
        if (candidateObj != null)
        {
            AudioManager.Instance.PlayerAudioSourcePlay(playerIndex, PlayerAudioEnum.PlayerCollect);
            
            if(candidateObj.GetComponentInParent<ChunkClass>().CollectTool(candidateObj, playerIndex))
                GameManager.Instance.IncrementPlayerOperactionCount(playerIndex);
        }
    }

    public void Move(float move, bool jump)
    {
        AssignCandidate(move);
        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * playerSpeed, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity,
                m_MovementSmoothing);

            PropAffectedMove(move * playerSpeed);

            playerAnimator.SetFloat("HorizontalSpeed", Mathf.Abs(move));


            // If the input is moving the player right and the player is facing left...
            if (move * playerSpeed > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move * playerSpeed < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
        }
        

        
        if (jump)
        {
            jumpTimer = jumpBufferTime;
        }
        TimeBufferJump();


        ApplyBound();
        ApplyGravity();
    }

    private void TimeBufferJump()
    {
        if (jumpTimer > 0)
        {
            jumpTimer -= Time.fixedDeltaTime;
            if ((m_Grounded || (m_Walled && canJumpOnWall)))
            {
                jumpTimer = -1;
                ApplyJump();
            }
        }
    }
    
    #endregion


    #region Prop affected movement

    private void PropAffectedMove(float horizontalInput)
    {
        jumpDirection = new Vector2(0, 1);
        canJumpOnWall = false;
        if (m_Grounded)
        {
            PropAffectedHorizontalMovement(DetectorEnum.Bottom);
            PropAffectedJumpDir(DetectorEnum.Bottom);
        }else if (faceDetectorList[(int)DetectorEnum.Right].collided && horizontalInput > 0)
        {
            canJumpOnWall = true;
            PropAffectedHorizontalMovement(DetectorEnum.Right);
            PropAffectedJumpDir(DetectorEnum.Right);
            SlideOnWall();
            
        }else if (faceDetectorList[(int)DetectorEnum.Left].collided && horizontalInput < 0)
        {
            canJumpOnWall = true;
            PropAffectedHorizontalMovement(DetectorEnum.Left);
            PropAffectedJumpDir(DetectorEnum.Left);
            SlideOnWall();
        }
    }

    private void PropAffectedHorizontalMovement(DetectorEnum detector)
    {
        for (int i = 0; i < 2; i++)
        {
            if (faceDetectorList[(int)detector].pointDetector[i].stickableObj != null)
            {
                playerTransform.position +=
                    new Vector3(
                        faceDetectorList[(int)detector].pointDetector[i].stickableObj.GetComponentInParent<ChunkClass>().accumulatedMove.x * Time.fixedDeltaTime,
                        0, 0);
                break;
            }
        }
    }

    private void PropAffectedJumpDir(DetectorEnum detector)
    {
        switch (detector)
        {
            case DetectorEnum.Bottom:
                jumpDirection = new Vector2(0, 1);
                break;
            case DetectorEnum.Right:
                jumpDirection = new Vector2(Mathf.Cos(Mathf.PI - jumpRadiance),
                    Mathf.Sin(Mathf.PI - jumpRadiance))*1.3f;
                break;
            case DetectorEnum.Left:
                jumpDirection = new Vector2(Mathf.Cos(jumpRadiance),
                    Mathf.Sin(jumpRadiance))*1.3f;
                break;
        }

        if (upsideDown)
            jumpDirection = new Vector2(jumpDirection.x, -jumpDirection.y);
    }

    private void SlideOnWall()
    {
        if(m_Rigidbody2D.velocity.y <= 0 && !upsideDown)
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -2);
        
        if(m_Rigidbody2D.velocity.y >= 0 && upsideDown)
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 2);
    }
    
    private void ApplyBound()
    {
        if (playerTransform.position.x >= GlobalParameters.Instance.horizontalBound && m_Rigidbody2D.velocity.x >= 0)
        {
            m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
            playerTransform.position = new Vector2(GlobalParameters.Instance.horizontalBound, playerTransform.position.y);
        }


        if (playerTransform.position.x <= -GlobalParameters.Instance.horizontalBound && m_Rigidbody2D.velocity.x <= 0)
        {
            m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
            playerTransform.position =
                new Vector2(-GlobalParameters.Instance.horizontalBound, playerTransform.position.y);
        }


        if (playerTransform.position.y >= GlobalParameters.Instance.verticalBound && m_Rigidbody2D.velocity.y >= 0)
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
            playerTransform.position = new Vector2(playerTransform.position.x, GlobalParameters.Instance.verticalBound);
            
            if(upsideDown)
                PlayerDie();
        }


        if (playerTransform.position.y <= -GlobalParameters.Instance.verticalBound && m_Rigidbody2D.velocity.y <= 0)
        {
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
            playerTransform.position = new Vector2(playerTransform.position.x, -GlobalParameters.Instance.verticalBound);
            if(!upsideDown)
                PlayerDie();
        }
    }
    
    #endregion


    private void ApplyJump()
    {
        m_Grounded = false;
        m_Walled = false;
        canJumpOnWall = false;
        playerAnimator.SetBool("Jump", true);
        
        AudioManager.Instance.PlayerAudioSourcePlay(playerIndex, PlayerAudioEnum.PlayerJump);
        
        Vector2 velocity = m_Rigidbody2D.velocity;
        if (velocity.y != 0)
            velocity = new Vector2(velocity.x, -velocity.y);
        
        m_Rigidbody2D.velocity += velocity + jumpDirection * Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(gravity));
    }
    
    public void AfterJumpAnim()
    {
        playerAnimator.SetBool("Jump", false);
    }
    
    private void ApplyGravity()
    {
        m_Rigidbody2D.velocity += new Vector2(0, -gravity)*Time.fixedDeltaTime;
    }
    
    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        playerTransform.rotation *= Quaternion.AngleAxis(180, Vector3.up);
    }

    public void FlipByChunk(ToolDirection dir)
    {
        if (dir == ToolDirection.Original || dir == ToolDirection.Left)
        {
            playerSpeed *= -1;
        }else if (dir == ToolDirection.Up || dir == ToolDirection.Down)
        {
            upsideDown = true;
            gravity *= -1;
        }
    }

    #region Alive & Death
    public void GotAttacked()
    {
        PlayerDie();
    }

    public void PlayerDie()
    {
        //disable some components
        this.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.ReturnToolSprite((int)ToolEnum.Corpse);
        playerAnimator.enabled = false;
        m_Rigidbody2D.Sleep();
        this.GetComponent<BoxCollider2D>().enabled = false;
        this.GetComponent<PlayerAction>().enabled = false;
        GameManager.Instance.SetResult(playerIndex, false);
        if (dieProcedure != null)
            StopCoroutine(dieProcedure);
        dieProcedure = DieProcedure();
        StartCoroutine(dieProcedure);
    }

    private bool upsideDown = false;
    public void PlayerAlive()
    {
        //enable those components
        playerTransform.rotation = Quaternion.identity;
        playerSpeed = 10;
        gravity = 20;
        upsideDown = false;
        m_FacingRight = true;
        this.gameObject.SetActive(true);
        playerAnimator.enabled = true;
        m_Rigidbody2D.WakeUp();
        this.GetComponent<BoxCollider2D>().enabled = true;
        this.GetComponent<PlayerAction>().enabled = true;
    }
    
    private WaitForSeconds dieDur = new WaitForSeconds(0.6f);
    private IEnumerator dieProcedure;

    private IEnumerator DieProcedure()
    {
        yield return dieDur;
        GameManager.Instance.StartGame();
    }


    #endregion
    



}