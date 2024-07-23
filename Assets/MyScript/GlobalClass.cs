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

[Serializable]
public class Chunk
{
    public List<PropTool> chunkPropList = new List<PropTool>();
}

[Serializable]
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

[Serializable]
public struct PlayerSpawn
{
    public PlayerSpawn(Vector3 pos)
    {
        spawnPos = new[] { pos.x, pos.y, pos.z };
    }
    public float[] spawnPos;
}
[Serializable]
public class LevelInfo
{
    public int levelID;
    public List<PlayerSpawn> playerSpawnList = new List<PlayerSpawn>();
    public List<Chunk> sceneChunkList = new List<Chunk>();
    public List<BagTool> bagToolList = new List<BagTool>();
    public int levelType;
}

public class LevelViewInfo
{
    public string name;
}

public struct LevelResult
{
    public LevelResult(int index, bool success, float dur)
    {
        playerIndex = index;
        playerSuccess = success;
        timeDur = dur;
    }
    public int playerIndex;
    public bool playerSuccess;
    public float timeDur;
}
