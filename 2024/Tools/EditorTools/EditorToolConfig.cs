# if UNITY_EDITOR
using UnityEngine;

public enum Game 
{
    Arcade,
    Casino
} 

public static class EditorToolConfig
{
    public const string WINDOW_BLASTWORKS_TOOLS_PATH = "Tools/";

    public static readonly Game Game = GetGame();

    private static Game GetGame() 
    {
        return Application.productName.Contains("Casino") ? Game.Casino : Game.Arcade;
    }

    public static bool IsCasino() 
    {
        return Game == Game.Casino;
    }

    public static bool IsArcade() 
    { 
        return Game == Game.Arcade; 
    }
}
#endif
