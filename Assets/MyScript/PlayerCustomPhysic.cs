using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCustomPhysic : MonoBehaviour
{
    private GameObject[] stickableObjList;
    
    [Serializable]
    public class DetectorClass
    {
        public CollideDetector collideDetector;
        public bool collided;
        [HideInInspector]
        public Vector2[] area;

        public GameObject collidedObj;
    }

    [SerializeField] public DetectorClass[] detectorArr;

    public Vector2[] upperArea;

    private Vector2[] clockWiseRot = new Vector2[] { new Vector2(0, 1), new Vector2(-1, 0) };
    
    //private List<int[]> antiClockWiseTri

    private Vector3[] antiClockWiseTri = new Vector3[] { new Vector3(0, 2, 3), new Vector3(1, 2, 3) };

    private Transform playerTransform;

    private PlayerController playerController;

    private bool Asleep = false;
    
    [HideInInspector]
    public Vector3 velocity = new Vector3();

    [HideInInspector] public Vector3 acceleration = new Vector3();
    public float m_Gravity = 20f;
    
    private bool bottomCollided;
    private bool upperCollided;
    private bool rightCollided;
    private bool leftCollided;

    #region Init Setup
    void Start()
    {
        stickableObjList = GameObject.FindGameObjectsWithTag("Stickable");
        playerController = GetComponent<PlayerController>();
        playerTransform = this.transform;
        Asleep = false;
        SetArea();
    }

    public void InitPhysic()
    {
        velocity = new Vector3();
        acceleration = new Vector3();
    }

    private void SetArea()
    {
        detectorArr[0].area = upperArea;
        for (int i = 1; i < detectorArr.Length; i++)
        {
            detectorArr[i].area = AreaMultipliedByMatrix(i);
        }
    }

    private Vector2[] AreaMultipliedByMatrix(int index)
    {
        Vector2[] pos = new Vector2[4];
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = VectorMultipliedByMatrix(detectorArr[index - 1].area[i]);
        }
        

        return pos;
    }

    private Vector2 VectorMultipliedByMatrix(Vector2 pos)
    {
        Vector2 afterRot = Vector2.zero;

        for (int i = 0; i < clockWiseRot.Length; i++)
        {
            afterRot[i] = Vector2.Dot(clockWiseRot[i], pos);
        }


        return afterRot;
    }

    #endregion
    // Start is called before the first frame update


    public bool gravityOn = true;

    public bool collideOn = true;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Asleep)
        {
            DetectStickable();
            SetState();
            ApplyAcceleration();
            if(!bottomCollided && gravityOn)
                ApplyGravity();
            if(collideOn)
                ApplyCollide();
            ApplyVelocity();
            
        }

    }
    

    private void DetectStickable()
    {
        
        for (int i = 0; i < detectorArr.Length; i++)
        {
            bool thisDetectorCollided = false;
            detectorArr[i].collidedObj = null;
            for (int j = 0; j < stickableObjList.Length; j++)
            {
                if (InDetectorTriangle(stickableObjList[j].transform.position, i))
                {
                    thisDetectorCollided = true;
                    detectorArr[i].collidedObj = stickableObjList[j];
                    break;
                }
            }

            detectorArr[i].collided = thisDetectorCollided;
        }
    }
    

    private bool InDetectorTriangle(Vector2 point, int index)
    {
        bool inTriangle1 = InTriangle(point, detectorArr[index].area[(int)antiClockWiseTri[0].x], detectorArr[index].area[(int)antiClockWiseTri[0].y], detectorArr[index].area[(int)antiClockWiseTri[0].z]);
        bool inTriangle2 = InTriangle(point, detectorArr[index].area[(int)antiClockWiseTri[1].x], detectorArr[index].area[(int)antiClockWiseTri[1].y], detectorArr[index].area[(int)antiClockWiseTri[1].z]);
        
        return inTriangle1 || inTriangle2;
    }

    private bool InTriangle(Vector2 detectPoint, Vector2 vert1, Vector2 vert2, Vector2 vert3)
    {
        Vector2 offset = playerTransform.position;
        Vector2 l_1 = vert2 - vert1;
        Vector2 l_2 = vert3 - vert2;
        Vector2 l_3 = vert1 - vert3;

        Vector2 t_1 = detectPoint - (vert2 + offset);
        Vector2 t_2 = detectPoint - (vert3 + offset);
        Vector2 t_3 = detectPoint - (vert1 + offset);

        if (Vector3.Cross(l_1, t_1).z > 0 && Vector3.Cross(l_2, t_2).z > 0 && Vector3.Cross(l_3, t_3).z > 0)
        {
            return true;
        }
        
        return false;
    }

    private void SetState()
    {
        bottomCollided = detectorArr[(int)CollideDetector.Bottom].collided;
        upperCollided = detectorArr[(int)CollideDetector.Upper].collided;
        rightCollided = detectorArr[(int)CollideDetector.Right].collided;
        leftCollided = detectorArr[(int)CollideDetector.Left].collided;
    }


    #region API

    public void WakeUp()
    {
        Asleep = false;
    }

    public void Sleep()
    {
        Asleep = true;
    }
    
    public void AddAcc(Vector3 acc)
    {
        velocity += acc * Time.fixedDeltaTime;
    }
    
    public void ApplyAcceleration()
    {
        velocity += acceleration*Time.fixedDeltaTime;
    }

    public void ApplyVelocity()
    {
        playerTransform.position += velocity*Time.fixedDeltaTime;
    }
    
    public void ApplyGravity()
    {
        velocity -= new Vector3(0, m_Gravity * Time.fixedDeltaTime);
    }

    private void ApplyCollide()
    {
        if (bottomCollided)
        {
            if (velocity.y < 0)
                velocity = new Vector3(velocity.x, 0, 0);
            
            Vector2 offset = playerTransform.position -
                             detectorArr[(int)CollideDetector.Bottom].collidedObj.transform.position;
            float scale = 1 - Vector2.Dot(offset, playerTransform.up);
            playerTransform.position += playerTransform.up * scale;
            // float scale = 1 - Vector2.Dot(offset, detectorArr[(int)CollideDetector.Bottom].collidedObj.transform.up);
            // playerTransform.position += detectorArr[(int)CollideDetector.Bottom].collidedObj.transform.up * scale;
            
        }
        
        if (rightCollided)
        {
            if (velocity.x > 0)
                velocity = new Vector3(0, velocity.y, 0);
            
            Vector2 offset = playerTransform.position -
                             detectorArr[(int)CollideDetector.Right].collidedObj.transform.position;
            float scale = 1 + Vector2.Dot(offset, playerTransform.right);
            playerTransform.position -= playerTransform.right * scale;
            // float scale = 1 + Vector2.Dot(offset, detectorArr[(int)CollideDetector.Right].collidedObj.transform.right);
            // playerTransform.position -= detectorArr[(int)CollideDetector.Right].collidedObj.transform.right * scale;
            
        }
        
        if (leftCollided)
        {
            if (velocity.x < 0)
                velocity = new Vector3(0, velocity.y, 0);
            
            Vector2 offset = playerTransform.position -
                             detectorArr[(int)CollideDetector.Left].collidedObj.transform.position;
            float scale = 1 - Vector2.Dot(offset, playerTransform.right);
            playerTransform.position += playerTransform.right * scale;
            // float scale = 1 - Vector2.Dot(offset, detectorArr[(int)CollideDetector.Left].collidedObj.transform.right);
            // playerTransform.position += detectorArr[(int)CollideDetector.Left].collidedObj.transform.right * scale;
            
        }
        
        if (upperCollided)
        {
            if (velocity.y > 0)
                velocity = new Vector3(velocity.x, 0, 0);
            
            Vector2 offset = playerTransform.position -
                             detectorArr[(int)CollideDetector.Upper].collidedObj.transform.position;
            float scale = 1 + Vector2.Dot(offset, playerTransform.up);
            playerTransform.position -= playerTransform.up * scale;
            // float scale = 1 + Vector2.Dot(offset, detectorArr[(int)CollideDetector.Upper].collidedObj.transform.up);
            // playerTransform.position -= detectorArr[(int)CollideDetector.Upper].collidedObj.transform.up * scale;
            
        }
            
        
        
    }

    #endregion
    
    

    
    

}