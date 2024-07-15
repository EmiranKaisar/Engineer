using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GlobalParameters : MonoBehaviour
{
    public int presentLevelID = 0;
    
    public float rotateToolDur = 2;

    public float moveToolSpeed = 1;

    public bool editMode = false;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        if (editMode)
        {
            //present editor UI
            
            //1. At first, one is presented with the list of existing levels, and can create a new one. 
            //2. In either way, when entered in a level, there are a side bar for 3 types of props: Block, tool, Start/End Point.
            //3. When clicked one of the props, one can place the prop in the grid of the scene just like tilemap in unity
            //4. There is a default size of the scene, however when props are located outside, the size of the scene is decided by the region surrounding all props.
            //5. Can select a tile and delete it with a delete button
            //6. Can save the level
            
            
        }
        else
        {
            //close editor UI
            
        }
    }

    public void SaveLevel(int index)
    {
        
    }

    public void LoadLevel(int index)
    {
        
    }

}
