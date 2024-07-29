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
    [SerializeField] public List<BagTool> toolStoreList;

    public GameObject starGlowPrefab;
    
    public LevelPreviewList levelPreviewList;

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

    private bool gotResult = false;

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

    private GameObject HintUI;
    private TMP_Text gamePlayHintTitle;
    private TMP_Text gamePlayHintDescription;
    private Image gamePlayHintImage;

    private GameObject LevelScrollView;
    
    private TMP_Text gamePlayPauseTitle;
    private TMP_Text gamePlayPausePlayerName;
    private TMP_Text gamePlayPauseTime;

    private TMP_Text levelNameText;

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
        
        DontDestroyOnLoad(Instance);
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
                AssignGamePlayHintUI(index);
                AssignGamePlayPauseUI(index);
                return;
            case StateEnum.MapEditorPause:
                EditorPauseUI = StateList[index].UIObj.transform.Find("PauseUI").gameObject;
                levelNameText = StateList[index].UIObj.transform.Find("Title").GetComponent<TMP_Text>();
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
            case StateEnum.GamePlayHint:
                StateList[index].stateAction += GotoGamePlayHintAction;
                return;
        }
    }

    private void AssignGamePlayHintUI(int index)
    {
        HintUI = StateList[index].UIObj.transform.Find("HintUI").gameObject;
        gamePlayHintTitle = HintUI.transform.Find("Title").GetComponent<TMP_Text>();
        gamePlayHintDescription = HintUI.transform.Find("Description").GetComponent<TMP_Text>();
        gamePlayHintImage = HintUI.transform.Find("Image").GetComponent<Image>();
        HintUI.SetActive(false);
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
        if (Input.GetKeyDown(KeyCode.Escape) && StateList[presentStateIndex].ThisState == StateEnum.GamePlay)
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
        
        selectedLevelIndex = 0;

        GlobalParameters.Instance.LoadLevel(selectedLevelIndex);
        
        //load local level data
        LevelScrollView.GetComponent<ScrollViewManager>().ShowLevel();
    }
    
    private void GotoGamePlayAction()
    {
        Time.timeScale = 1;
        ClosePauseGameUI();
        CloseHintUI();
        if(starGlowObj != null)
            starGlowObj.SetActive(false);

        if (StateList[previousStateIndex].ThisState != StateEnum.GamePlayPause && StateList[previousStateIndex].ThisState != StateEnum.GamePlayHint)
        {
            StartGame();
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
        

        selectedLevelIndex = 0;
        
        GlobalParameters.Instance.LoadLevel(selectedLevelIndex);
        
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

    private void GotoGamePlayHintAction()
    {
        Time.timeScale = 0;
        
        //show hint
        HintUI.SetActive(true);

        if (GlobalParameters.Instance.presentLevel.levelDescription.thisImage != null)
        {
            gamePlayHintTitle.text = GlobalParameters.Instance.presentLevel.levelDescription.descriptionTitle;
            gamePlayHintDescription.text = GlobalParameters.Instance.presentLevel.levelDescription.description;
            gamePlayHintImage.sprite = GlobalParameters.Instance.presentLevel.levelDescription.thisImage;
        }
        else
        {
            gamePlayHintTitle.text = "No hint for this level";
            gamePlayHintDescription.text = "";
        }

        

    }

    
    #endregion

    public void StartGame()
    {
        //start the game from the beginning
        gotResult = false;
        if(GlobalParameters.Instance.presentLevel.levelID != selectedLevelIndex)
             GlobalParameters.Instance.LoadLevel(selectedLevelIndex);
        
        
        GlobalParameters.Instance.EditMode(false);
        
        BagManager.Instance.BagUI.transform.SetParent(StateList[(int)StateEnum.GamePlay].UIObj.transform);
        
        
        GlobalParameters.Instance.ResetLevel();
        
        StartPlayerComponents();

        if (!levelPreviewList.previewList[selectedLevelIndex].hinted)
        {
            if (GlobalParameters.Instance.presentLevel.levelDescription.thisImage != null)
            {
                StateButtonAction((int)StateEnum.GamePlayHint);
                levelPreviewList.previewList[selectedLevelIndex].hinted = true;
            }
        }
            
            
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
        
        if(GlobalParameters.Instance.presentLevel.levelID != selectedLevelIndex)
            GlobalParameters.Instance.LoadLevel(selectedLevelIndex);

        levelNameText.text = GlobalParameters.Instance.presentLevel.levelName;

        
        GlobalParameters.Instance.EditMode(true);
        
        BagManager.Instance.BagUI.transform.SetParent(StateList[(int)StateEnum.MapEditor].UIObj.transform);
    }

    public void ConfirmExistEditor()
    {
        GlobalParameters.Instance.EditMode(false);
    }
    
    private void ClosePauseGameUI()
    {
        GamePlayPauseUI.SetActive(false);
    }

    private void CloseHintUI()
    {
        HintUI.SetActive(false);
    }

    private void ClosePauseEditorUI()
    {
        EditorPauseUI.SetActive(false);
    }

    public void SelectLevel(int index)
    {
        selectedLevelIndex = index;
    }

    public void SaveLevelInfo()
    {
        //get present level info
        GlobalParameters.Instance.GetInfo();

        GlobalParameters.Instance.EditMode(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void NextLevel()
    {
        int nextLevelIndex = selectedLevelIndex + 1;
        if (nextLevelIndex < levelPreviewList.previewList.Count)
        {
            SelectLevel(nextLevelIndex);
            StateButtonAction(3);
            StartGame();
        }
        else
        {
            //exit game
            StateButtonAction(2);
        }
            
    }
    

    public void PlayStarGlowAnimation(GameObject targetObj)
    {
        if(starAnimation != null)
            StopCoroutine(starAnimation);
        starAnimation = StarAnimation(targetObj);
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
    private IEnumerator StarAnimation(GameObject targetObj)
    {
        if (starGlowObj == null)
        {
            starGlowObj = Instantiate(starGlowPrefab, targetObj.transform.position, quaternion.identity);
        }
        
        starGlowObj.transform.rotation = Quaternion.identity;
        starGlowObj.transform.position = targetObj.transform.position;
        starGlowObj.transform.SetParent(targetObj.transform);
        starGlowObj.SetActive(true);
        yield return starAnimDur;
        StateButtonAction((int)StateEnum.GamePlayPause);
        
    }

    #endregion


}