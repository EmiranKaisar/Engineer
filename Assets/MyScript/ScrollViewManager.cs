using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScrollViewManager : MonoBehaviour
{
    public class LevelObj
    {
        public LevelObj(int index, GameObject obj)
        {
            levelIndex = index;
            levelUI = obj;
        }
        public int levelIndex;
        public GameObject levelUI;
    }

    private List<LevelObj> levelObjList = new List<LevelObj>();
    
    private List<string> levelList = new List<string>();

    public GameObject content;
    
    
    // Start is called before the first frame update
    void Start()
    {
        //InitScrollView();
    }

    private void InitScrollView()
    {
        GetItemObj();
        HideAllObj();
        GetLevelList();
    }

    private void GetItemObj()
    {
        levelObjList.Clear();
        int index = 0;
        foreach (Transform itemTransform in content.transform)
        {
            levelObjList.Add(new LevelObj(index, itemTransform.gameObject));
            index++;
        }
    }

    private void HideAllObj()
    {
        foreach (var item in levelObjList)
        {
            item.levelUI.SetActive(false);
        }
    }

    
    private void GetLevelList()
    {
        levelList = SaveSystem.FileList(); 
    }

    public void ShowLevel()
    {
        InitScrollView();
        AssignAllObjIndex();
        AssignAllData();
    }

    private void AssignAllObjIndex()
    {
        for (int i = 0; i < levelList.Count; i++)
        {
            AssignObjIndex(i, i);
            AddButtonListener(i);
        }

    }

    private void AssignObjIndex(int levelIndex, int objIndex)
    {
        levelObjList[objIndex].levelIndex = levelIndex;
    }

    private void AssignAllData()
    {
        int dataCount = levelList.Count;
        int uiObjCount = levelObjList.Count;
        
        for (int i = 0; i < uiObjCount; i++)
        {
            if(i < dataCount)
               AssignData(i);
            else
            {
                levelObjList[i].levelUI.SetActive(false);
            }
                
        }
        
    }

    private void AssignData(int objIndex)
    {
        int levelIndex = levelObjList[objIndex].levelIndex;
        levelObjList[objIndex].levelUI.SetActive(true);
        levelObjList[objIndex].levelUI.GetComponentInChildren<TMP_Text>().text = levelList[levelIndex];
    }
    
    
    private void AddButtonListener(int objIndex)
    {
        levelObjList[objIndex].levelUI.GetComponent<Button>().onClick.AddListener(() => PickUIObjButtonAction(objIndex));
    }

    private void PickUIObjButtonAction(int objIndex)
    {
        int levelIndex = levelObjList[objIndex].levelIndex;
        Debug.Log("picked :" + levelIndex);
    }
    
    

    // Update is called once per frame
    void Update()
    {
        //update ui position
        //checkout "HomeUIManager" in layer demo
    }
}
