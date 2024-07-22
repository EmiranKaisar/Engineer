using System;using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ToolEnum
{
    Block,
    Move,
    Rotate,
    Trap,
    Star,
    Destination,
    Corpse,
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

public enum DetectorEnum
{
    Bottom,
    Right,
    Upper,
    Left,
}


public enum LevelType
{
    Legacy,
    Custom
}
