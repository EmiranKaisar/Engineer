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

    public GameObject content;
    
    private int previousSelectedObjIndex = -1;
    
    public void ShowLevel()
    {
        InitScrollView();
        AssignAllObjIndex();
        AssignAllData();
    }

    private void InitScrollView()
    {
        previousSelectedObjIndex = -1;
        GetItemObj();
        HideAllObj();

        InitAppearance();
        UpdateSelectedAppearance(0);
    }

    private void InitAppearance()
    {
        for (int i = 0; i < levelObjList.Count; i++)
        {
            HighLight(i, false);
        }
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




    private void AssignAllObjIndex()
    {
        for (int i = 0; i < GameManager.Instance.levelPreviewList.Count; i++)
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
        int dataCount = GameManager.Instance.levelPreviewList.Count;
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
        levelObjList[objIndex].levelUI.GetComponentInChildren<TMP_Text>().text = GameManager.Instance.levelPreviewList[levelIndex].levelName;
    }
    
    
    private void AddButtonListener(int objIndex)
    {
        levelObjList[objIndex].levelUI.GetComponent<Button>().onClick.AddListener(() => PickUIObjButtonAction(objIndex));
    }

    private void PickUIObjButtonAction(int objIndex)
    {
        int levelIndex = levelObjList[objIndex].levelIndex;
        if (CanSelect(levelIndex) || StateEnum.ChooseEditorLevel == GameManager.Instance.PresentState())
        {
            UpdateSelectedAppearance(objIndex);
            GameManager.Instance.SelectLevel(levelIndex);
        }
        
    }

    private void UpdateSelectedAppearance(int index)
    {
        if (index != previousSelectedObjIndex)
        {
            //update appearance
            HighLight(index, true);
            HighLight(previousSelectedObjIndex, false);
            previousSelectedObjIndex = index;
        }
    }

    private void HighLight(int index, bool highLight)
    {
        if (index >= 0 && index < levelObjList.Count)
        {
            if (highLight)
            {
                levelObjList[index].levelUI.GetComponent<Image>().color = Color.yellow;
            }
            else
            {
                levelObjList[index].levelUI.GetComponent<Image>().color = Color.white;
            }
            
        }
    }

    private bool CanSelect(int index)
    {
        if (index == 0)
        {
            return true;
        }
        
        if (index > 0 && index < levelObjList.Count)
        {
            if (GameManager.Instance.levelPreviewList[index - 1].passed)
            {
                return true;
            }

            return false;
            
        }

        return false;
        
    }
    
    

    // Update is called once per frame
    void Update()
    {
        //update ui position
        //checkout "HomeUIManager" in layer demo
    }
}
