using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Samples.RebindUI;
using UnityEngine.Events;
using TMPro;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
using UnityEngine.UI;


public class InputManager : MonoBehaviour
{
    //public PlayerInput inputAction;

    public MyInputSetting myInputSetting;
    
    [Serializable]
    public class RebindClass
    {
        public string actionName;
        public GameObject rebindButtonObj;
    }
    [SerializeField]
    public RebindClass[] rebindClassArr;

    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
        myInputSetting = new MyInputSetting();
        SetPlayerInput();
        SetRebindButton();
    }

    private void SetPlayerInput()
    {
        foreach (var obj in GameManager.Instance.playerList)
        {
            obj.GetComponent<PlayerAction>().SetPlayerInput(myInputSetting);
        }
    }

    private void SetRebindButton()
    {
        foreach (var bind in myInputSetting.bindings)
        {
            if (bind.groups == "Keyboard")
            {
                for (int i = 0; i < rebindClassArr.Length; i++)
                {
                    if (rebindClassArr[i].actionName == bind.action)
                    {
                        TMP_Text bindingText = rebindClassArr[i].rebindButtonObj.GetComponentInChildren<TMP_Text>();
                        m_BindingText = bindingText;
                        ResolveActionAndBinding(bind.id.ToString(), out var action, out int bindingIndex);
                        UpdateBindingDisplay(action, bindingIndex);
                        rebindClassArr[i].rebindButtonObj.GetComponent<Button>().onClick.AddListener(() =>
                            ClickRebindButtonAction(this, bind.id.ToString(), bindingText));
                    }
                }
            }
            
        }

    }

    private void ClickRebindButtonAction(InputManager inputManager, string bindID, TMP_Text bindTextUI)
    {
        inputManager.StartRebind(bindID);
        m_BindingText = bindTextUI;
    }


    #region Rebind
    
    public void StartRebind(string bindingID)
    {
        if (!ResolveActionAndBinding(bindingID, out var action, out var bindingIndex))
            return;

        if (!action.bindings[bindingIndex].isComposite)
            PerformInteractiveRebind(action, bindingIndex);
    }

    private bool ResolveActionAndBinding(string bindingID, out InputAction action, out int bindingIndex)
    {
        bindingIndex = -1;
        action = ReturnActionByBindingID(bindingID);
        if (action == null)
            return false;

        if (string.IsNullOrEmpty(bindingID))
            return false;

        // Look up binding index.
        var bindingId = new Guid(bindingID);
        bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
        if (bindingIndex == -1)
        {
            Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
            return false;
        }

        return true;
    }
    

    
    private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;
    
    [SerializeField]
    private InputBinding.DisplayStringOptions m_DisplayStringOptions;
    

    
    [Tooltip("Optional UI that will be shown while a rebind is in progress.")]
    [SerializeField]
    private GameObject m_RebindOverlay;
    
    [Tooltip("Text label that will receive the current, formatted binding string.")]
    [SerializeField]
    private TMP_Text m_BindingText;
    
    [Tooltip("Optional text label that will be updated with prompt for user input.")]
    [SerializeField]
    private TMP_Text m_RebindText;
    
    [SerializeField]
    private InteractiveRebindEvent m_RebindStartEvent;
    
    [Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying "
             + "bindings in custom ways, e.g. using images instead of text.")]
    [SerializeField]
    private UpdateBindingUIEvent m_UpdateBindingUIEvent;
    
    [Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
    [SerializeField]
    private InteractiveRebindEvent m_RebindStopEvent;
    


    private void PerformInteractiveRebind(InputAction action, int bindingIndex)
    {
        m_RebindOperation?.Cancel(); // Will null out m_RebindOperation.

        void CleanUp()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;
        }

        action.Disable();

        // Configure the rebind.
        m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .OnCancel(
                operation =>
                {
                    m_RebindStopEvent?.Invoke(this, operation);
                    m_RebindOverlay?.SetActive(false);
                    UpdateBindingDisplay(action, bindingIndex);
                    CleanUp();
                    action.Enable();
                })
            .OnComplete(
                operation =>
                {
                    m_RebindOverlay?.SetActive(false);
                    m_RebindStopEvent?.Invoke(this, operation);
                    UpdateBindingDisplay(action, bindingIndex);
                    CleanUp();
                    action.Enable();
                });

        // If it's a part binding, show the name of the part in the UI.
        var partName = default(string);
        if (action.bindings[bindingIndex].isPartOfComposite)
            partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

        // Bring up rebind overlay, if we have one.
        m_RebindOverlay?.SetActive(true);
        if (m_RebindText != null)
        {
            Debug.Log(m_RebindOperation.expectedControlType);
            var text = !string.IsNullOrEmpty(m_RebindOperation.expectedControlType)
                ? $"{partName}Waiting for {m_RebindOperation.expectedControlType} input..."
                : $"{partName}Waiting for input...";
            m_RebindText.text = text;
        }

        // If we have no rebind overlay and no callback but we have a binding text label,
        // temporarily set the binding text label to "<Waiting>".
        if (m_RebindOverlay == null && m_RebindText == null && m_RebindStartEvent == null && m_BindingText != null)
            m_BindingText.text = "<Waiting...>";

        // Give listeners a chance to act on the rebind starting.
        m_RebindStartEvent?.Invoke(this, m_RebindOperation);

        m_RebindOperation.Start();
    }
    
    /// <summary>
    /// Trigger a refresh of the currently displayed binding.
    /// </summary>
    public void UpdateBindingDisplay(InputAction action, int bindingIndex)
    {
        var displayString = string.Empty;
        var deviceLayoutName = default(string);
        var controlPath = default(string);

        if (action != null)
        {
            if (bindingIndex != -1)
                displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, m_DisplayStringOptions);
        }
        
        // Set on label (if any).
        if (m_BindingText != null)
            m_BindingText.text = displayString;
        

        // Give listeners a chance to configure UI in response.
        m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
    }
    
    
    [Serializable]
    public class UpdateBindingUIEvent : UnityEvent<InputManager, string, string, string>
    {
    }

    [Serializable]
    public class InteractiveRebindEvent : UnityEvent<InputManager, InputActionRebindingExtensions.RebindingOperation>
    {
    }


    private InputAction ReturnActionByBindingID(string bindingID)
    {
        var localbindingID = new Guid(bindingID);
        foreach (var binding in myInputSetting.asset.bindings)
        {
            if (binding.id == localbindingID)
                return myInputSetting.FindAction(binding.action);
        }

        return null;
    }
    
    #endregion
}