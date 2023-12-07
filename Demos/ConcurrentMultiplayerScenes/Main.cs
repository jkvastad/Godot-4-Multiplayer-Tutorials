using Godot;
using static GodotUtils.GlobalConstants;

public partial class Main : Node
{
    [Export]
    Lobby _lobby;    
    public override void _Ready()
    {           
        _lobby.LobbyDone += GameStartBootstraper;
    }
    private void GameStartBootstraper(Lobby lobby)
    {        
        lobby.QueueFree();

        if (Multiplayer.IsServer())
        {
            GameStart();
            return;
        }
        Multiplayer.ConnectedToServer += GameStart;
    }

    private void GameStart()
    {
        GetTree().ChangeSceneToPacked(GD.Load<PackedScene>(GAME_SESSION_SCENE_PATH));
    }
}