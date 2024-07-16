using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public struct BagTool
{
    public BagTool(ToolEnum id, ToolDirection dir)
    {
        toolID = id;
        toolDirection = dir;
    }
    public ToolEnum toolID;
    public ToolDirection toolDirection;
}

public struct PropTool
{
    public PropTool(int id, int dir, Vector3 pos)
    {
        toolID = id;
        toolDirection = dir;
        toolPos = new[] { pos.x, pos.y, pos.z };
    }
    public int toolID;
    public int toolDirection;
    public float[] toolPos;
}

public class LevelInfo
{
    public int levelID;
    public float[] playerInstantiatePos = new float[3];
    public List<PropTool> scenePropTools = new List<PropTool>();
    public int levelType;
}
