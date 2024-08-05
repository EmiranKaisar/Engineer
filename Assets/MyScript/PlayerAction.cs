using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAction : MonoBehaviour
{
    public PlayerController playerController;

    public float playerSpeed;

    private float horizontalMove = 0f;

    private bool jump;


    public void PlayerMove(InputAction.CallbackContext ctx)
    {
        switch (ctx.phase)
        {
            case InputActionPhase.Performed:
                horizontalMove = ctx.ReadValue<Vector2>().x * playerSpeed;
                break;
            case InputActionPhase.Canceled:
                horizontalMove = 0;
                break;
        }
    }


    public void PlayerJump(InputAction.CallbackContext ctx)
    {
        switch (ctx.phase)
        {
            case InputActionPhase.Performed:
                jump = true;
                break;
        }
    }

    public void PlayerPut(InputAction.CallbackContext ctx)
    {
        //ctx.performed += playerController.StampCandidate();
        switch (ctx.phase)
        {
            case InputActionPhase.Performed:
                playerController.StampCandidate();
                return;
        }
    }
    
    public void PlayerCollect(InputAction.CallbackContext ctx)
    {
        //ctx.performed += playerController.StampCandidate();
        switch (ctx.phase)
        {
            case InputActionPhase.Performed:
                playerController.CollectCandidate();
                return;
        }
    }


    private void FixedUpdate()
    {
        playerController.Move(horizontalMove*Time.fixedDeltaTime, jump);
        jump = false;
        
    }
    
}
