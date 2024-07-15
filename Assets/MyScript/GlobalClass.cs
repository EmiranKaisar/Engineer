using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public struct BagTool
{
    public BagTool(int id, int dir)
    {
        toolID = id;
        toolDirection = dir;
    }
    public int toolID;
    public int toolDirection;
}

public struct PropTool
{
    public int toolID;
    public int toolDirection;
    public float[] toolPos;
}

public class LevelInfo
{
    public int levelID;
    public float[] playerInstantiatePos;
    public List<PropTool> scenePropTools;
    public int levelType;
}
