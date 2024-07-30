using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class BagManager : MonoBehaviour
{

    public GameObject BagUI;
    public List<BagTool> currentBagList = new List<BagTool>();
    public int presentBagCapacity = 3;

    private List<GameObject> BagItemUIList = new List<GameObject>();
    
    public static BagManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
        InitBag();
    }

    public void InitBag()
    {
        GetBagItemUIList();
        UpdateAllTools();
    }

    private void GetBagItemUIList()
    {
        int uiItemCount = BagUI.transform.childCount;
        for (int i = 0; i < uiItemCount; i++)
        {
            if (BagUI.transform.GetChild(i).name != "Title")
            {
                BagItemUIList.Add(BagUI.transform.GetChild(i).gameObject);
                BagItemUIList[i].SetActive(false);
            }
            
        }
    }

    public void UpdateAllTools()
    {
        for (int i = 0; i < currentBagList.Count; i++)
        {
            BagItemUIList[i].SetActive(true);
            UpdateSprite(currentBagList[i], i);
        }
    }

    public void AddTool(BagTool tool)
    {
        if (currentBagList.Count < presentBagCapacity)
        {
            BagItemUIList[currentBagList.Count].SetActive(true);
            UpdateSprite(tool, currentBagList.Count);
            currentBagList.Add(tool);
        }
           
    }

    public void DeleteSelectedTool()
    {
        //delete the first item
        if (currentBagList.Count > 0)
        {
            BagItemUIList[currentBagList.Count - 1].SetActive(false);
            currentBagList.RemoveAt(0);

            for (int i = 0; i < currentBagList.Count; i++)
            {
                UpdateSprite(currentBagList[i], i);
            }
        }
    }

    private void UpdateSprite(BagTool tool, int index)
    {
        BagItemUIList[index].GetComponent<Image>().sprite =
            SpriteManager.Instance.ReturnToolSprite((int)tool.toolID);
        GlobalMethod.OperateUIDirection(BagItemUIList[index].gameObject, (int)tool.toolDirection);
    }


    public BagTool PresentSelectedBagTool()
    {
        if (currentBagList.Count > 0)
            return currentBagList[0];
        else
        {
            return new BagTool(ToolEnum.Block, ToolDirection.Original);
        }
    }

    public bool IsFull()
    {
        if (presentBagCapacity > currentBagList.Count)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

}
