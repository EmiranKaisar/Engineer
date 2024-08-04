using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SlotVeiwManager : MonoBehaviour
{
    //show which progress is highlighted now
    //
    public List<GameObject> slotObjList;

    private int formerHighlightedObjIndex = -1;

    private List<Progress> progressList = new List<Progress>();

    public void InitSlotView()
    {
        //show played data if there is
        GetProgressList();

        //highlight the first slot in default
        GotHightLight(0);
    }

    private void GetProgressList()
    {
        progressList.Clear();
        for (int i = 0; i < slotObjList.Count; i++)
        {
            progressList.Add(SaveSystem.LoadProgress(i));
            PutProgressDataOnSlot(i);
            AddButtonListener(i);
        }
    }

    private void PutProgressDataOnSlot(int index)
    {
        if (progressList[index].levelResultlist.Count > 0)
        {
            slotObjList[index].transform.Find("Level").GetComponent<TMP_Text>().text = "Level: " + progressList[index].levelResultlist.Count;
            slotObjList[index].transform.Find("Time").GetComponent<TMP_Text>().text = "Time: " + WholeTime(index);
            slotObjList[index].transform.Find("Operation").GetComponent<TMP_Text>().text = "Operation: " + WholeOperation(index);
            slotObjList[index].transform.Find("Date").GetComponent<TMP_Text>().text = "Date: " + progressList[index].lastPlayDate;
            slotObjList[index].transform.Find("Add").GetComponent<TMP_Text>().text = "";
        }
        else
        {
            slotObjList[index].transform.Find("Level").GetComponent<TMP_Text>().text = "";
            slotObjList[index].transform.Find("Time").GetComponent<TMP_Text>().text = "";
            slotObjList[index].transform.Find("Operation").GetComponent<TMP_Text>().text = "";
            slotObjList[index].transform.Find("Date").GetComponent<TMP_Text>().text = "";
            slotObjList[index].transform.Find("Add").GetComponent<TMP_Text>().text = "+";
        }
        
    }

    public void GotHightLight(int index)
    {
        if (index != formerHighlightedObjIndex)
        {
            slotObjList[index].GetComponent<Image>().color = Color.yellow;
            
            if(formerHighlightedObjIndex >=0 && formerHighlightedObjIndex < slotObjList.Count)
                slotObjList[formerHighlightedObjIndex].GetComponent<Image>().color = Color.white;
            
            formerHighlightedObjIndex = index;
        }
    }
    
    private void AddButtonListener(int objIndex)
    {
        slotObjList[objIndex].GetComponent<Button>().onClick
            .AddListener(() => PickSlotButtonAction(objIndex));
    }


    private void PickSlotButtonAction(int index)
    {
        GotHightLight(index);
        GameManager.Instance.SelectProgress(index);
    }


    private string WholeTime(int index)
    {
        float time = 0;
        foreach (var result in progressList[index].levelResultlist)
        {
            time += result.timeDur;
        }

        return System.TimeSpan.FromSeconds(time).ToString("hh':'mm':'ss");;
    }


    private string WholeOperation(int index)
    {
        int op = 0;
        foreach (var result in progressList[index].levelResultlist)
        {
            op += result.operationCount;
        }

        return op.ToString();
    }


    

}
