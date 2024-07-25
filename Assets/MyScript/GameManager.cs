using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public GameObject menuObj;

    [SerializeField] public List<BagTool> toolStoreList;

    public GameObject starGlowPrefab;

    public GameObject resultUI;

    public float levelTime;
    
    public struct LevelResult
    {
        public LevelResult(int index, bool success, float dur)
        {
            playerIndex = index;
            playerSuccess = success;
            timeDur = dur;
        }
        public int playerIndex;
        public bool playerSuccess;
        public float timeDur;
    }

    public LevelResult presentLevelResult;

    public string presentLevelName;

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

    public List<LevelPreview> levelPreviewList = new List<LevelPreview>();

    private int homeIndex = 0;
    private int presentStateIndex = 0;
    private int previousStateIndex = 0;
    
    private GameObject GamePlayPauseUI;
    private GameObject EditorPauseUI;
    private GameObject GameButtonsObj;
    private GameObject EditorButtonsObj;

    private GameObject LevelScrollView;
    
    private TMP_Text gamePlayPauseTitle;
    private TMP_Text gamePlayPausePlayerName;
    private TMP_Text gamePlayPauseTime;

    private TMP_InputField levelNameInputText;

    private int selectedLevelIndex = 0;

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
                LevelScrollView = StateList[index].UIObj.transform.Find("ScrollView").gameObject;
                return;
            case StateEnum.GamePlayPause:
                AssignGamePlayPauseUI(index);
                return;
            case StateEnum.MapEditorPause:
                EditorPauseUI = StateList[index].UIObj.transform.Find("PauseUI").gameObject;
                levelNameInputText = StateList[index].UIObj.transform.Find("LevelName").GetComponent<TMP_InputField>();
                EditorPauseUI.SetActive(false);
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
        GamePlayPauseUI.SetActive(false);
    }

    private void InitGame()
    {
        Time.timeScale = 0;
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
        
        SaveSystem.SetLevelPreviewList(levelPreviewList);
        
        selectedLevelIndex = 0;
        
        GlobalParameters.Instance.presentLevel = SaveSystem.LoadLevel(selectedLevelIndex);
        
        //load local level data
        LevelScrollView.GetComponent<ScrollViewManager>().ShowLevel();
    }
    
    private void GotoGamePlayAction()
    {
        Time.timeScale = 1;
        ClosePauseGameUI();
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
        
        SaveSystem.SetLevelPreviewList(levelPreviewList);

        selectedLevelIndex = 0;
        GlobalParameters.Instance.presentLevel = SaveSystem.LoadLevel(selectedLevelIndex);
        
        //load local level data
        LevelScrollView.GetComponent<ScrollViewManager>().ShowLevel();
    }

    private void GotoMapEditorAction()
    {
        switch (StateList[previousStateIndex].ThisState)
        {
            case StateEnum.ChooseEditorLevel:
                ClosePauseEditorUI();
                StartEditor();
                return;
            case StateEnum.MapEditorPause:
                ClosePauseEditorUI();
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
        if(GlobalParameters.Instance.presentLevel.levelID != selectedLevelIndex)
             GlobalParameters.Instance.presentLevel = SaveSystem.LoadLevel(selectedLevelIndex);
        
        //hide editor level objs
        GlobalParameters.Instance.ShowEditorLevelObjs(false);
        //show level objs
        GlobalParameters.Instance.ShowLevelObjs(true);
        
        BagManager.Instance.BagUI.transform.SetParent(StateList[(int)StateEnum.GamePlay].UIObj.transform);
        
        
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

    private void StartEditor()
    {
        //update selected level id before this
        Time.timeScale = 0;
        
        GlobalParameters.Instance.presentLevel.levelID = selectedLevelIndex;
        
        if (levelPreviewList.Count > selectedLevelIndex)
        {
            levelNameInputText.text = levelPreviewList[selectedLevelIndex].levelName;
        }
             
        
        //show editor level objs
        GlobalParameters.Instance.ShowEditorLevelObjs(true);
        //hide level objs
        GlobalParameters.Instance.ShowLevelObjs(false);
        
        BagManager.Instance.BagUI.transform.SetParent(StateList[(int)StateEnum.MapEditor].UIObj.transform);
    }

    public void ConfirmExistEditor()
    {
        GlobalParameters.Instance.ShowEditorLevelObjs(false);
    }
    
    public void ClosePauseGameUI()
    {
        GamePlayPauseUI.SetActive(false);
    }

    private void ClosePauseEditorUI()
    {
        EditorPauseUI.SetActive(false);
    }


    public void CreateNewLevel()
    {
        int presentLevelCount = levelPreviewList.Count;
        SelectLevel(presentLevelCount);
        GlobalParameters.Instance.presentLevel = new LevelInfo();
        GlobalParameters.Instance.presentLevel.levelID = presentLevelCount;
    }

    public void SelectLevel(int index)
    {
        selectedLevelIndex = index;
    }

    public void SaveLevelInfo()
    {
        presentLevelName = levelNameInputText.text;
        
        GlobalParameters.Instance.GetInfo();
        
        //check if this leve is created
        if (GlobalParameters.Instance.presentLevel.levelID >= levelPreviewList.Count)
        {
            levelPreviewList.Add(new LevelPreview(levelPreviewList.Count, presentLevelName));
        }
        else
        {
            levelPreviewList[selectedLevelIndex].levelName = presentLevelName;
        }
           
        
        SaveSystem.SaveLevel(GlobalParameters.Instance.presentLevel);
        SaveSystem.SaveLevelPreviewList(levelPreviewList);
        GlobalParameters.Instance.ShowEditorLevelObjs(false);
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
        else
        {
            levelPreviewList[selectedLevelIndex].passed = true;
            SaveSystem.SaveLevelPreviewList(levelPreviewList);
        }
        
    }

    #region API


    public StateEnum PresentState()
    {
        return StateList[presentStateIndex].ThisState;
    }

    #endregion
    

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