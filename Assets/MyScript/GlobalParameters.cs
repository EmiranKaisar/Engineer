using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GlobalParameters : MonoBehaviour
{
    //for now, only save the current level info
    //the basics are stickable obj
    //there are 3 types of tool: mover, rotater, killer
    //there are instantiate points for players
    //flag points, flag target points
    
    public float rotateToolDur = 2;

    public float moveToolSpeed = 1;

    public float verticalBound = 7.5f;
    public float horizontalBound = 13.72f;

    public GameObject playerObjs;
    
    public GameObject levelObjs;
    public GameObject levelEditorObjs;
    

    public LevelTemplate presentLevel;

    private int orderInLayer = 0;
    private bool editMode = false;
    
    public class ChunkObjPool
    {
        private int pointer = 0;
        private List<GameObject> chunkObjPool = new List<GameObject>();
        private GameObject levelObjs;
        
        public void InitChunkPool(int initCount, GameObject parentObj)
        {
            levelObjs = parentObj;
            pointer = 0;
            for (int i = 0; i < initCount; i++)
            {
                chunkObjPool.Add(NewChunk(i));
                chunkObjPool[i].SetActive(false);
            }
        }


        public void PushAll()
        {
            for (int i = pointer; i > 0; i--)
            {
                Push();
            }
        }

        public void Push()
        {
            pointer--;
            chunkObjPool[pointer].SetActive(false);
        }

        public GameObject Pop()
        {
            if (pointer >= chunkObjPool.Count)
            {
                //create new chunk
                chunkObjPool.Add(NewChunk(pointer));
                return Pop();
            }
            else
            {
                int popIndex = pointer;
                chunkObjPool[popIndex].SetActive(true);
                pointer++;
                return chunkObjPool[popIndex];
            }
        }

        private GameObject NewChunk(int index)
        {
            GameObject newChunk = new GameObject("Chunk "+ index);
            newChunk.tag = "Chunk";
            newChunk.layer = LayerMask.NameToLayer("StickableObj");
            newChunk.AddComponent<ChunkClass>();
            newChunk.transform.SetParent(levelObjs.transform);
            return newChunk;
        }
        
        

        public int Count()
        {
            return chunkObjPool.Count;
        }
    }
    
    public class StickableObjPool
    {
        private int pointer = 0;
        private List<GameObject> stickableObjPool = new List<GameObject>();
        private GameObject levelObjs;
        public void InitStickablePool(int initCount, GameObject parentObj)
        {
            levelObjs = parentObj;
            pointer = 0;
            for (int i = 0; i < initCount; i++)
            {
                stickableObjPool.Add(NewStickable(i));
                stickableObjPool[i].SetActive(false);
            }
        }
        
        public void PushAll()
        {
            for (int i = pointer; i > 0; i--)
            {
                Push();
            }
        }

        public void Push()
        {
            pointer--;
            stickableObjPool[pointer].SetActive(false);
        }

        public GameObject Pop()
        {
            if (pointer >= stickableObjPool.Count)
            {
                //create new chunk
                stickableObjPool.Add(NewStickable(pointer));
                return Pop();
            }
            else
            {
                int popIndex = pointer;
                stickableObjPool[popIndex].SetActive(true);
                pointer++;
                return stickableObjPool[popIndex];
            }
        }

        private GameObject NewStickable(int index)
        {
            GameObject newStickable = new GameObject("Stickable "+ index);
            newStickable.tag = "Stickable";
            newStickable.layer = LayerMask.NameToLayer("StickableObj");
            newStickable.AddComponent<SpriteRenderer>();
            newStickable.AddComponent<BoxCollider2D>();
            newStickable.GetComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.9f);
            newStickable.GetComponent<BoxCollider2D>().edgeRadius = 0.05f;
            newStickable.transform.SetParent(levelObjs.transform);
            return newStickable;
        }

        public int Count()
        {
            return stickableObjPool.Count;
        }
    }
    
    
    ChunkObjPool chunkObjPool = new ChunkObjPool();
    StickableObjPool stickableObjPool = new StickableObjPool();

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
        
        chunkObjPool.InitChunkPool(10, levelObjs);
        stickableObjPool.InitStickablePool(20, levelObjs);
    }

    private void Update()
    {
        if (editMode)
        {
            foreach (Transform obj in levelEditorObjs.transform)
            {
                if (obj.CompareTag("Chunk") &&  obj.gameObject.activeSelf)
                {
                    obj.GetComponent<ChunkClass>().InitProp();
                }
            }
        }
    }

    public bool LoadLevel(string levelName)
    {
        presentLevel = Resources.Load("Level/" + levelName) as LevelTemplate;
        if (presentLevel != null)
        {
            return true;
        }
        
        Debug.LogError("there is no such level");
        return false;
    }
    

    #region Get scene info into presentLevel
    
    public void GetInfo()
    {
        //get player spawn list
        GetPlayerSpawnPosList();
        
        //get all other objs info
        GetPropInfo();
        
        EditorUtility.SetDirty(presentLevel);
    }
    

    private void GetPlayerSpawnPosList()
    {
        presentLevel.playerSpawnList.Clear();
        for (int i = 0; i < playerObjs.transform.childCount; i++)
        {
            presentLevel.playerSpawnList.Add(playerObjs.transform.GetChild(i).position);
        }
    }

    
    private void GetPropInfo()
    {
        presentLevel.sceneChunkList.Clear();

        int index = 0;

        foreach (Transform obj in levelEditorObjs.transform)
        {
            if (obj.CompareTag("Chunk") &&  obj.gameObject.activeSelf)
            {
                presentLevel.sceneChunkList.Add(new Chunk());
                foreach (var item in obj.GetComponent<ChunkClass>().chunkChildList)
                {
                    presentLevel.sceneChunkList[index].chunkPropList.Add(new PropTool((int)item.toolID, (int)item.toolDir, item.stickablObj.transform.position));
                }

                index++;
            }
        }
    }
    

    #endregion



    #region Get presentLevel into scene
    //call this after this level is set up
    public void ResetLevel()
    {
        orderInLayer = 0;
        
        SetPropInfo();
        SetPlayerInfo();
        SetBagToolInfo();
    }

    private void SetPlayerInfo()
    {
        int index = 0;

        foreach (var playerSpawn in presentLevel.playerSpawnList)
        {
            if (index < GameManager.Instance.playerList.Count)
            {
                GameManager.Instance.playerList[index].transform.position = playerSpawn;
                GameManager.Instance.playerList[index].GetComponent<SpriteRenderer>().sortingOrder = orderInLayer;
                orderInLayer++;
            }
            else
            {
                break;
            }
               
            
            index++;
        }
    }

    private void SetPropInfo()
    {
        
        chunkObjPool.PushAll();
        stickableObjPool.PushAll();
        
        foreach (var chunkStruct in presentLevel.sceneChunkList)
        {
            
            GameObject chunk = chunkObjPool.Pop();
            ChunkClass chunkClass = chunk.GetComponent<ChunkClass>();
            chunkClass.chunkChildList.Clear();
            chunk.transform.rotation = Quaternion.identity;
            foreach (var toolStruct in chunkStruct.chunkPropList)
            {
                GameObject toolObj = stickableObjPool.Pop();
                toolObj.transform.position = toolStruct.toolPos;
                toolObj.transform.SetParent(chunk.transform);
                chunkClass.chunkChildList.Add(new ChunkClass.StickableClass(toolStruct.toolID, toolStruct.toolDirection, toolObj));
                chunkClass.InitChunk();
                GlobalMethod.OperateUIDirection(toolObj, toolStruct.toolDirection);
                toolObj.GetComponent<SpriteRenderer>().sortingOrder = orderInLayer;
            }

            orderInLayer++;
        }
    }

    private void SetBagToolInfo()
    {
        BagManager.Instance.currentBagList.Clear();
        foreach (var item in presentLevel.bagToolList)
        {
            BagManager.Instance.currentBagList.Add(new(item.toolID, item.toolDirection));
        }
        
        BagManager.Instance.InitBag();
    }
    
    

    #endregion

    public void EditMode(bool edit)
    {
        editMode = edit;
        if (edit)
        {
            levelEditorObjs.SetActive(true);
            levelObjs.SetActive(false);
        }
        else
        {
            levelEditorObjs.SetActive(false);
            levelObjs.SetActive(true);
        }
    }
    


    
    



}
