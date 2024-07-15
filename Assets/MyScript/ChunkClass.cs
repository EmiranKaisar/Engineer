using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class ChunkClass : MonoBehaviour
{
    //将每个链接的物体看成一个chunk
    //chunk管理贴在本chunk上的sticker，
    //通过子集和父亲的相对移动来调整中心位置
    //chunk的速度应该在gameManager中确定

    public float moveSpeed = 1;
    public float rotateSpeed = 1;
    public float rotateDur = 2;


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

    [SerializeField] public List<StickableClass> chunkChildList = new List<StickableClass>();

    private Transform chunkTransform;

    private List<Vector3> relevantPos = new List<Vector3>();

    private Vector3 presentCentre;

    private bool ifSticked = false;

    private int rotateCount = 0;

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
        if (ifSticked)
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
        if (index >= 0 && !chunkChildList[index].sticked && selectedTool.toolID >= 0 && !inRotateProcedure)
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

        if (index >= 0 && chunkChildList[index].sticked && !inRotateProcedure)
        {
            BagManager.Instance.AddTool(new BagTool(chunkChildList[index].toolID, chunkChildList[index].toolDir));
            chunkChildList[index].sticked = false;
            chunkChildList[index].toolID = -1;
            chunkChildList[index].toolDir = -1;
            obj.GetComponent<SpriteRenderer>().sprite =
                SpriteManager.Instance.ReturnToolSprite(chunkChildList[index].originalSpriteID);
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
    private float accumulatedRotDur;

    private void CalculatePresentKinematic()
    {
        accumulatedMove = Vector3.zero;
        accumulatedRotDur = 2 * rotateDur;
        rotateCount = 0;
        foreach (var item in chunkChildList)
        {
            if (item.toolID == 0)
            {
                accumulatedMove += item.stickablObj.transform.right * moveSpeed;
            }


            if (item.toolID == 1)
            {
                rotateCount++;
                accumulatedRotDur *= 0.5f;
            }
        }
    }

    private void UpdateAfterRotation()
    {
        accumulatedMove = Vector3.zero;
        foreach (var item in chunkChildList)
        {
            if (item.toolID == 0)
            {
                accumulatedMove += item.stickablObj.transform.right * moveSpeed;
                UpdateToolDirection(item);
            }
        }
    }

    private Vector3[] compareAxis = new[]
        { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(-1, 0, 0), new Vector3(0, -1, 0) };

    private Vector3 flipCompareAxis = new Vector3(0, 0, 1);
    private void UpdateToolDirection(StickableClass item)
    {
        if (item.toolID == 1)
        {
            if (Vector3.Dot(item.stickablObj.transform.forward, flipCompareAxis) >= 1)
            {
                item.toolDir = (int) ToolDirection.Original;
            }
            else
            {
                item.toolDir = (int) ToolDirection.Flip;
            }
        }
        else
        {
            for (int i = 0; i < compareAxis.Length; i++)
            {
                if (Vector3.Dot(item.stickablObj.transform.right, compareAxis[i]) >= 1)
                    item.toolDir = i;
            }
        }
    }

    private bool inRotateProcedure = false;
    private float rotateTimer = 0;
    private IEnumerator rotateProcedure;

    private void FixedUpdate()
    {
        if (!inRotateProcedure)
            chunkTransform.position += accumulatedMove * Time.fixedDeltaTime;

        if (rotateCount > 0)
        {
            rotateTimer += Time.fixedDeltaTime;
            if (rotateTimer >= accumulatedRotDur)
            {
                rotateTimer = 0;
                if (rotateProcedure != null)
                    StopCoroutine(rotateProcedure);
                rotateProcedure = RotateProcedure(accumulatedRotDur * 0.8f);
                StartCoroutine(rotateProcedure);
            }
        }
        else
        {
            rotateTimer = 0;
        }
    }

    private IEnumerator RotateProcedure(float procedureDur)
    {
        //在此过程中，停止移动
        inRotateProcedure = true;
        float timer = 0;
        Quaternion presentRot = chunkTransform.rotation;
        Quaternion targetRot = presentRot * Quaternion.AngleAxis(90, Vector3.forward);
        while (timer <= procedureDur)
        {
            timer += Time.fixedDeltaTime;
            chunkTransform.rotation = Quaternion.Lerp(presentRot, targetRot, timer / procedureDur);
            yield return null;
        }

        UpdateAfterRotation();

        inRotateProcedure = false;
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