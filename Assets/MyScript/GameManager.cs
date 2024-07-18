using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;

public class GameManager : MonoBehaviour
{
    //Defing
    public GameObject playerObj;

    public Vector3 playerInitPos;

    public GameObject menuObj;

    [SerializeField] public List<BagTool> toolStoreList;


    public GameObject toolStoreUI;

    public GameObject starGlowPrefab;

    public GameObject resultUI;

    public float levelTime;

    public LevelResult presentLevelResult;
    
    

    private bool gotResult = false;

    private List<GameObject> toolItemUIList = new List<GameObject>();

    public List<GameObject> playerList = new List<GameObject>();
    public delegate void StateChangeAction();

    [Serializable]
    public class StateClass
    {
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
    
    private TMP_Text gamePlayPauseTitle;
    private TMP_Text gamePlayPausePlayerName;
    private TMP_Text gamePlayPauseTime;

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
                Debug.Log("State " + StateList[i].ThisState + "is not in right position");
            }
            
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
                AssignGamePlayPauseUI(index);
                //GamePlayPauseUI = StateList[index].UIObj.transform.Find("PauseUI").gameObject;
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

    private void AssignGamePlayPauseUI(int index)
    {
        GamePlayPauseUI = StateList[index].UIObj.transform.Find("PauseUI").gameObject;
        gamePlayPauseTitle = GamePlayPauseUI.transform.Find("Title").GetComponent<TMP_Text>();
        gamePlayPausePlayerName = GamePlayPauseUI.transform.Find("PlayerName").GetComponent<TMP_Text>();
        gamePlayPauseTime = GamePlayPauseUI.transform.Find("Time").GetComponent<TMP_Text>();
    }

    private void Start()
    {
        playerObj.transform.position = playerInitPos;
    }

    private void InitGame()
    {
        
        InitUI();
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
        Time.timeScale = 1;
        if(starGlowObj != null)
            starGlowObj.SetActive(false);
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
        Time.timeScale = 0;
        
        GamePlayPauseUI.SetActive(true);

        if (gotResult)
        {
            if (presentLevelResult.playerSuccess)
            {
                gamePlayPauseTitle.text = "Success !";
            }
            else
            {
                gamePlayPauseTitle.text = "Fail !";
            }

            gamePlayPausePlayerName.text = playerList[presentLevelResult.playerIndex].name;

            gamePlayPauseTime.text = levelTime.ToString();
        }
        else
        {
            gamePlayPauseTitle.text = "";
            gamePlayPausePlayerName.text = "";
            gamePlayPauseTime.text = "";
        }


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
        gotResult = false;
        GlobalParameters.Instance.ResetLevel();
        StartPlayerComponents();
    }

    private void StartPlayerComponents()
    {
        foreach (var obj in playerList)
        {
            obj.GetComponent<PlayerController>().PlayerAlive();
        }
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


    public void PlayStarGlowAnimation(Vector3 targetPos)
    {
        if(starAnimation != null)
            StopCoroutine(starAnimation);
        starAnimation = StarAnimation(targetPos);
        StartCoroutine(starAnimation);
    }
    
    
    public void SetResult(int playerIndex, bool success)
    {
        presentLevelResult = new LevelResult(playerIndex, success, levelTime);

        gotResult = true;
        //save the result as local data
        if (!success)
        {
            StateButtonAction((int)StateEnum.GamePlayPause);
        }
        
    }
    


    public void ShowResult(string result)
    {
        menuObj.SetActive(true);
        resultUI.SetActive(true);
        resultUI.GetComponentInChildren<TMP_Text>().text = result;
    }

    #region Animation

    //private float starAnimDur = .4f;
    private WaitForSeconds starAnimDur = new WaitForSeconds(0.4f);
    private GameObject starGlowObj;
    private IEnumerator starAnimation;
    private IEnumerator StarAnimation(Vector2 targetPos)
    {
        if (starGlowObj == null)
        {
            starGlowObj = Instantiate(starGlowPrefab, targetPos, quaternion.identity);
        }
        starGlowObj.SetActive(true);
        yield return starAnimDur;
        StateButtonAction((int)StateEnum.GamePlayPause);
        
    }

    #endregion


}