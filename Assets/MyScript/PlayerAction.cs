using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAction : MonoBehaviour
{
    public PlayerController playerController;

    public float playerSpeed;

    private float horizontalMove = 0f;

    private bool jump;

    private MyInputSetting playerInput;

    private float selectUIMove = 0;

    public void SetPlayerInput(MyInputSetting setting)
    {
        playerInput = setting;
        SwitchActionMap("Player");
        playerInput.Player.Move.performed += PlayerMove;
        playerInput.Player.Move.canceled += PlayerMove;
        playerInput.Player.Put.performed += PlayerPut;
        playerInput.Player.Collect.performed += PlayerCollect;
        playerInput.Player.Jump.performed += PlayerJump;
        playerInput.UI.Move.performed += UIMove;
        
    }


    public void PlayerMove(InputAction.CallbackContext ctx)
    {
        switch (ctx.phase)
        {
            case InputActionPhase.Performed:
                horizontalMove = ctx.ReadValue<float>()* playerSpeed;
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
        switch (ctx.phase)
        {
            case InputActionPhase.Performed:
                playerController.StampCandidate();
                return;
        }
    }


    public void UIMove(InputAction.CallbackContext ctx)
    {
        switch (ctx.phase)
        {
            case InputActionPhase.Performed:
                selectUIMove = ctx.ReadValue<float>();
                break;
            case InputActionPhase.Canceled:
                selectUIMove = 0;
                break;
        }
        Debug.Log("ui move: " + selectUIMove);
    }

    public void UIChoose(InputAction.CallbackContext ctx)
    {
        switch (ctx.phase)
        {
            case InputActionPhase.Performed:
                Debug.Log("choose");
                break;    
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

    public void SwitchActionMap(string mapName)
    {
        switch (mapName)
        {
            case "Player":
                playerInput.Player.Enable();
                playerInput.UI.Disable();
                break;
            case "UI":
                playerInput.Player.Disable();
                playerInput.UI.Enable();
                break;
            default:
                playerInput.Player.Enable();
                playerInput.UI.Disable();
                break;
        }
    }
    
}
