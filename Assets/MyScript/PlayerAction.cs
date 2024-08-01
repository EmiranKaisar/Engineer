using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    public PlayerController playerController;

    public float playerSpeed;

    private float horizontalMove = 0f;

    private bool jump;

    private bool clingRightWall;

    private bool clingLeftWall;

    private float jumpPressedThreshold = 0.2f;
    private float jumpPressedDur = 0;
    

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") *playerSpeed;

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }
        


        if (Input.GetKeyDown(KeyCode.S))
        {
            playerController.StampCandidate();
        }
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            playerController.CollectCandidate();
        }
        
        
        

    }

    private void FixedUpdate()
    {
        playerController.Move(horizontalMove*Time.fixedDeltaTime, jump);
        jump = false;
        
    }
}
