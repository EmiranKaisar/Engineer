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
        toolPos = pos;
    }
    public int toolID;
    public int toolDirection;
    public Vector3 toolPos;
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
public class LevelDescription
{
    public Sprite thisImage;
    public string descriptionTitle;
    public string description;
}

[Serializable]
public class LevelInfo
{
    public int levelID = -1;
    public string levelName;
    public List<PlayerSpawn> playerSpawnList = new List<PlayerSpawn>();
    public List<Chunk> sceneChunkList = new List<Chunk>();
    public List<BagTool> bagToolList = new List<BagTool>();
    public int levelType = 0;
}


[Serializable]
public class LevelPreview
{
    public LevelPreview(int id, string name)
    {
        levelID = id;
        levelName = name;
    }
    public int levelID;
    public string levelName;
    public bool hinted = false;
    public int levelType = 0;
}

[Serializable]
public class LevelResult
{
    public bool hinted = false;
    public bool hasPassed = false;
    public float timeDur = 0;
    public int operationCount = 0;
}

[Serializable]
public class Progress
{
    public Progress(int index, string date)
    {
        slot = index;
        lastPlayDate = date;
    }
    public int slot;
    public List<LevelResult> levelResultlist = new List<LevelResult>();
    public string lastPlayDate;
}
