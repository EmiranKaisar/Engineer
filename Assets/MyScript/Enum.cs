using System;using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ToolEnum
{
    Block,
    Move,
    Rotate,
    Trap, //abandoned
    Star,
    Destination,
    Corpse,
    Attack,
    Killer,
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
    GamePlayHint,
}

public enum DetectorEnum
{
    Bottom,
    Right,
    Upper,
    Left,
}

public enum PlayerAudioEnum
{
    PlayerJump,
    PlayerStick,
    PlayerCollect,
    StarGlow,
}


public enum LevelType
{
    SinglePlayerLegacy,
    MultiPlayerLegacy,
    SinglePlayerCustom,
    MultiPlayerCustom,
}
