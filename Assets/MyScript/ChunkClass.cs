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

    private float moveSpeed = 1;
    private float rotateDur = 2;


    [Serializable]
    public class StickableClass
    {
        public bool sticked = false;
        public ToolEnum toolID;
        public ToolDirection toolDir;
        public GameObject stickablObj;
    }

    [SerializeField] public List<StickableClass> chunkChildList = new List<StickableClass>();

    private Transform chunkTransform;

    private List<Vector3> relevantPos = new List<Vector3>();

    private Vector3 presentCentre;

    private int rotateCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        InitProp();
        //init parameters
        InitParameters();
        //init centre
        InitCentre();
    }

    public void InitChunk()
    {
        InitProp();
        //init parameters
        InitParameters();
        //init centre
        InitCentre();
    }

    private void InitProp()
    {
        chunkTransform = this.transform;
        foreach (var item in chunkChildList)
        {
            item.stickablObj.GetComponent<SpriteRenderer>().sprite =
                SpriteManager.Instance.ReturnToolSprite((int)item.toolID);
            
            UpdateStickState(item);
        }
    }

    private void InitParameters()
    {
        moveSpeed = GlobalParameters.Instance.moveToolSpeed;
        rotateDur = GlobalParameters.Instance.rotateToolDur;
    }

    private void InitCentre()
    {
        UpdateRelevantPos();
        UpdateCentre();
        MoveChunkToCentre();
        CalculatePresentKinematic();
    }


    #region Tools
    public bool StickTool(GameObject obj)
    {
        int index = ReturnIndexByObj(obj);
        return StickToolByIndex(index);
    }

    private bool StickToolByIndex(int index)
    {
        BagTool selectedTool = BagManager.Instance.PresentSelectedBagTool();
        
        if (index >= 0 && !inRotateProcedure)
        {
            if (!chunkChildList[index].sticked)
            {
                chunkChildList[index].sticked = true;
                chunkChildList[index].toolID = selectedTool.toolID;
                chunkChildList[index].toolDir = selectedTool.toolDirection;
                UpdateSprite(index, selectedTool.toolID);
                //chunkChildList[index].stickablObj.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.ReturnToolSprite((int)selectedTool.toolID);

                GlobalMethod.OperateUIDirection(chunkChildList[index].stickablObj, (int)selectedTool.toolDirection);

                BagManager.Instance.DeleteSelectedTool();
                UpdateStickState(chunkChildList[index]);
                UpdateRelevantPos();
                UpdateCentre();
                MoveChunkToCentre();
                CalculatePresentKinematic();
            }
            else
            {
                if (chunkChildList[index].toolID == ToolEnum.Destination && selectedTool.toolID == ToolEnum.Star)
                {
                    //put on star collected animation
                    GameManager.Instance.PlayStarGlowAnimation(chunkChildList[index].stickablObj.transform.position);
                    return true;
                }
            }
            
        }

        return false;
    }

    public void CollectTool(GameObject obj)
    {
        if (!BagManager.Instance.IsFull())
        {
            int index = ReturnIndexByObj(obj);

            if (index >= 0 && chunkChildList[index].sticked && !inRotateProcedure)
            {
                BagManager.Instance.AddTool(new BagTool(chunkChildList[index].toolID, chunkChildList[index].toolDir));
                chunkChildList[index].sticked = false;
                chunkChildList[index].toolID = ToolEnum.Block;
                chunkChildList[index].toolDir = ToolDirection.Original;
                UpdateSprite(index, ToolEnum.Block);
            
                UpdateStickState(chunkChildList[index]);
                UpdateRelevantPos();
                UpdateCentre();
                MoveChunkToCentre();
                CalculatePresentKinematic();
            }
        }

    }

    private void UpdateSprite(int index, ToolEnum toolID)
    {
        chunkChildList[index].stickablObj.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.ReturnToolSprite((int)toolID);
    }

    public void CheckTrap(GameObject obj, GameObject player)
    {
        int index = ReturnIndexByObj(obj);
        if (index >= 0 && !inRotateProcedure)
        {
            if (chunkChildList[index].toolID == ToolEnum.Trap)
            {
                TrapFunction(index, player);
            }
        }
    }

    private void TrapFunction(int index, GameObject player)
    {
        if (BagManager.Instance.currentBagList.Count > 0)
        {
            StickToolByIndex(index);
        }
        else
        {
            StickPlayer(index, player);
        }
    }
    
    private void StickPlayer(int index, GameObject player)
    {
        chunkChildList[index].sticked = false;
        chunkChildList[index].toolID = ToolEnum.Corpse;
        chunkChildList[index].toolDir = ToolDirection.Original;
        if(stickPlayerAnim != null)
            StopCoroutine(stickPlayerAnim);
        stickPlayerAnim = StickPlayerAnim(player, index);
        StartCoroutine(stickPlayerAnim);
    }

    private void UpdateStickState(StickableClass item)
    {
        if (item.toolID == ToolEnum.Block || item.toolID == ToolEnum.Trap || item.toolID == ToolEnum.Corpse)
        {
            item.sticked = false;
        }
        else
        {
            item.sticked = true;
        }
    }

    #endregion
    
    #region Chunk motion
    private void UpdateRelevantPos()
    {
        relevantPos.Clear();
        int rotaterCount = 0;
        foreach (var item in chunkChildList)
        {
            if (item.toolID == ToolEnum.Rotate)
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
    
    [HideInInspector]
    public Vector3 accumulatedMove = Vector3.zero;
    private float accumulatedRotDur;

    private void CalculatePresentKinematic()
    {
        accumulatedMove = Vector3.zero;
        accumulatedRotDur = 2 * rotateDur;
        rotateCount = 0;
        foreach (var item in chunkChildList)
        {
            if (item.toolID == ToolEnum.Move)
            {
                accumulatedMove += item.stickablObj.transform.right * moveSpeed;
            }


            if (item.toolID == ToolEnum.Rotate)
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
            if (item.toolID == ToolEnum.Move)
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
        if (item.toolID == ToolEnum.Rotate)
        {
            if (Vector3.Dot(item.stickablObj.transform.forward, flipCompareAxis) >= 1)
            {
                item.toolDir = ToolDirection.Original;
            }
            else
            {
                item.toolDir = ToolDirection.Flip;
            }
        }
        else
        {
            for (int i = 0; i < compareAxis.Length; i++)
            {
                if (Vector3.Dot(item.stickablObj.transform.right, compareAxis[i]) >= 1)
                    item.toolDir = (ToolDirection)i;
            }
        }
    }

    private bool inRotateProcedure = false;
    private float rotateTimer = 0;
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
    
    #endregion
    
    #region Coroutine
    
    private IEnumerator rotateProcedure;
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
    
    private float stickAnimDur = 0.2f;
    private IEnumerator stickPlayerAnim;
    private IEnumerator StickPlayerAnim(GameObject playerObj, int index)
    {
        Vector3 startPos = playerObj.transform.position;
        
        float timer = 0;
        while (timer <= stickAnimDur)
        {
            timer += Time.deltaTime;
            playerObj.transform.position = Vector3.Lerp(startPos, chunkChildList[index].stickablObj.transform.position, timer/stickAnimDur);
            yield return null;
        }
        
        playerObj.SetActive(false);


        UpdateSprite(index, ToolEnum.Corpse);

        yield return new WaitForSeconds(0.4f);
        GameManager.Instance.SetResult(playerObj.GetComponent<PlayerController>().playerIndex, false);
    }

    #endregion

    #region Helper

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