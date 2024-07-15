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

    [SerializeField]
    public List<BagTool> toolStoreList;
    
    
    public GameObject toolStoreUI;
    
    public GameObject resultUI;
    
    private List<GameObject> toolItemUIList = new List<GameObject>();

    public static GameManager Instance
    {
        get;
        private set;
    }
    private void Awake()
    {
        if (Instance != null && Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        InitGame();
    }

    private void Start()
    {
        playerObj.transform.position = playerInitPos;
    }

    private void InitGame()
    {
        menuObj.SetActive(true);
        resultUI.SetActive(false);
        //get tool item UI obj
        GetToolItemUIObjList();
        //update appearance, bond action to it.
        UpdateToolStoreItemUI();
        //pause the game
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
            int toolID = toolStoreList[i].toolID;
            toolItemUIList[i].SetActive(true);
            toolItemUIList[i].GetComponent<Image>().sprite = SpriteManager.Instance.ReturnToolSprite(toolID);
            //operate direction
            OperateUIDirection(toolItemUIList[i], toolStoreList[i].toolDirection);
            AddButtonListener(i);
        }
    }
    
    //original sprite should be operated due to the localRotation
    //tool sprite should be with worldRotation
    public void OperateUIDirection(GameObject obj, int toolDir)
    {
        if (toolDir <= 3)
        {
            obj.transform.rotation = Quaternion.AngleAxis(90*toolDir, Vector3.forward);
        }
        else
        {
            obj.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
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
    }


    
    

    public void OpenMenu()
    {
        //pause the game
        //present tool item UI, each UI is a button containing an action with index as a reference
        
        menuObj.SetActive(true);
        resultUI.SetActive(false);
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
