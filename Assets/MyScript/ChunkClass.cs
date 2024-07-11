using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class ChunkClass : MonoBehaviour
{
    //将每个链接的物体看成一个chunk
    //chunk管理贴在本chunk上的sticker，
    //通过子集和父亲的相对移动来调整中心位置
    //chunk的速度应该在gameManager中确定

    public float moveSpeed = 1;
    public float rotateSpeed = 1;
    
    
    [Serializable]
    public class StickableClass
    {
        public bool sticked = false;
        public int toolID;
        public int toolDir;
        public int originalSpriteID;
        public int originalSpriteDir;
        public GameObject stickablObj;
    }
    
    [SerializeField]
    public List<StickableClass> chunkChildList = new List<StickableClass>();

    private Transform chunkTransform;

    private List<Vector3> relevantPos = new List<Vector3>();
    
    //maintain a list for rotater, first rotater should be the centre
    private List<int> rotaterIndexList = new List<int>();

    private Vector3 presentCentre;

    private bool ifSticked = false;
    
    // Start is called before the first frame update
    void Start()
    {
        //init centre
        ifSticked = false;
        chunkTransform = this.transform;
        InitCentre();
    }

    private void InitCentre()
    {
        UpdateRelevantPos();
        UpdateCentre();
        MoveChunkToCentre();
        UpdateIfChunkSticked();
        if(ifSticked)
           CalculatePresentKinematic();
    }

    private void UpdateRelevantPos()
    {
        relevantPos.Clear();
        int rotaterCount = 0;
        foreach (var item in chunkChildList)
        {
            if (item.toolID == 1)
            {
                rotaterCount++;
                relevantPos.Add(item.stickablObj.transform.position);
            }
        }

        if (rotaterCount <= 0)
        {
            foreach (var item in chunkChildList)
            {
                relevantPos.Add(item.stickablObj.transform.position);
            }
        }
    }

    private void UpdateCentre()
    {
        Vector3 centre = Vector3.zero;
        foreach (var pos in relevantPos)
        {
            centre += pos;
        }

        presentCentre = centre / relevantPos.Count;
    }

    private void MoveChunkToCentre()
    {

        Vector3 offset = chunkTransform.position - presentCentre;
        
        foreach (var item in chunkChildList)
        {
            item.stickablObj.transform.position += offset;
        }

        chunkTransform.position -= offset;
    }

    public void StickTool(GameObject obj)
    {
        int index = ReturnIndexByObj(obj);
        BagTool selectedTool = BagManager.Instance.PresentSelectedBagTool();
        if (index >= 0 && !chunkChildList[index].sticked && selectedTool.toolID >= 0)
        {
            chunkChildList[index].sticked = true;
            chunkChildList[index].toolID = selectedTool.toolID;
            chunkChildList[index].toolDir = selectedTool.toolDirection;
            obj.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.ReturnToolSprite(selectedTool.toolID);
            GameManager.Instance.OperateUIDirection(obj, selectedTool.toolDirection);
            
            BagManager.Instance.DeleteSelectedTool();
            UpdateRelevantPos();
            UpdateCentre();
            MoveChunkToCentre();
            CalculatePresentKinematic();
        }
        
    }

    public void CollectTool(GameObject obj)
    {
        int index = ReturnIndexByObj(obj);
        
        if (index >= 0 && chunkChildList[index].sticked)
        {
            BagManager.Instance.AddTool(new BagTool(chunkChildList[index].toolID, chunkChildList[index].toolDir));
            chunkChildList[index].sticked = false;
            chunkChildList[index].toolID = -1;
            chunkChildList[index].toolDir = -1;
            obj.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.ReturnToolSprite(chunkChildList[index].originalSpriteID);
            GameManager.Instance.OperateUIDirection(obj, chunkChildList[index].originalSpriteDir);
            
            UpdateRelevantPos();
            UpdateCentre();
            MoveChunkToCentre();
            UpdateIfChunkSticked();
            CalculatePresentKinematic();
        }
        
        
    }

    private void UpdateIfChunkSticked()
    {
        ifSticked = false;
        foreach (var item in chunkChildList)
        {
            if (item.sticked)
                ifSticked = true;
        }
    }


    public Vector3 accumulatedMove = Vector3.zero;
    private float accumulatedRot = 0;
    private void CalculatePresentKinematic()
    {
        accumulatedMove = Vector3.zero;
        accumulatedRot = 0;
        foreach (var item in chunkChildList)
        {
            if (item.toolID == 0)
                accumulatedMove += MovementByDir(item.toolDir)*moveSpeed;

            if (item.toolID == 1)
                accumulatedRot += RotByDir(item.toolDir);
        }
    }

    private Vector3 MovementByDir(int dir)
    {
        Vector3 movement = Vector3.zero;
        if (dir == 0)
        {
            movement = new Vector3(1, 0, 0);
        }else if (dir == 1)
        {
            movement = new Vector3(-1, 0, 0);
        }else if (dir == 2)
        {
            movement = new Vector3(0, 1, 0);
        }else if (dir == 3)
        {
            movement = new Vector3(0, -1, 0);
        }

        return movement;
    }

    private float RotByDir(int dir)
    {
        float rot = 0;

        if (dir == 0)
        {
            rot = rotateSpeed;
        }else if (dir == 1)
        {
            rot = -rotateSpeed;
        }

        return rot;
    }
    
    

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        chunkTransform.position += accumulatedMove * Time.fixedDeltaTime;
        chunkTransform.rotation *= Quaternion.AngleAxis(accumulatedRot * Time.fixedDeltaTime, Vector3.forward);
    }

    #region API

    public int ReturnIndexByObj(GameObject obj)
    {
        int index = -1;
        for (int i = 0; i < chunkChildList.Count; i++)
        {
            if (chunkChildList[i].stickablObj == obj)
                index = i;
        }

        return index;
    }
    

    #endregion
}
