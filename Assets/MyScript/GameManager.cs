using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    //Defing
    public GameObject playerObj;

    public Vector3 playerInitPos;

    public GameObject menuObj;

    [SerializeField] public List<BagTool> toolStoreList;


    public GameObject toolStoreUI;

    public GameObject resultUI;

    private List<GameObject> toolItemUIList = new List<GameObject>();
    
    public delegate void StateChangeAction();

    [Serializable]
    public class StateClass
    {
        public string StateName;
        public StateEnum ThisState;
        public StateChangeAction stateAction;
        public GameObject UIObj;
    }

    [SerializeField] public List<StateClass> StateList;

    private int homeIndex = 0;
    private int presentStateIndex = 0;
    private int previousStateIndex = 0;
    
    private GameObject GamePlayPauseUI;
    private GameObject EditorPauseUI;
    private GameObject GameButtonsObj;
    private GameObject EditorButtonsObj;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        CheckState();
        
        InitGame();
    }

    private void CheckState()
    {
        bool allRight = true;
        for (int i = 0; i < StateList.Count; i++)
        {
            AssignObj(i);
            AssignAction(i);
            if ((StateEnum)i != StateList[i].ThisState)
            {
                allRight = false;
                Debug.Log("State " + StateList[i].StateName + "is not in right position");
            }
            
        }

        if (allRight)
        {
            Debug.Log("All states are in right position");
        }

    }

    private void AssignObj(int index)
    {
        switch (StateList[index].ThisState)
        {
            case StateEnum.ChooseLevel:
                GameButtonsObj = StateList[index].UIObj.transform.Find("GameButtons").gameObject;
                EditorButtonsObj = StateList[index].UIObj.transform.Find("EditorButtons").gameObject;
                return;
            case StateEnum.GamePlayPause:
                GamePlayPauseUI = StateList[index].UIObj.transform.Find("PauseUI").gameObject;
                return;
            case StateEnum.MapEditorPause:
                EditorPauseUI = StateList[index].UIObj.transform.Find("PauseUI").gameObject;
                return;
        }
    }

    private void AssignAction(int index)
    {
        switch (StateList[index].ThisState)
        {
            case StateEnum.Home:
                Debug.Log("it is home");
                StateList[index].stateAction += GotoHomeAction;
                return;
            case StateEnum.ChooseLevel:
                StateList[index].stateAction += GotoChooseLevelAction;
                return;
            case StateEnum.ChoosePlayer:
                StateList[index].stateAction += GotoChoosePlayerAction;
                return;
            case StateEnum.GamePlay:
                StateList[index].stateAction += GotoGamePlayAction;
                return;
            case StateEnum.GamePlayPause:
                StateList[index].stateAction += GotoGamePlayPauseAction;
                return;
            case StateEnum.ChooseEditorLevel:
                StateList[index].stateAction += GotoChooseEditorLevelAction;
                return;
            case StateEnum.MapEditor:
                StateList[index].stateAction += GotoMapEditorAction;
                return;
            case StateEnum.MapEditorPause:
                StateList[index].stateAction += GotoMapEditorPauseAction;
                return;
        }
    }

    private void Start()
    {
        playerObj.transform.position = playerInitPos;
    }

    private void InitGame()
    {
        InitUI();

        // menuObj.SetActive(true);
        // resultUI.SetActive(false);
        //
        // //get tool item UI obj
        // GetToolItemUIObjList();
        // //update appearance, bond action to it.
        // UpdateToolStoreItemUI();
        //pause the game
    }

    private void InitUI()
    {
        int i = 0;
        foreach (var item in StateList)
        {
            item.UIObj.SetActive(false);

            i++;
        }

        ShowUI(homeIndex);
    }


    //根据玩家的输入跳转到对应的游戏状态
    public void StateButtonAction(int actionIndex)
    {
        //presentStateIndex = ReturnIndexByActionName(_actionName);
        presentStateIndex = actionIndex;

        if (presentStateIndex != previousStateIndex)
        {
            UpdateState();
            previousStateIndex = presentStateIndex;
        }
    }


    private void UpdateState()
    {
        if (StateList[presentStateIndex].UIObj != StateList[previousStateIndex].UIObj)
        {
            HideUI(previousStateIndex);
            ShowUI(presentStateIndex);
        }
        
        StateList[presentStateIndex].stateAction?.Invoke();

    }


    private void HideUI(int _index)
    {
        StateList[_index].UIObj.SetActive(false);
    }

    //显示对应的UI
    private void ShowUI(int _index)
    {
        StateList[_index].UIObj.SetActive(true);
    }

    private void GetToolItemUIObjList()
    {
        for (int i = 0; i < toolStoreUI.transform.childCount; i++)
        {
            toolItemUIList.Add(toolStoreUI.transform.GetChild(i).gameObject);
            toolItemUIList[i].SetActive(false);
        }
    }

    private void UpdateToolStoreItemUI()
    {
        for (int i = 0; i < toolStoreList.Count; i++)
        {
            int toolID = (int)toolStoreList[i].toolID;
            toolItemUIList[i].SetActive(true);
            toolItemUIList[i].GetComponent<Image>().sprite = SpriteManager.Instance.ReturnToolSprite(toolID);
            //operate direction
            GlobalMethod.OperateUIDirection(toolItemUIList[i], (int)toolStoreList[i].toolDirection);
            AddButtonListener(i);
        }
    }

    private void AddButtonListener(int index)
    {
        toolItemUIList[index].GetComponent<Button>().onClick.AddListener(() => PickUIObjButtonAction(index));
    }

    void PickUIObjButtonAction(int index)
    {
        //add to bag

        BagManager.Instance.AddTool(new BagTool(toolStoreList[index].toolID, toolStoreList[index].toolDirection));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            playerObj.transform.position = playerInitPos;
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            StateButtonAction((int)StateEnum.GamePlayPause);
        }
    }


    #region State Actions

    private void GotoHomeAction()
    {

    }

    private void GotoChoosePlayerAction()
    {
        
    }

    private void GotoChooseLevelAction()
    {
        EditorButtonsObj.SetActive(false);
        GameButtonsObj.SetActive(true);
    }
    
    private void GotoGamePlayAction()
    {
        switch (StateList[previousStateIndex].ThisState)
        {
            case StateEnum.GamePlayPause:
                ClosePauseGameUI();
                return;
            case StateEnum.ChooseLevel:
                StartGame();
                return;
            case StateEnum.MapEditor:
                StartGame();
                return;
        }
    }
    
    
    private void GotoGamePlayPauseAction()
    {
        if (GamePlayPauseUI == null)
        {
            GamePlayPauseUI = StateList[presentStateIndex].UIObj.transform.Find("PauseUI").gameObject;
        }

        GamePlayPauseUI.SetActive(true);
    }

    private void GotoChooseEditorLevelAction()
    {
        EditorButtonsObj.SetActive(true);
        GameButtonsObj.SetActive(false);
    }

    private void GotoMapEditorAction()
    {
        switch (StateList[previousStateIndex].ThisState)
        {
            case StateEnum.GamePlay:
                BackToEditor();
                return;
            case StateEnum.ChooseEditorLevel:
                StartEditor();
                return;
            case StateEnum.MapEditorPause:
                BackToEditor();
                return;
        }
    }

    private void GotoMapEditorPauseAction()
    {
        Debug.Log("GotoMapEditorPauseAction");
        if (EditorPauseUI == null)
        {
            EditorPauseUI = StateList[presentStateIndex].UIObj.transform.Find("PauseUI").gameObject;
        }

        EditorPauseUI.SetActive(true);
    }


    
    #endregion

    public void StartGame()
    {
        //start the game from the beginning
        GlobalParameters.Instance.ResetLevel();
    }

    private void BackToGame()
    {
        //just return to game
    }

    private void StartEditor()
    {
        
    }

    private void BackToEditor()
    {
        
    }


    public void OpenMenu()
    {
        //pause the game
        //present tool item UI, each UI is a button containing an action with index as a reference

        menuObj.SetActive(true);
        resultUI.SetActive(false);
    }

    


    public void ClosePauseGameUI()
    {
        GamePlayPauseUI.SetActive(false);
    }

    private void ClosePauseEditorUI()
    {
        EditorPauseUI.SetActive(false);
    }

    public void CloseMenus()
    {
        // Hide UI
        //continue the game
        resultUI.SetActive(false);
        menuObj.SetActive(false);
    }


    public void ShowResult(string result)
    {
        menuObj.SetActive(true);
        resultUI.SetActive(true);
        resultUI.GetComponentInChildren<TMP_Text>().text = result;
    }


}