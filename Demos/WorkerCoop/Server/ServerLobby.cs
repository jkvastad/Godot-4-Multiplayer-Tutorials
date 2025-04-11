using System.Collections.Generic;
using Godot;

namespace Server;

/// <summary>
/// The server lobby, used to start a server at a desired port with specified number of player seats
/// </summary>
public partial class ServerLobby : Control
{
#nullable disable
    [Export]
    public LobbyNetwork LobbyNetwork { get; set; }
    [Export]
    public LineEdit PortInput { get; set; }
    [Export]
    public Button HostGame { get; set; }
    [Export]
    public LineEdit PlayerSeats { get; set; }
#nullable enable

    public Dictionary<long, string> PlayerNames { get; set; } = []; //peerId key, player name value
    public bool StartServer { get; set; } = false;

    public int TimeoutMS { get; set; }


    public override void _Ready()
    {
        HostGame.Pressed += CreateServer;
        if (StartServer)
            CreateServer();
    }

    private void CreateServer()
    {
        var peer = new ENetMultiplayerPeer();
        // Should probably be some error handling here but in this very simple game I did not encounter any problems which needed it.
        Error error = peer.CreateServer(PortInput.Text.ToInt()); 

        Multiplayer.MultiplayerPeer = peer;

        StartGameSession();
        HostGame.Visible = false;
    }

    private void StartGameSession()
    {
        ServerGameSession gameSession = GD.Load<PackedScene>("res://Server/server_game_session.tscn").Instantiate<ServerGameSession>();
        gameSession.Setup(PlayerSeats.Text.ToInt(), TimeoutMS);
        Main.Singleton.AddChild(gameSession);

        //No QueueFree - Keep server lobby alive for late joining clients
    }
}
