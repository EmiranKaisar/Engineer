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
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") *playerSpeed;

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            playerController.StampCandidate();
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            playerController.CollectCandidate();
        }
        
        
        

    }

    private void FixedUpdate()
    {
        playerController.Move(horizontalMove*Time.fixedDeltaTime, false, jump);
        jump = false;
        
    }
}
