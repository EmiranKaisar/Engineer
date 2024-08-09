using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        public StickableClass(int id, int dir, GameObject obj)
        {
            toolID = (ToolEnum)id;
            toolDir = (ToolDirection)dir;
            stickablObj = obj;
        }
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

    //the first one is rotate center
    private List<int> rotaterList = new List<int>();

    private List<int> linkList = new List<int>();
    

    // Start is called before the first frame update
    void Start()
    {
        //init parameters
        InitParameters();
        
        InitProp();
        
        //init centre
        InitCentre();
        

    }


    #region Init
    
    public void InitChunk()
    {
        //init parameters
        InitParameters();
        
        InitProp();
        
        //init centre
        InitCentre();
    }
    
    public void InitProp()
    {
        int index = 0;
        foreach (var item in chunkChildList)
        {
            UpdateSprite(index, item.toolID);
            GlobalMethod.OperateUIDirection(item.stickablObj, (int)item.toolDir);
            UpdateStickState(item);
            item.stickablObj.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.8f, 1);
            if (item.toolID == ToolEnum.Killer)
            {
                if(this.gameObject.GetComponent<KillerController>() == null)
                     this.gameObject.AddComponent<KillerController>();
            }

            index++;
        }
    }

    private void InitParameters()
    {
        chunkTransform = this.transform;
        moveSpeed = GlobalParameters.Instance.moveToolSpeed;
        rotateDur = GlobalParameters.Instance.rotateToolDur;
        inRotateProcedure = false;
        rotateTimer = 0;
    }

    private void InitCentre()
    {
        InitRotaterList();
        UpdateCentre();
        MoveChunkToCentre();
        CalculatePresentKinematic();
    }
    
    #endregion

    #region Tools
    
    public bool StickTool(GameObject obj, int playerIndex)
    {
        int index = ReturnIndexByObj(obj);
        BagTool selectedTool = BagManager.Instance.PresentSelectedBagTool();
        if (index >= 0 && !inRotateProcedure && selectedTool.toolID != ToolEnum.Block)
        {
            if (!chunkChildList[index].sticked)
            {
                chunkChildList[index].sticked = true;
                chunkChildList[index].toolID = selectedTool.toolID;
                chunkChildList[index].toolDir = selectedTool.toolDirection;
                
                UpdateSprite(index, selectedTool.toolID);
                GlobalMethod.OperateUIDirection(chunkChildList[index].stickablObj, (int)selectedTool.toolDirection);

                BagManager.Instance.DeleteSelectedTool();
                
                if(selectedTool.toolID == ToolEnum.Rotate)
                    PushRotaterList(index);
                
                if(selectedTool.toolID == ToolEnum.Flip)
                    ApplyFlip(index, selectedTool.toolDirection, playerIndex);

                UpdateStickState(chunkChildList[index]);
                UpdateCentre();
                MoveChunkToCentre();
                CalculatePresentKinematic();
                return true;
            }
            else
            {
                if (chunkChildList[index].toolID == ToolEnum.Destination && selectedTool.toolID == ToolEnum.Star)
                {
                    //put on star collected animation
                    BagManager.Instance.DeleteSelectedTool();
                    AudioManager.Instance.PlayerAudioSourcePlay(playerIndex, PlayerAudioEnum.StarGlow);
                    GameManager.Instance.PlayStarGlowAnimation(chunkChildList[index].stickablObj);
                    GameManager.Instance.SetResult(playerIndex, true);
                    return true;
                }
            }
            
        }

        return false;
    }

    public bool CollectTool(GameObject obj, int playerIndex)
    {
        if (!BagManager.Instance.IsFull())
        {
            int index = ReturnIndexByObj(obj);

            if (index >= 0 && chunkChildList[index].sticked && !inRotateProcedure)
            {
                BagManager.Instance.AddTool(new BagTool(chunkChildList[index].toolID, chunkChildList[index].toolDir));
                UpdateSprite(index, SpriteEnum.Block);
                
                if(chunkChildList[index].toolID == ToolEnum.Rotate)
                    PopRotaterList(index);
                
                if(chunkChildList[index].toolID == ToolEnum.Flip)
                    ApplyFlip(index, chunkChildList[index].toolDir, playerIndex);
                
                chunkChildList[index].sticked = false;
                chunkChildList[index].toolID = ToolEnum.Block;
                chunkChildList[index].toolDir = ToolDirection.Original;
                
                
            
                UpdateStickState(chunkChildList[index]);
                UpdateCentre();
                MoveChunkToCentre();
                CalculatePresentKinematic();
                return true;
            }
        }

        return false;
    }

    private void UpdateSprite(int index, ToolEnum toolID)
    {
        chunkChildList[index].stickablObj.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.ReturnToolSprite((int)toolID);
        
    }

    private void UpdateSprite(int index, SpriteEnum spriteID)
    {
        chunkChildList[index].stickablObj.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.ReturnToolSprite((int)spriteID);
    }
    

    private void UpdateStickState(StickableClass item)
    {
        if (item.toolID == ToolEnum.Block || item.toolID == ToolEnum.Corpse)
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

    private void InitRotaterList()
    {
        rotaterList.Clear();
        for (int i = 0; i < chunkChildList.Count; i++)
        {
            if(chunkChildList[i].toolID == ToolEnum.Rotate)
                PushRotaterList(i);
        }
    }

    private void PushRotaterList(int index)
    {
        rotaterList.Add(index);
        UpdateSprite(rotaterList[0], SpriteEnum.RotateCenter);
    }

    private void PopRotaterList(int index)
    {
        rotaterList.Remove(index);
        if(rotaterList.Count > 0)
            UpdateSprite(rotaterList[0], SpriteEnum.RotateCenter);
    }
    
    private void UpdateCentre()
    {
        if (rotaterList.Count > 0)
            presentCentre = chunkChildList[rotaterList[0]].stickablObj.transform.position;
        else
        {
            presentCentre = chunkChildList[0].stickablObj.transform.position;
        }
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

    public bool inRotateProcedure = false;
    private float rotateTimer = 0;
    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyRotation();
        UpdateAttack();
        
    }

    private void ApplyMovement()
    {
        if (!inRotateProcedure)
            chunkTransform.position += accumulatedMove * Time.fixedDeltaTime;
    }

    private void ApplyRotation()
    {
        if (rotateCount > 0)
        {
            rotateTimer += Time.fixedDeltaTime;
            if (rotateTimer >= accumulatedRotDur)
            {
                rotateTimer = 0;
                if (rotateProcedure != null)
                {
                    StopCoroutine(rotateProcedure);
                    chunkTransform.rotation *= Quaternion.identity;
                }
                rotateProcedure = RotateProcedure(accumulatedRotDur * 0.2f);
                StartCoroutine(rotateProcedure);
            }
        }
        else
        {
            rotateTimer = 0;
        }
    }

    private GameObject tempPlayerObj;
    private void ApplyFlip(int index, ToolDirection dir, int playerIndex)
    {
        presentCentre = chunkChildList[index].stickablObj.transform.position;
        MoveChunkToCentre();
        tempPlayerObj = GameManager.Instance.playerList[playerIndex];
        tempPlayerObj.transform.SetParent(chunkTransform);
        if (dir == ToolDirection.Original || dir == ToolDirection.Left)
        {
            //rotate 180 around Y
            chunkTransform.rotation *= Quaternion.AngleAxis(180, Vector3.up);
            tempPlayerObj.transform.rotation *= Quaternion.AngleAxis(180, Vector3.up);
        }
        else
        {
            //rotate 180 around X
            chunkTransform.rotation *= Quaternion.AngleAxis(180, Vector3.right);
        }
        
        tempPlayerObj.transform.SetParent(GlobalParameters.Instance.playerObjs.transform);
        tempPlayerObj.GetComponent<PlayerController>().FlipByChunk(dir);
        
        
        foreach (var item in chunkChildList)
        {
            UpdateToolDirection(item);
        }
        
    }
    
    #endregion

    #region Attack functionality
    private void UpdateAttack()
    {
        if (!inRotateProcedure)
        {
            for (int i = 0; i < chunkChildList.Count; i++)
            {
                if(chunkChildList[i].toolID == ToolEnum.Attack)
                    ApplyAttack(i);
            }
        }

    }

    private float attackDistance = .2f;
    private RaycastHit2D attackHit;
    private Vector3 attackDir;
    private List<Vector3> attackOriginList = new List<Vector3>()
    {
        new Vector3(0.52f, 0.45f, 0),
        new Vector3(0.52f, -0.45f, 0)
    };

    private GameObject upper;
    private GameObject lower;
    private Vector3 attackOrigin;
    private void ApplyAttack(int index)
    {
        attackDir = chunkChildList[index].stickablObj.transform.right;
        int posIndex= 0;
        foreach (var attack in attackOriginList)
        {
            attackOrigin = chunkChildList[index].stickablObj.transform.TransformPoint(attack);
            attackHit = Physics2D.Raycast(attackOrigin, attackDir, attackDistance);
            if (attackHit.collider != null)
            {
                attackHit.collider.GetComponent<IAlive>()?.GotAttacked();
                attackHit.collider.GetComponentInParent<IAlive>()?.GotAttacked();
            }

            posIndex++;
        }
    }
    

    #endregion
    
    
    #region Coroutine
    
    private IEnumerator rotateProcedure;
    private WaitForSeconds rotateDelta = new WaitForSeconds(0.01f);
    private float rotateDeltaFloat = 0.01f;
    private IEnumerator RotateProcedure(float procedureDur)
    {
        //在此过程中，停止移动
        inRotateProcedure = true;
        float timer = 0;
        Quaternion presentRot = chunkTransform.rotation;
        Quaternion targetRot = presentRot * Quaternion.AngleAxis(90, Vector3.forward);
        while (timer <= procedureDur)
        {
            timer += rotateDeltaFloat;
            chunkTransform.rotation = Quaternion.Lerp(presentRot, targetRot, timer / procedureDur);
            yield return rotateDelta;
        }

        UpdateAfterRotation();

        inRotateProcedure = false;
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