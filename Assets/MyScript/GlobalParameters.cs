using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GlobalParameters : MonoBehaviour
{
    //for now, only save the current level info
    //the basics are stickable obj
    //there are 3 types of tool: mover, rotater, killer
    //there are instantiate points for players
    //flag points, flag target points
    
    public float rotateToolDur = 2;

    public float moveToolSpeed = 1;

    public GameObject playerObj;
    public GameObject levelObjs;

    public LevelInfo presentLevel = new LevelInfo();
    

    public static GlobalParameters Instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        
        
    }



    // Start is called before the first frame update
    void Start()
    {
        
        GetInfo();
    }
    
    
    //call this after this level is set up
    private void GetInfo()
    {
        //get player position
        SetFloatPosition(presentLevel.playerInstantiatePos, playerObj.transform.position);
        
        //get all other objs info
        SetPropInfo(levelObjs.transform);
        
        //get all tools in bag
        SetBagToolInfo();
    }

    private void SetFloatPosition(float[] floatPos ,Vector3 vectorPos)
    {
        floatPos[0] = vectorPos.x;
        floatPos[1] = vectorPos.y;
        floatPos[2] = vectorPos.z;
    }

    private void SetPropInfo(Transform transform)
    {
        foreach (Transform childObj in transform)
        {
            if (childObj.CompareTag("Chunk"))
            {
                foreach (var item in childObj.GetComponent<ChunkClass>().chunkChildList)
                {
                    presentLevel.scenePropTools.Add(new PropTool((int)item.toolID, (int)item.toolDir, item.stickablObj.transform.position));
                }
                
            }
        }
    }

    private void SetBagToolInfo()
    {
        foreach (var item in BagManager.Instance.currentBagList)
        {
            presentLevel.bagTools.Add(new(item.toolID, item.toolDirection));
        }
    }

    public void ResetLevel()
    {
        playerObj.transform.position = ReturnVector(presentLevel.playerInstantiatePos);
        ResetPropInfo(levelObjs.transform);
        ResetBagToolInfo();
    }

    private void ResetPropInfo(Transform transform)
    {
        int index = 0;
        foreach (Transform childObj in transform)
        {
            if (childObj.CompareTag("Chunk"))
            {
                foreach (var item in childObj.GetComponent<ChunkClass>().chunkChildList)
                {
                    item.stickablObj.SetActive(true);
                    item.stickablObj.transform.position = ReturnVector(presentLevel.scenePropTools[index].toolPos);
                    item.toolID = (ToolEnum)presentLevel.scenePropTools[index].toolID;
                    item.toolDir = (ToolDirection)presentLevel.scenePropTools[index].toolDirection;
                    GlobalMethod.OperateUIDirection(item.stickablObj, (int)item.toolDir);
                    
                    index++;
                }
                
                childObj.GetComponent<ChunkClass>().InitChunk();
                
            }
        }
    }

    private void ResetBagToolInfo()
    {
        BagManager.Instance.currentBagList.Clear();
        foreach (var item in presentLevel.bagTools)
        {
            BagManager.Instance.currentBagList.Add(new(item.toolID, item.toolDirection));
        }
        
        BagManager.Instance.UpdateAllTools();
    }

    private Vector3 ReturnVector(float[] pos)
    {
        return new Vector3(pos[0], pos[1], pos[2]);
    }
    

    // Update is called once per frame
    void Update()
    {

    }



}
