
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
    Flip,
}

public enum SpriteEnum
{
    Block = 0,
    Move = 1,
    Rotate = 2,
    Trap = 3, //abandoned
    Star = 4,
    Destination = 5,
    Corpse = 6,
    Attack = 7,
    Killer = 8,
    Flip = 9,
    RotateCenter = 10,
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
