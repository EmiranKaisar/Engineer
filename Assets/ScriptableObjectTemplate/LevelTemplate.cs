using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelTemplate", menuName = "ScriptableObject/LevelTemplate")]
public class LevelTemplate : ScriptableObject
{
    public int levelID = -1;
    public string levelName;
    public List<Vector3> playerSpawnList = new List<Vector3>();
    public List<Chunk> sceneChunkList = new List<Chunk>();
    public List<BagTool> bagToolList = new List<BagTool>();
    public int levelType = 0;
}
