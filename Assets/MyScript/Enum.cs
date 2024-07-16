using System;using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ToolEnum
{
    Move,
    Rotate,
}

public enum ToolDirection
{
    Original,
    Up,
    Left,
    Down,
    Flip
}

public enum StateEnum
{
    Home,
    ChoosePlayer,
    ChooseLevel,
    GamePlay,
    GamePlayPause,
    ChooseEditorLevel,
    MapEditor,
    MapEditorPause,
}

public enum LevelType
{
    Legacy,
    Custom
}
