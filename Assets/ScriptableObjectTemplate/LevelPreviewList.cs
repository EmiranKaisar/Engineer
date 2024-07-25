using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelPreviewList", menuName = "ScriptableObject/LevelPreviewList")]
public class LevelPreviewList : ScriptableObject
{
    public List<LevelPreview> previewList;
}
