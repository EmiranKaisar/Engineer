using System;
using UnityEngine;

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
    public int originalID;
    public int originalDir;
    public Vector3 toolPos;
}
