using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    [Serializable]
    public class ToolSprite
    {
        public int toolID;
        public Sprite toolSprite;
    }

    private Sprite tempSprite;
    
    [SerializeField]
    public List<ToolSprite> toolSpriteList = new List<ToolSprite>();
    

    public static SpriteManager Instance
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
        
        DontDestroyOnLoad(this);
    }

    public Sprite ReturnToolSprite(int id)
    {
        int index = SpriteIndex(id);
        if (index >= 0)
        {
            tempSprite = toolSpriteList[index].toolSprite;
            return tempSprite;
        }
        return null;
    }

    private int SpriteIndex(int id)
    {
        int index = -1;
        for (int i = 0; i < toolSpriteList.Count; i++)
        {
            if (toolSpriteList[i].toolID == id)
                index = i;

        }

        return index;
    }
    
    
}
