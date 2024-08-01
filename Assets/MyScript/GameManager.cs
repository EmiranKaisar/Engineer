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

    [HideInInspector] public Progress presentProgress;

    #region Level Result
    public float levelTime;

    public class PlayerResultClass
    {
        public PlayerResultClass(bool success, int count)
        {
            PlayerSuccess = success;
            OperationCount = count;
        }
        public bool PlayerSuccess;
        public int OperationCount;
    }

    private List<PlayerResultClass> playerResultList = new List<PlayerResultClass>();

    private bool gotResult = false;
    #endregion

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

    private bool gotPaused = false;

    private int homeIndex = 0;
    private int presentStateIndex = 0;
    private int previousStateIndex = 0;

    #region Supplementary UI
    
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

    #endregion


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

    // Update is called once per frame
    void Update()
    {
        if(!gotPaused)
           levelTime += Time.deltaTime;
        
        if (Input.GetKeyDown(KeyCode.Escape) && StateList[presentStateIndex].ThisState == StateEnum.GamePlay)
        {
            StateButtonAction((int)StateEnum.GamePlayPause);
        }
        
        if(Input.GetKeyDown(KeyCode.L) && presentProgress != null)
            LogAllResults();
    }

    private void LogAllResults()
    {
        int index = 0;
        foreach (var item in presentProgress.levelResultlist)
        {
            Debug.Log("level: " + index + "; time: " + item.timeDur + "; operation: " + item.operationCount);
            index++;
        }
    }

    #region Init
    private void CheckState()
    {
        for (int i = 0; i < StateList.Count; i++)
        {
            AssignObj(i);
            AssignAction(i);
            if ((StateEnum)i != StateList[i].ThisState)
            {
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
        InitPlayerResultList();
        InitProgress();
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
    
    private void InitPlayerResultList()
    {
        playerResultList.Clear();
        for (int i = 0; i < 4; i++)
        {
            playerResultList.Add(new PlayerResultClass(false, 0));
        }
    }


    private void InitProgress()
    {
        presentProgress = SaveSystem.LoadProgress(0);
        presentProgress.slot = 0;
    }
    
    #endregion

    #region Change state

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

    //更新状态
    private void UpdateState()
    {
        if (StateList[presentStateIndex].UIObj != StateList[previousStateIndex].UIObj)
        {
            HideUI(previousStateIndex);
            ShowUI(presentStateIndex);
        }

        StateList[presentStateIndex].stateAction?.Invoke();
    }

    //隐藏对应UI
    private void HideUI(int _index)
    {
        StateList[_index].UIObj.SetActive(false);
    }

    //显示对应的UI
    private void ShowUI(int _index)
    {
        StateList[_index].UIObj.SetActive(true);
    }

    #endregion

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
        //set parameters
        Time.timeScale = 1;
        gotPaused = false;
        
        //set UI
        GamePlayPauseUI.SetActive(false);
        HintUI.SetActive(false);
        if (starGlowObj != null)
            starGlowObj.SetActive(false);

        

        if (StateList[previousStateIndex].ThisState != StateEnum.GamePlayPause &&
            StateList[previousStateIndex].ThisState != StateEnum.GamePlayHint)
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
            if (playerResultList[0].PlayerSuccess)
            {
                gamePlayPauseTitle.text = "Success !";
            }
            else
            {
                gamePlayPauseTitle.text = "Fail !";
            }

            //gamePlayPausePlayerName.text = playerList[presentLevelResult.playerIndex].name;

            gamePlayPausePlayerName.text = "Operation: " + playerResultList[0].OperationCount;
            gamePlayPauseTime.text = "Time: " + levelTime.ToString("F1");
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
        //hide and show UI
        EditorButtonsObj.SetActive(true);
        GameButtonsObj.SetActive(false);

        //load level data
        selectedLevelIndex = 0;
        GlobalParameters.Instance.LoadLevel(selectedLevelIndex);

        //load level preview list for scroll view
        LevelScrollView.GetComponent<ScrollViewManager>().ShowLevel();

        if (StateList[previousStateIndex].ThisState == StateEnum.MapEditor)
            ConfirmExistEditor();
    }

    private void GotoMapEditorAction()
    {
        EditorPauseUI.SetActive(false);
        switch (StateList[previousStateIndex].ThisState)
        {
            case StateEnum.ChooseEditorLevel:
                StartEditor();
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

    #region Supplementary state change action

    public void StartGame()
    {
        //start the game from the beginning
        gotResult = false;
        levelTime = 0;
        ClearResult();
        if (GlobalParameters.Instance.presentLevel.levelID != selectedLevelIndex)
            GlobalParameters.Instance.LoadLevel(selectedLevelIndex);


        GlobalParameters.Instance.EditMode(false);

        BagManager.Instance.BagUI.transform.SetParent(StateList[(int)StateEnum.GamePlay].UIObj.transform);


        GlobalParameters.Instance.ResetLevel();

        StartPlayerComponents();
        AudioManager.Instance.AssignPlayerAudioSource();
        
        if(presentProgress.levelResultlist.Count <= selectedLevelIndex)
            presentProgress.levelResultlist.Add(new LevelResult());

        presentProgress.lastPlayDate = DateTime.Now.Date.ToString("MM/dd/yyyy HH:mm");

        if (!presentProgress.levelResultlist[selectedLevelIndex].hinted)
        {
            if (GlobalParameters.Instance.presentLevel.levelDescription.thisImage != null)
            {
                StateButtonAction((int)StateEnum.GamePlayHint);
                
            }
            
            presentProgress.levelResultlist[selectedLevelIndex].hinted = true;
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

        if (GlobalParameters.Instance.presentLevel.levelID != selectedLevelIndex)
            GlobalParameters.Instance.LoadLevel(selectedLevelIndex);

        levelNameText.text = GlobalParameters.Instance.presentLevel.levelName;


        GlobalParameters.Instance.EditMode(true);

        BagManager.Instance.BagUI.transform.SetParent(StateList[(int)StateEnum.MapEditor].UIObj.transform);
    }
    

    private void ConfirmExistEditor()
    {
        GlobalParameters.Instance.EditMode(false);
    }

    public void SaveLevelInfo()
    {
        //get present level info
        GlobalParameters.Instance.GetInfo();

        GlobalParameters.Instance.EditMode(false);
    }

    public void QuitGame()
    {
        SaveSystem.SetProgress(presentProgress.slot);
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

    private void SetSuccessProgress()
    {
        presentProgress.lastPlayDate = DateTime.Now.Date.ToString("MM/dd/yyyy HH:mm");
        presentProgress.levelResultlist[selectedLevelIndex].hasPassed = true;
        presentProgress.levelResultlist[selectedLevelIndex].timeDur = levelTime;
        presentProgress.levelResultlist[selectedLevelIndex].operationCount = playerResultList[0].OperationCount;
        SaveSystem.SetProgress(presentProgress.slot);
    }

    #endregion
    
    #region API

    public void IncrementPlayerOperactionCount(int playerIndex)
    {
        playerResultList[playerIndex].OperationCount++;
    }

    public void ClearResult()
    {
        foreach (var item in playerResultList)
        {
            item.PlayerSuccess = false;
            item.OperationCount = 0;
        }
    }

    public void SetResult(int playerIndex, bool success)
    {
        playerResultList[playerIndex].PlayerSuccess = success;
        gotResult = true;

        if (success)
        {
            //save the result as local data
            SetSuccessProgress();
        }
        else
        {
            StateButtonAction((int)StateEnum.GamePlayPause);
        }
        
    }
    
    

    public bool SelectLevel(int index)
    {
        if (index == 0)
        {
            selectedLevelIndex = index;
            return true;
        }
            
        
        int formerIndex = index - 1;
        if (presentProgress.levelResultlist.Count > formerIndex)
        {
            if (presentProgress.levelResultlist[formerIndex].hasPassed)
            {
                selectedLevelIndex = index;
                return true;
            }
        }
        
        //in editor we don't care if player passed the former level
        if (StateList[presentStateIndex].ThisState == StateEnum.ChooseEditorLevel)
        {
            selectedLevelIndex = index;
            return true;
        }

        return false;
    }

    public void PlayStarGlowAnimation(GameObject targetObj)
    {
        if (starAnimation != null)
            StopCoroutine(starAnimation);
        starAnimation = StarAnimation(targetObj);
        StartCoroutine(starAnimation);
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